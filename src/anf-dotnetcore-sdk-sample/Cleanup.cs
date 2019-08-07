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
    using Microsoft.Azure.Management.ANF.Samples.Common.Sdk;

    public static class Cleanup
    {
        /// <summary>
        /// Removes all created resources
        /// </summary>
        /// <returns></returns>
        public static async Task RunCleanupTasksSampleAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient anfClient)
        {
            //
            // Cleaning up snapshots
            //
            Utils.WriteConsoleMessage("Cleaning up snapshots...");
            List<Task> snapshotCleanupTasks = new List<Task>();
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
                                IEnumerable<Snapshot> anfSnapshotList = await CommonSdk.ListResourceAsync<Snapshot>(anfClient, config.ResourceGroup, anfAcct.Name, pool.Name, volume.Name);

                                if (anfSnapshotList != null && anfSnapshotList.Count() > 0)
                                {
                                    // Snapshot Name property (and other ANF's related nested resources) returns a relative path up to the name 
                                    // and to use this property with DeleteAsync for example, the argument needs to be sanitized and just the
                                    // actual name needs to be used. 
                                    // Snapshot Name poperty example: "pmarques-anf01/pool01/pmarques-anf01-pool01-vol01/test-a"
                                    // "test-a" is the actual name that needs to be used instead. Below you will see a sample function that 
                                    // parses the name from snapshot resource id
                                    snapshotCleanupTasks.AddRange(anfSnapshotList.ToList().Select(
                                        async snapshot =>
                                        {
                                            await anfClient.Snapshots.DeleteAsync(config.ResourceGroup, anfAcct.Name, pool.Name, volume.Name, ResourceUriUtils.GetAnfSnapshot(snapshot.Id));
                                            Utils.WriteConsoleMessage($"\tDeleted snapshot: {snapshot.Id}");
                                        }).ToList());
                                }
                            }
                        }
                    }
                }
            }

            await WaitForTasksCompletion(snapshotCleanupTasks).ConfigureAwait(false);

            //
            // Cleaning up all volumes
            //
            // Note: Volume deletion operations at the RP level are executed serially
            Utils.WriteConsoleMessage("Cleaning up Volumes...");
            foreach (ModelNetAppAccount anfAcct in config.Accounts)
            {
                if (anfAcct.CapacityPools != null)
                {
                    foreach (ModelCapacityPool pool in anfAcct.CapacityPools)
                    {
                        if (pool.Volumes != null)
                        {
                            IEnumerable<Volume> anfVolumeList = await CommonSdk.ListResourceAsync<Volume>(anfClient, config.ResourceGroup, anfAcct.Name, pool.Name);
                            if (anfVolumeList != null && anfVolumeList.Count() > 0)
                            {
                                foreach (Volume volume in anfVolumeList)
                                {
                                    try
                                    {
                                        await anfClient.Volumes.DeleteAsync(config.ResourceGroup, anfAcct.Name, pool.Name, ResourceUriUtils.GetAnfVolume(volume.Id));
                                        Utils.WriteConsoleMessage($"\tDeleted volume: {volume.Id}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Utils.WriteErrorMessage($"An error ocurred while deleting volume {volume.Id}.\nError message: {ex.Message}");
                                        throw;
                                    }
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

            //
            // Cleaning up capacity pools
            //
            Utils.WriteConsoleMessage("Cleaning up capacity pools...");
            List<Task> poolCleanupTasks = new List<Task>();
            foreach (ModelNetAppAccount anfAcct in config.Accounts)
            {
                if (anfAcct.CapacityPools != null)
                {
                    poolCleanupTasks.AddRange(anfAcct.CapacityPools.Select(
                        async pool =>
                        {
                            CapacityPool anfPool = await GetResourceAsync<CapacityPool>(anfClient, config.ResourceGroup, anfAcct.Name, pool.Name);
                            if (anfPool != null)
                            {
                                await anfClient.Pools.DeleteAsync(config.ResourceGroup, anfAcct.Name, ResourceUriUtils.GetAnfCapacityPool(anfPool.Id));
                                Utils.WriteConsoleMessage($"\tDeleted volume: {anfPool.Id}");
                            }
                        }).ToList());
                }
            }

            await WaitForTasksCompletion(poolCleanupTasks).ConfigureAwait(false);

            //
            // Cleaning up accounts
            //
            Utils.WriteConsoleMessage("Cleaning up accounts...");
            List<Task> accountCleanupTasks = new List<Task>();
            if (config.Accounts != null)
            {
                accountCleanupTasks.AddRange(config.Accounts.Select(
                    async account =>
                    {
                        NetAppAccount anfAccount = await GetResourceAsync<NetAppAccount>(anfClient, config.ResourceGroup, account.Name);
                        if (anfAccount != null)
                        {
                            await anfClient.Accounts.DeleteAsync(config.ResourceGroup, anfAccount.Name);
                            Utils.WriteConsoleMessage($"\tDeleted account: {anfAccount.Id}");
                        }
                    }).ToList());
            }

            await WaitForTasksCompletion(accountCleanupTasks).ConfigureAwait(false);
        }
    }
}
