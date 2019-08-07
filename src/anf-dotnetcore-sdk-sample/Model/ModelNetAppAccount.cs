// Copyright (c) Microsoft and contributors.  All rights reserved.
//
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.

namespace Microsoft.Azure.Management.ANF.Samples.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Instantiates a ModelNetAppAccount object
    /// </summary>
    public class ModelNetAppAccount
    {
        /// <summary>
        /// Gets or sets a list of capacity pools
        /// </summary>
        public List<ModelCapacityPool> CapacityPools { get; set; }

        /// <summary>
        /// Gets or sets name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gtes or sets Location
        /// </summary>
        public string Location { get; set; }
    }
}
