// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Common
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Identity.Client;

    /// <summary>
    /// Contains public methods to get configuration settigns, to initiate authentication, output error results, etc.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Authentication scope, this is the minimum required to be able to manage resources
        /// </summary>
        private static string[] Scopes { get; set; } = new string[] { @"https://management.core.windows.net/.default" };

        /// <summary>
        /// Simple function to display this console app basic information
        /// </summary>
        public static void DisplayConsoleAppHeader()
        {
            Console.WriteLine("Azure NetAppFiles .netcore SDK Samples - Sample project that performs CRUD management operations with Azure NetApp Files SDK");
            Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------");
            Console.WriteLine("");
        }

        /// <summary>
        /// Function to create the configuration object, used for authentication and ANF resource information
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static ProjectConfiguration GetConfiguration(string filename)
        {
            return ProjectConfiguration.ReadFromJsonFile(filename);
        }

        /// <summary>
        /// Function to authenticate against Azure AD using MSAL 3.0 
        /// </summary>
        /// <param name="appConfig">Application configuration required for the authentication process</param>
        /// <returns>AuthenticationResult object</returns>
        public static async Task<AuthenticationResult> AuthenticateAsync(PublicClientApplicationOptions appConfig)
        {
            var app = PublicClientApplicationBuilder.CreateWithApplicationOptions(appConfig)
                                                    .Build();

            DeviceCodeFlow tokenAcquisitionHelper = new DeviceCodeFlow(app);

            return (await tokenAcquisitionHelper.AcquireATokenFromCacheOrDeviceCodeFlowAsync(Scopes));
        }

        /// <summary>
        /// Converts bytes into TiB
        /// </summary>
        /// <param name="size">Size in bytes</param>
        /// <returns>Returns (decimal) the value of bytes in TiB scale</returns>
        public static decimal GetBytesInTiB(long size)
        {
            return (decimal)size / 1024 / 1024 / 1024 / 1024;
        }

        /// <summary>
        /// Converts TiB into bytes
        /// </summary>
        /// <param name="size">Size in TiB</param>
        /// <returns>Returns (long) the value of TiB in bytes scale</returns>
        public static long GetTiBInBytes(decimal size)
        {
            return (long)size * 1024 * 1024 * 1024 * 1024;
        }

        /// <summary>
        /// Displays errors messages in red
        /// </summary>
        /// <param name="message">Message to be written in console</param>
        public static void WriteErrorMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Utils.WriteConsoleMessage(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Displays errors messages in red
        /// </summary>
        /// <param name="message">Message to be written in console</param>
        public static void WriteConsoleMessage(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
        }
    }
}
