// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ANF.Samples.Model;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure.Authentication;

    /// <summary>
    /// Contains public methods to get configuration settigns, to initiate authentication, output error results, etc.
    /// </summary>
    public static class ServicePrincipalAuth
    {
        /// <summary>
        /// Gets service principal based credentials
        /// </summary>
        /// <param name="authEnvironmentVariable">Environment variable that points to the file system secured azure auth settings</param>
        /// <returns>ServiceClientCredentials</returns>
        public static async Task<ServiceClientCredentials> GetServicePrincipalCredential(string authEnvironmentVariable)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            AzureAuthInfo authSettings = JsonSerializer.Deserialize<AzureAuthInfo>(File.ReadAllText(Environment.GetEnvironmentVariable(authEnvironmentVariable)), jsonOptions);

            var aadSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(authSettings.ActiveDirectoryEndpointUrl),
                TokenAudience = new Uri(authSettings.ManagementEndpointUrl),
                ValidateAuthority = true
            };

            return await ApplicationTokenProvider.LoginSilentAsync(
                authSettings.TenantId,
                authSettings.ClientId,
                authSettings.ClientSecret,
                aadSettings);
        }
    }
}
