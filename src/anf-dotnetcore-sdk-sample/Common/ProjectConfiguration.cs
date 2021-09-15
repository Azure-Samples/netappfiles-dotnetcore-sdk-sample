// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common
{
    using System.IO;
    using System.Collections.Generic;
    using System.Reflection;
    using Microsoft.Azure.Management.ANF.Samples.Model;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Description of the configuration of an AzureAD public client application (desktop/mobile application). This should
    /// match the application registration done in the Azure portal
    /// Source solution: https://github.com/azure-samples/active-directory-dotnetcore-console-up-v2
    /// </summary>
    public class ProjectConfiguration
    {
        /// <summary>
        /// Authentication options
        /// </summary>
        public PublicClientApplicationOptions PublicClientApplicationOptions { get; set; }

        /// <summary>
        /// Gets or sets a list of accounts to be created
        /// </summary>
        public List<ModelNetAppAccount> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the subcription Id where account(s) will be deployed
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets or sets the resource group where the ANF account(s) will be created
        /// </summary>
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Gets or sets the flag for cleaning up the resources after the creation
        /// </summary>
        public bool ShouldCleanUp { get; set; }

        /// <summary>
        /// Reads the configuration from a json file
        /// </summary>
        /// <param name="path">Path to the configuration json file</param>
        /// <returns>SampleConfiguration as read from the json file</returns>
        public static ProjectConfiguration ReadFromJsonFile(string path)
        {
            // .NET configuration
            IConfigurationRoot dotnetConfig;

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile(path);

            dotnetConfig = builder.Build();

            // Initializing and reading the auth endpoint and other configuration information
            ProjectConfiguration config = new ProjectConfiguration()
            {
                PublicClientApplicationOptions = new PublicClientApplicationOptions(),
                Accounts = new List<ModelNetAppAccount>()
            };

            dotnetConfig.Bind("authentication", config.PublicClientApplicationOptions);
            dotnetConfig.Bind("accounts", config.Accounts);

            config.SubscriptionId = dotnetConfig.GetValue<string>("general:subscriptionId");
            config.ResourceGroup = dotnetConfig.GetValue<string>("general:resourceGroup");
            config.ShouldCleanUp = dotnetConfig.GetValue<bool>("general:shouldCleanUp");

            return config;
        }
    }
}

