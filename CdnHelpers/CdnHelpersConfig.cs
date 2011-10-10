// ----------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// 
// This source is subject to the Microsoft Public License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.Web.Samples;

namespace CdnHelpers {
    public class CdnHelpersConfig {

        private CloudStorageAccount cloudStorageAccount = null;
        private List<string> cdnFolders = new List<string>();

        internal CdnHelpersConfig() {
            this.StorageFolderName = "cdn";
            this.HashKey = "default";
            this.DebuggingEnabled = () => { return false; };
        }

        public string CdnEndointUrl { get; set; }

        public string StorageFolderName { get; set; }

        public Func<bool> DebuggingEnabled { get; set; }

        internal bool BlobStorageBackingEnabled {
            get { return this.cloudStorageAccount != null; }
        }

        internal CloudStorageAccount CloudStorageAccount {
            get { return this.cloudStorageAccount; }
        }

        internal ICollection<string> CdnEnabledFolders {
            get { return this.cdnFolders; }
        }

        internal bool ImageOptimizationsEnabled { get; set; }

        public void EnableBlobStorageBacking(CloudStorageAccount account) {
            this.cloudStorageAccount = account;
        }

        public void EnableImageOptimizations() {
            this.ImageOptimizationsEnabled = true;
        }

        public void UseCdnForContentFolder() {
            cdnFolders.Add("~/Content/");
        }

        public void UseCdnForScriptsFolder() {
            cdnFolders.Add("~/Scripts/");
        }

        public void UserCdnForFolder(string virtualPath) {
            cdnFolders.Add(virtualPath);
        }


        public string HashKey { get; set; }
    }
}
