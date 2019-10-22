// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common.Sdk
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ANF.Samples.Model;
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure;

    /// <summary>
    /// Contains public methods for SDK related operations
    /// </summary>
    public static class CommonSdk
    {

        /// <summary>
        /// Creates or updates a Azure NetApp Files Account
        /// </summary>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="account">Account object generated from information contained at the appsettings.json file at Accounts section</param>
        /// <returns>NetAppAccount</returns>
        public static async Task<NetAppAccount> CreateOrUpdateAnfAccountAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient client, ModelNetAppAccount account)
        {
            // Setting up NetApp Files account object
            NetAppAccount anfAccountBody = new NetAppAccount(account.Location, null, account.Name);

            // Requesting account to be created
            return await client.Accounts.CreateOrUpdateAsync(anfAccountBody, config.ResourceGroup, account.Name);
        }

        /// <summary>
        /// Creates or updates a Azure NetApp Files Account with Active Directory information for SMB
        /// </summary>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="account">Account object generated from information contained at the appsettings.json file at Accounts section</param>
        /// <param name="activeDirectoryInfoList">Active Directory object list</param>
        /// <returns>NetAppAccount object</returns>
        public static async Task<NetAppAccount> CreateOrUpdateAnfAccountAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient client, ModelNetAppAccount account, ActiveDirectory[] activeDirectories)
        {
            // Setting up NetApp Files account object and Active Directory Information
            NetAppAccount anfAccount = new NetAppAccount(account.Location.ToLower(), null, account.Name, null, null, null, activeDirectories);

            // Requesting account to be created
            return await client.Accounts.CreateOrUpdateAsync(anfAccount, config.ResourceGroup, account.Name);
        }

        /// <summary>
        /// Creates or updates a capacity pool
        /// </summary>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="resourceGroup">Resource Group name where capacity pool will be created</param>
        /// <param name="account">Account object generated from information contained at the appsettings.json file at Accounts section</param>
        /// <param name="pool">ModelCapacityPool object that describes the capacity pool to be created, this information comes from appsettings.json</param>
        /// <returns>CapacityPool object</returns>
        public static async Task<CapacityPool> CreateOrUpdateCapacityPoolAsync(AzureNetAppFilesManagementClient client, string resourceGroup, ModelNetAppAccount account, ModelCapacityPool pool)
        {
            CapacityPool capacityPoolBody = new CapacityPool()
            {
                Location = account.Location.ToLower(),
                ServiceLevel = pool.ServiceLevel,
                Size = pool.Size
            };

            return await client.Pools.CreateOrUpdateAsync(capacityPoolBody, resourceGroup, account.Name, pool.Name);
        }

        /// <summary>
        /// Creates or updates a volume. In this process, notice that we need to create two mandatory objects, one as the export rule list and the voluome body itself before
        /// we request the volume creation.
        /// </summary>
        /// <param name="client">Azure NetApp Files Management Client</param>
        /// <param name="resourceGroup">Resource Group name where volume will be created</param>
        /// <param name="account">Account object generated from information contained at the appsettings.json file at Accounts section</param>
        /// <param name="pool">ModelCapacityPool object that describes the capacity pool to be created, this information comes from appsettings.json</param>
        /// <param name="volume">ModelVolume object that represents the volume to be created that is defined in appsettings.json</param>
        /// <returns>Volume object</returns>
        public static async Task<Volume> CreateOrUpdateVolumeAsync(AzureNetAppFilesManagementClient client, string resourceGroup, ModelNetAppAccount account, ModelCapacityPool pool, ModelVolume volume)
        {
            List<ExportPolicyRule> ruleList = new List<ExportPolicyRule>();
            foreach (ModelExportPolicyRule rule in volume.ExportPolicies)
            {
                ruleList.Add(new ExportPolicyRule()
                {
                    AllowedClients = rule.AllowedClients,
                    Cifs = rule.Cifs,
                    Nfsv3 = rule.Nfsv3,
                    Nfsv41 = rule.Nfsv4,
                    RuleIndex = rule.RuleIndex,
                    UnixReadOnly = rule.UnixReadOnly,
                    UnixReadWrite = rule.UnixReadWrite
                });
            }

            VolumePropertiesExportPolicy exportPolicies = new VolumePropertiesExportPolicy() { Rules = ruleList };

            Volume volumeBody = new Volume()
            {
                ExportPolicy = exportPolicies,
                Location = account.Location.ToLower(),
                ServiceLevel = pool.ServiceLevel,
                CreationToken = volume.CreationToken,
                SubnetId = volume.SubnetId,
                UsageThreshold = volume.UsageThreshold,
                ProtocolTypes = new List<string> { "NFSv3" } // If NFS 4.1 is required, use NFSv4.1 value instead, at this moment only one protocol is supported in a single volume
            };

            return await client.Volumes.CreateOrUpdateAsync(volumeBody, resourceGroup, account.Name, pool.Name, volume.Name);
        }

        /// <summary>
        /// Returns an ANF resource or null if it does not exist
        /// </summary>
        /// <typeparam name="T">Valid types: NetAppAccount, CapacityPool, Volume, Snapshot</typeparam>
        /// <param name="client">ANF Client object</param>
        /// <param name="parameterList">List of parameters required depending on the resource type:
        ///     Snapshot     -> ResourceGroupName, AccountName, PoolName, VolumeName, SnapshotName
        ///     Volume       -> ResourceGroupName, AccountName, PoolName, VolumeName
        ///     CapacityPool -> ResourceGroupName, AccountName, PoolName
        ///     Account      -> ResourceGroupName, AccountName</param>
        /// <returns></returns>
        public static async Task<T> GetResourceAsync<T>(AzureNetAppFilesManagementClient client, params string[] parameterList )
        {
            try
            {
                if (typeof(T) == typeof(Snapshot))
                {
                    return (T)(object)await client.Snapshots.GetAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1],
                        poolName: parameterList[2],
                        volumeName: parameterList[3],
                        snapshotName: parameterList[4]);
                }
                else if (typeof(T) == typeof(Volume))
                {
                    return (T)(object)await client.Volumes.GetAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1],
                        poolName: parameterList[2],
                        volumeName: parameterList[3]);
                }
                else if (typeof(T) == typeof(CapacityPool))
                {
                    return (T)(object)await client.Pools.GetAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1],
                        poolName: parameterList[2]);
                }
                else if (typeof(T) == typeof(NetAppAccount))
                {
                    return (T)(object)await client.Accounts.GetAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1]);
                }
            }
            catch (Exception ex)
            {
                // The following HResult is thrown if no resource is found
                if (ex.HResult != -2146233088)
                {
                    throw;
                }
            }

            // If object is not supported by this method or nothing returned from GetAsync, return null
            return default(T);
        }

        /// <summary>
        /// Returns a list of ANF resources or null if none returns
        /// </summary>
        /// <typeparam name="T">Valid types: NetAppAccount, CapacityPool, Volume, Snapshot</typeparam>
        /// <param name="client">ANF Client object</param>
        /// <param name="parameterList">List of parameters required depending on the resource type:
        ///     Snapshot     -> ResourceGroupName, AccountName, PoolName, VolumeName, SnapshotName
        ///     Volume       -> ResourceGroupName, AccountName, PoolName, VolumeName
        ///     CapacityPool -> ResourceGroupName, AccountName, PoolName
        ///     Account      -> ResourceGroupName, AccountName</param>
        /// <returns></returns>
        public static async Task<IEnumerable<T>> ListResourceAsync<T>(AzureNetAppFilesManagementClient client, params string[] parameterList)
        {
            try
            {
                if (typeof(T) == typeof(Snapshot))
                {
                    return (IEnumerable<T>)(object)await client.Snapshots.ListAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1],
                        poolName: parameterList[2],
                        volumeName: parameterList[3]);
                }
                if (typeof(T) == typeof(Volume))
                {
                    return (IEnumerable<T>)(object)await client.Volumes.ListAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1],
                        poolName: parameterList[2]);
                }
                else if (typeof(T) == typeof(CapacityPool))
                {
                    return (IEnumerable<T>)(object)await client.Pools.ListAsync(
                        resourceGroupName: parameterList[0],
                        accountName: parameterList[1]);
                }
                else if (typeof(T) == typeof(NetAppAccount))
                {
                    return (IEnumerable<T>)(object)await client.Accounts.ListAsync(
                        resourceGroupName: parameterList[0]);
                }
            }
            catch (Exception ex)
            {
                // The following HResult is thrown if no resource is found
                if (ex.HResult != -2146233088)
                {
                    throw;
                }
            }

            // If object is not supported by this method or a not found exception was raised, return null
            return null;
        }

        /// <summary>
        /// Waits for a list of tasks to complete and output and throw errors if they occur
        /// </summary>
        /// <typeparam name="T">Valid types: NetAppAccount, CapacityPool, Volume, Snapshot</typeparam>
        /// <param name="taskList">List of tasks to wait for completion</param>
        /// <returns></returns>
        public static async Task WaitForTasksCompletion<T>(List<Task<T>> taskList)
        {
            try
            {
                await Task.WhenAll(taskList);
            }
            catch
            {
                OutputTaskErrorResults<T>(taskList);
                throw;
            }
        }

        /// <summary>
        /// Waits for a list of tasks to complete and output and throw errors if they occur, this is used when the waited tasks do not return values
        /// </summary>
        /// <param name="taskList">List of tasks to wait for completion</param>
        /// <returns></returns>
        public static async Task WaitForTasksCompletion(List<Task> taskList)
        {
            try
            {
                await Task.WhenAll(taskList);
            }
            catch
            {
                OutputTaskErrorResults(taskList);
                throw;
            }
        }

        /// <summary>
        /// Iterates over a Task<typeparamref name="T"/> list for faulted tasks and outputs the error messages
        /// </summary>
        /// <typeparam name="T">This can be NetAppAccount, CapacityPool, Volume, Snapshot</typeparam>
        /// <param name="tasks">List of tasks</param>
        /// <returns></returns>
        public static void OutputTaskErrorResults<T>(List<Task<T>> tasks)
        {
            // Outputs errors if any
            Console.ForegroundColor = ConsoleColor.Red;

            tasks.Where(task => task.IsFaulted).ToList()
                 .ForEach(task =>
                 {
                     Utils.WriteConsoleMessage($"\tTask Id: {task.Id}");
                     foreach (CloudException ce in task.Exception.InnerExceptions)
                     {
                         HttpRequestMessageWrapper request = ce.Request;
                         Utils.WriteConsoleMessage($"\t\tTask Exception Message: {ce.Message}");
                         Utils.WriteConsoleMessage($"\t\tTask Exception Request Content: {request.Content}");
                     }
                 });
            Console.ResetColor();
        }

        /// <summary>
        /// Iterates over a Task list for faulted tasks and outputs the error messages
        /// </summary>
        /// <param name="tasks">List of tasks</param>
        /// <returns></returns>
        public static void OutputTaskErrorResults(List<Task> tasks)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            tasks.Where(task => task.IsFaulted).ToList()
                 .ForEach(task =>
                 {
                     Utils.WriteConsoleMessage($"\tTask Id: {task.Id}");
                     foreach (CloudException ce in task.Exception.InnerExceptions)
                     {
                         HttpRequestMessageWrapper request = ce.Request;
                         Utils.WriteConsoleMessage($"\t\tTask Exception Message: {ce.Message}");
                         Utils.WriteConsoleMessage($"\t\tTask Exception Request Content: {request.Content}");
                     }
                 });

            Console.ResetColor();
        }

        static public async Task WaitForNoAnfResource<T>(AzureNetAppFilesManagementClient client, string resourceId, int intervalInSec = 10, int retries = 60)
        {
            for (int i = 0; i < retries; i++)
            {
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(intervalInSec));

                try
                {
                    if (typeof(T) == typeof(Snapshot))
                    {
                        var resource = await client.Snapshots.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId),
                            ResourceUriUtils.GetAnfSnapshot(resourceId));
                    }
                    else if (typeof(T) == typeof(Volume))
                    {
                        var resource = await client.Volumes.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId),
                            ResourceUriUtils.GetAnfVolume(resourceId));
                    }
                    else if (typeof(T) == typeof(CapacityPool))
                    {
                        var resource = await client.Pools.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId),
                            ResourceUriUtils.GetAnfCapacityPool(resourceId));
                    }
                    else if (typeof(T) == typeof(NetAppAccount))
                    {
                        var resource = await client.Accounts.GetAsync(ResourceUriUtils.GetResourceGroup(resourceId),
                            ResourceUriUtils.GetAnfAccount(resourceId));
                    }
                }
                catch (Exception ex)
                {
                    // The following HResult is thrown if no resource is found
                    if (ex.HResult == -2146233088)
                    {
                        break;
                    }
                    throw;
                }
            }
        }
    }
}
