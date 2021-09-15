// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ANF.Samples.Common;
    using Microsoft.Azure.Management.NetApp;
    using static Microsoft.Azure.Management.ANF.Samples.Common.Utils;

    class program
    {
        /// <summary>
        /// Sample console application that execute CRUD management operations on Azure NetApp Files resource
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            DisplayConsoleAppHeader();

            try
            {
                RunAsync().GetAwaiter().GetResult();
                Utils.WriteConsoleMessage("Sample application successfuly completed execution.");
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
            }
        }

        static private async Task RunAsync()
        {
            // Getting project configuration
            ProjectConfiguration config = GetConfiguration("appsettings.json");

            // Authenticating using service principal, refer to README.md file for requirement details
            var credentials = await ServicePrincipalAuth.GetServicePrincipalCredential("AZURE_AUTH_LOCATION");

            //// Authenticating using Device Login flow - uncomment following 4 lines for this type of authentication and
            //// comment the lines related to service principal authentication, refer to README.md file for requirement details
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //AuthenticationResult authenticationResult = await AuthenticateAsync(config.PublicClientApplicationOptions);
            //TokenCredentials credentials = new TokenCredentials(authenticationResult.AccessToken);
            //Console.ResetColor();

            // Instantiating a new ANF management client
            WriteConsoleMessage("Instantiating a new Azure NetApp Files management client...");
            AzureNetAppFilesManagementClient anfClient = new AzureNetAppFilesManagementClient(credentials) { SubscriptionId = config.SubscriptionId };
            WriteConsoleMessage($"\tApi Version: {anfClient.ApiVersion}");

            // Creating ANF resources (Account, Pool, Volumes)
            await Creation.RunCreationSampleAsync(config, anfClient);

            // Creating and restoring snapshots
            await Snapshots.RunSnapshotOperationsSampleAsync(config, anfClient);

            // Performing updates on Capacity Pools and Volumes
            await Updates.RunUpdateOperationsSampleAsync(config, anfClient);

            if (config.ShouldCleanUp)
            {
                await Cleanup.RunCleanupTasksSampleAsync(config, anfClient);
            }
        }
    }
}
