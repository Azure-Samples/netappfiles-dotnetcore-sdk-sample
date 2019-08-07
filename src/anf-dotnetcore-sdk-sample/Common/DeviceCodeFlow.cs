// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;
    
    /// <summary>
    /// Security token provider using the Device Code flow
    /// </summary>
    public class DeviceCodeFlow
    {
        /// <summary>
        /// Constructor of a public application leveraging Device Code Flow to sign-in a user
        /// </summary>
        /// <param name="app">MSAL.NET Public client application</param>
        /// <param name="httpClient">HttpClient used to call the protected Web API</param>
        /// <remarks>
        /// For more information see https://aka.ms/msal-net-device-code-flow
        /// </remarks>
        public DeviceCodeFlow(IPublicClientApplication app)
        {
            App = app;
        }
        protected IPublicClientApplication App { get; private set; }

        /// <summary>
        /// Acquires a token from the token cache, or device code flow
        /// </summary>
        /// <returns>An AuthenticationResult if the user successfully signed-in, or otherwise <c>null</c></returns>
        public async Task<AuthenticationResult> AcquireATokenFromCacheOrDeviceCodeFlowAsync(IEnumerable<String> scopes)
        {
            AuthenticationResult result = null;
            var accounts = await App.GetAccountsAsync();

            if (accounts.Any())
            {
                try
                {
                    // Attempt to get a token from the cache (or refresh it silently if needed)
                    result = await App.AcquireTokenSilent(scopes, accounts.FirstOrDefault())
                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException)
                {
                }
            }

            // Cache empty or no token for account in the cache, attempt by device code flow
            if (result == null)
            {
                result = await GetTokenForWebApiUsingDeviceCodeFlowAsync(scopes);
            }

            return result;
        }

        /// <summary>
        /// Gets an access token so that the application accesses the web api in the name of the user
        /// who signs-in on a separate device
        /// </summary>
        /// <returns>An authentication result, or null if the user canceled sign-in, or did not sign-in on a separate device
        /// after a timeout (15 mins)</returns>
        private async Task<AuthenticationResult> GetTokenForWebApiUsingDeviceCodeFlowAsync(IEnumerable<string> scopes)
        {
            AuthenticationResult result;
            try
            {
                result = await App.AcquireTokenWithDeviceCode(scopes,
                    deviceCodeCallback =>
                    {
                        Console.WriteLine(deviceCodeCallback.Message);
                        return Task.FromResult(0);
                    }).ExecuteAsync();
            }
            catch (MsalServiceException ex)
            {
                // Kind of errors you could have (in errorCode and ex.Message)
                string errorCode = ex.ErrorCode;
                throw;
            }
            catch (OperationCanceledException)
            {
                result = null;
            }
            catch (MsalClientException ex)
            {
                string errorCode = ex.ErrorCode;
                result = null;
            }
            return result;
        }
    }
}
