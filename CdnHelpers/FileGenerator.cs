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
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web;
using System.IO;

namespace CdnHelpers {
    internal abstract class FileGenerator {

        public string EncryptionKey { get; set; }

        public abstract CombinedFileResult Parse(params string[] files);

        protected static string CombineFiles(params string[] files) {
            var sb = new StringBuilder();

            foreach (var file in files) {
                string filePath;
                if (Path.IsPathRooted(file)) {
                    filePath = file;
                } else {
                    filePath = HostingEnvironment.MapPath(file);
                }
                var source = System.IO.File.ReadAllText(filePath);
                sb.AppendLine(source);
            }
            return sb.ToString();
        }

        internal static string GenerateKey(string minResult) {
            string hash;
            using (var cryto = new HMACMD5(Encoding.Default.GetBytes(CdnHelpersContext.Current.Configuration.HashKey))) {
                var bytes = cryto.ComputeHash(Encoding.UTF8.GetBytes(minResult));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    sb.Append(bytes[i].ToString("x2"));
                }
                hash = sb.ToString();
            }
            return hash;
        }
    }
}
