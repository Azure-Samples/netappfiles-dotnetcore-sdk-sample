// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ANF.Samples.Common;
    using Microsoft.Azure.Management.ANF.Samples.Model;
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using static Microsoft.Azure.Management.ANF.Samples.Common.Utils;
    using static Microsoft.Azure.Management.ANF.Samples.Common.Sdk.CommonSdk;
    using System.Collections;
    using System.Runtime.CompilerServices;

    public static class Creation
    {
        /// <summary>
        /// Executes basic CRUD operations using Azure NetApp files SDK
        /// </summary>
        /// <returns></returns>
        public static async Task RunCreationSampleAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient anfClient)
        {
            //
            // Creating ANF Accounts
            //
            Utils.WriteConsoleMessage("Creating Azure NetApp Files accounts ...");
            if (config.Accounts.Count == 0)
            {
                Utils.WriteConsoleMessage("No ANF accounts defined within appsettings.json file, exiting.");
                return;
            }
            else
            {
                List<Task<NetAppAccount>> accountTasks = config.Accounts.Select(
                    async anfAcct => await CreateOrRetrieveAccountAsync(config, anfClient, anfAcct)).ToList();

                await WaitForTasksCompletion<NetAppAccount>(accountTasks).ConfigureAwait(false);
            }

            //
            // Creating Capacity Pools
            //
            Utils.WriteConsoleMessage("Creating Capacity Pools...");
            List<Task<CapacityPool>> poolTasks = new List<Task<CapacityPool>>();
            foreach (ModelNetAppAccount anfAcct in config.Accounts)
            {
                if (anfAcct.CapacityPools != null)
                {
                    poolTasks.AddRange(anfAcct.CapacityPools.Select(
                        async pool => await CreateOrRetrieveCapacityPoolAsync(anfClient, config.ResourceGroup, anfAcct, pool)).ToList());
                }
                else
                {
                    Utils.WriteConsoleMessage($"\tNo capacity pool defined for account {anfAcct.Name}");
                }
            }

            await WaitForTasksCompletion<CapacityPool>(poolTasks).ConfigureAwait(false);

            //
            // Creating Volumes
            //
            // Note: Volume creation operations at the RP level are executed serially
            Utils.WriteConsoleMessage("Creating Volumes...");
            foreach (ModelNetAppAccount anfAcct in config.Accounts)
            {
                if (anfAcct.CapacityPools != null)
                {
                    foreach (ModelCapacityPool pool in anfAcct.CapacityPools)
                    {
                        if (pool.Volumes != null)
                        {
                            foreach (ModelVolume volume in pool.Volumes)
                            {
                                try
                                {
                                    Volume volumeResource = await CreateOrRetrieveVolumeAsync(anfClient, config.ResourceGroup, anfAcct, pool, volume);
                                }
                                catch (Exception ex)
                                {
                                    Utils.WriteErrorMessage($"An error ocurred while creating volume {anfAcct.Name}{pool.Name}{volume.Name}.\nError message: {ex.Message}");
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            Utils.WriteConsoleMessage($"\tNo volumes defined for Account: {anfAcct.Name}, Capacity Pool: {pool.Name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates or retrieves an Azure NetApp Files Account
        /// </summary>
        /// <param name="config">Project Configuration file which contains the resource group needed</param>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="account">ModelNetAppAccount object that contains the data configured in the appsettings.json file for the ANF account</param>
        /// <returns>NetAppCount object</returns>
        private static async Task<NetAppAccount> CreateOrRetrieveAccountAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient client, ModelNetAppAccount account)
        {
            // Creating ANF Account
            NetAppAccount anfAccount = await GetResourceAsync<NetAppAccount>(client, config.ResourceGroup, account.Name);
            if (anfAccount == null)
            {
                anfAccount = await CreateOrUpdateAnfAccountAsync(config, client, account);
                Utils.WriteConsoleMessage($"\tAccount successfully created, resource id: {anfAccount.Id}");
            }
            else
            {
                Utils.WriteConsoleMessage($"\tAccount already exists, resource id: {anfAccount.Id}");
            }

            return anfAccount;
        }

        /// <summary>
        /// Creates or retrieves a Capacity Pool
        /// </summary>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="resourceGroup">Resource Group name where the capacity pool will be created</param>
        /// <param name="account">ModelNetAppAccount object that contains the data configured in the appsettings.json file for the ANF account</param>
        /// <param name="pool">ModelCapacityPool object that describes the capacity pool to be created, this information comes from appsettings.json</param>
        /// <returns>CapacityPool object</returns>
        private static async Task<CapacityPool> CreateOrRetrieveCapacityPoolAsync(AzureNetAppFilesManagementClient client, string resourceGroup, ModelNetAppAccount account, ModelCapacityPool pool)
        {
            CapacityPool anfCapacityPool = await GetResourceAsync<CapacityPool>(client, resourceGroup, account.Name, pool.Name);
            if (anfCapacityPool == null)
            {
                anfCapacityPool = await CreateOrUpdateCapacityPoolAsync(client, resourceGroup, account, pool);
                Utils.WriteConsoleMessage($"\tCapacity Pool successfully created, resource id: {anfCapacityPool.Id}");
            }
            else
            {
                Utils.WriteConsoleMessage($"\tCapacity already exists, resource id: {anfCapacityPool.Id}");
            }

            return anfCapacityPool;
        }

        /// <summary>
        /// Creates or retrieves volume
        /// </summary>
        /// <param name="config">Project Configuration file which contains the resource group needed</param>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="account">ModelNetAppAccount object that contains the data configured in the appsettings.json file for the ANF account</param>
        /// <returns>NetAppCount object</returns>
        private static async Task<Volume> CreateOrRetrieveVolumeAsync(AzureNetAppFilesManagementClient client, string resourceGroup, ModelNetAppAccount account, ModelCapacityPool pool, ModelVolume volume)
        {
            Volume anfVolume = await GetResourceAsync<Volume>(client, resourceGroup, account.Name, pool.Name, volume.Name);
            if (anfVolume == null)
            {
                anfVolume = await CreateOrUpdateVolumeAsync(client, resourceGroup, account, pool, volume);
                Utils.WriteConsoleMessage($"\tVolume successfully created, resource id: {anfVolume.Id}");
            }
            else
            {
                Utils.WriteConsoleMessage($"\tVolume already exists, resource id: {anfVolume.Id}");
            }

            return anfVolume;
        }
    }
}
