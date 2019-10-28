// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Model
{
	/// <summary>
    /// Instantiates a ModelExportPolicyRule object
    /// </summary>
    public class ModelExportPolicyRule
    {
        /// <summary>
        /// Gets or sets RuleIndex
        /// </summary>
        /// <remarks>This is initally non zero-based index, therefore must start with 1</remarks>
        public int RuleIndex { get; set; }

        /// <summary>
        /// Gets or sets AllowedClients
        /// </summary>
        public string AllowedClients { get; set; }

        /// <summary>
        /// Gets or sets CIFS
        /// </summary>
        public bool? Cifs { get; set; }

        /// <summary>
        /// Gets or sets Nfsv3
        /// </summary>
        public bool? Nfsv3 { get; set; }

        /// <summary>
        /// Gets or sets Nfsv41
        /// </summary>
        public bool? Nfsv41 { get; set; }

        /// <summary>
        /// Gets or sets UnixReadOnly
        /// </summary>
        public bool? UnixReadOnly { get; set; }

        /// <summary>
        /// Gets or sets UnixReadWrite
        /// </summary>
        public bool? UnixReadWrite { get; set; }
        
    }
}
