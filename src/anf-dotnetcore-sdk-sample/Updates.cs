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
    using Microsoft.Azure.Management.NetApp;
    using Microsoft.Azure.Management.NetApp.Models;
    
    public static class Updates
    {
        /// <summary>
        /// Executes some updates on first capacity pool and first volume listed in the configuration file (appsettings.json)
        /// </summary>
        /// <returns></returns>
        public static async Task RunUpdateOperationsSampleAsync(ProjectConfiguration config, AzureNetAppFilesManagementClient anfClient)
        {
            //
            // Capacity Pool Updates
            //
            Utils.WriteConsoleMessage("Performing size update on a Capacity Pool");

            // Get current Capacity Pool information
            CapacityPool capacityPool = null;
            try
            {
                capacityPool = await anfClient.Pools.GetAsync(
                    config.ResourceGroup,
                    config.Accounts[0].Name,
                    config.Accounts[0].CapacityPools[0].Name);
            }
            catch (Exception ex)
            {
                Utils.WriteErrorMessage($"An error occured while getting current Capacity Pool information ({config.Accounts[0].CapacityPools[0].Name}).\nError message: {ex.Message}");
                throw;
            }

            int newCapacityPoolSizeTiB = 10;
            Utils.WriteConsoleMessage($"\tChanging Capacity Pools size from {Utils.GetBytesInTiB(capacityPool.Size)}TiB to {newCapacityPoolSizeTiB}TiB");

            // New size in bytes
            long newCapacityPoolSizeBytes = Utils.GetTiBInBytes(newCapacityPoolSizeTiB);

            // Create capacity pool patch object passing required arguments and the updated size
            CapacityPoolPatch capacityPoolPatch = new CapacityPoolPatch(
                capacityPool.Location,
                size: newCapacityPoolSizeBytes);

            // Update capacity pool resource
            try
            {
                CapacityPool updatedCapacityPool = await anfClient.Pools.UpdateAsync(
                    capacityPoolPatch, 
                    config.ResourceGroup, 
                    config.Accounts[0].Name,
                    config.Accounts[0].CapacityPools[0].Name);

                Utils.WriteConsoleMessage($"\tCapacity Pool successfully updated, new size: {Utils.GetBytesInTiB(updatedCapacityPool.Size)}TiB, resource id: {updatedCapacityPool.Id}");
            }
            catch (Exception ex)
            {
                Utils.WriteErrorMessage($"An error occured while updating Capacity Pool {capacityPool.Id}.\nError message: {ex.Message}");
                throw;
            }

            //
            // Volume Updates
            //
            Utils.WriteConsoleMessage("Performing size and export policy update on a volume");

            // Get current Volume information
            Volume volume = null;
            try
            {
                volume = await anfClient.Volumes.GetAsync(
                    config.ResourceGroup,
                    config.Accounts[0].Name,
                    config.Accounts[0].CapacityPools[0].Name,
                    config.Accounts[0].CapacityPools[0].Volumes[0].Name);
            }
            catch (Exception ex)
            {
                Utils.WriteErrorMessage($"An error occured while getting current Volume information ({config.Accounts[0].CapacityPools[0].Volumes[0].Name}).\nError message: {ex.Message}");
                throw;
            }

            int newVolumeSizeTiB = 1;
            Utils.WriteConsoleMessage($"\tChanging Volume size from {Utils.GetBytesInTiB(volume.UsageThreshold)}TiB to {newVolumeSizeTiB}TiB. Also adding new export policy rule, current count is {volume.ExportPolicy.Rules.Count}.");

            // New size in bytes
            long newVolumeSizeBytes = Utils.GetTiBInBytes(newVolumeSizeTiB);

            // New Export Policy rule
            List<ExportPolicyRule> ruleList = volume.ExportPolicy.Rules.OrderByDescending(r => r.RuleIndex).ToList();

            // Currently, ANF's volume export policy supports up to 5 rules
            VolumePatchPropertiesExportPolicy exportPoliciesPatch = null;
            if (ruleList.Count <= 4)
            {
                ruleList.Add(new ExportPolicyRule()
                {
                    AllowedClients = "10.0.0.4/32",
                    Cifs = false,
                    Nfsv3 = true,
                    Nfsv41 = false,
                    RuleIndex = ruleList.ToList()[0].RuleIndex + 1,
                    UnixReadOnly = false,
                    UnixReadWrite = true
                });

                exportPoliciesPatch = new VolumePatchPropertiesExportPolicy() { Rules = ruleList };
            }

            // Create volume patch object passing required arguments and the updated size
            VolumePatch volumePatch = null;
            if (exportPoliciesPatch != null)
            {
                volumePatch = new VolumePatch(
                    volume.Location,
                    usageThreshold: newVolumeSizeBytes,
                    exportPolicy: exportPoliciesPatch);
            }
            else
            {
                volumePatch = new VolumePatch(
                    volume.Location,
                    usageThreshold: newVolumeSizeBytes);
            }

            // Update size at volume resource
            try
            {
                Volume updatedVolume = await anfClient.Volumes.UpdateAsync(
                    volumePatch,
                    config.ResourceGroup,
                    config.Accounts[0].Name,
                    config.Accounts[0].CapacityPools[0].Name,
                    config.Accounts[0].CapacityPools[0].Volumes[0].Name);

                Utils.WriteConsoleMessage($"\tVolume successfully updated, new size: {Utils.GetBytesInTiB(updatedVolume.UsageThreshold)}TiB, export policy count: {updatedVolume.ExportPolicy.Rules.Count}, resource id: {updatedVolume.Id}");
            }
            catch (Exception ex)
            {
                Utils.WriteErrorMessage($"An error occured while updating Volume {volume.Id}.\nError message: {ex.Message}");
                throw;
            }
        }
    }
}
