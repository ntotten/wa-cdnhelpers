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
using System.Web;
using Microsoft.Ajax.Utilities;
using RazorEngine;
using System.Security.Cryptography;
using System.Web.Hosting;

namespace CdnHelpers {
    internal class CssGenerator : FileGenerator {

        public override CombinedFileResult Parse(params string[] files) {
            var combined = CombineFiles(files);

            var razorResult = Razor.Parse(combined);

            var cssParser = new CssParser();
            cssParser.Settings.CommentMode = CssComment.Important;
            var minResult = cssParser.Parse(razorResult);

            string key = GenerateKey(minResult);

            return new CombinedFileResult {
                Content = minResult,
                Key = key,
            };
        }

    }
}
