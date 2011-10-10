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
using Microsoft.Ajax.Utilities;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;

namespace CdnHelpers {
    internal class JsGenerator : FileGenerator {

        public override CombinedFileResult Parse(params string[] files) {
            var combined = CombineFiles(files);

            var settings = new CodeSettings {
                
            };

            var parser = new JSParser(combined);

            var block = parser.Parse(settings);
            var result = block.ToCode();

            var key = GenerateKey(result);

            return new CombinedFileResult {
                Content = result,
                Key = key,
            };
        }

    }
}
