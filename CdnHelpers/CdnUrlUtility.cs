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
using System.Web.Mvc;
using System.Configuration;
using System.IO;

namespace CdnHelpers {
    internal class CdnUrlUtility {

        public static string BuildCdnUrl(UrlHelper urlHelper, string contentPath) {
            if (contentPath.StartsWith("cdn/")) {
                throw new ArgumentException("The content path must not start with cdn/");
            }

            string relativePath = urlHelper.Content(contentPath);

            var config = CdnHelpersContext.Current.Configuration;

            if (config.DebuggingEnabled()) {
                return "/" + config.StorageFolderName  + relativePath;
            }

            
            string cdnEndpoint = config.CdnEndointUrl;

            if (config.BlobStorageBackingEnabled) {
                cdnEndpoint = cdnEndpoint + "/" + config.StorageFolderName;
            }

            return String.Concat(urlHelper.RequestContext.HttpContext.Request.Url.Scheme, "://", cdnEndpoint, relativePath);
        }

    }
}
