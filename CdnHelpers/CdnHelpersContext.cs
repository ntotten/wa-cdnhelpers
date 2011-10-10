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
using System.Web.Hosting;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.Web.Samples;
using System.Web;

namespace CdnHelpers {
    public class CdnHelpersContext {

        private readonly string appStritesFolderName;
        private static CdnHelpersContext _instance = new CdnHelpersContext();
        private Dictionary<string, string> cssFiles = new Dictionary<string, string>();
        private Dictionary<string, string> jsFiles = new Dictionary<string, string>();
        private Dictionary<string, string> spriteCssFiles = new Dictionary<string, string>();
        private CdnHelpersConfig config = new CdnHelpersConfig();

        public static CdnHelpersContext Current {
            get { return _instance; }
        }

        public CdnHelpersConfig Configuration {
            get { return this.config; }
        }

        private CdnHelpersContext() {
            appStritesFolderName = ImageOptimizations.SpriteDirectoryRelativePath.Replace("~/", String.Empty).Replace("/", String.Empty);
        }

        public void Configure(Action<CdnHelpersConfig> configure) {
            configure(this.config);

            // Create local directory for local debugging or when using web role as backing store
            var cdnPath = HostingEnvironment.MapPath("~/" + this.config.StorageFolderName);
            if (!Directory.Exists(cdnPath)) {
                Directory.CreateDirectory(cdnPath);
            } else {
                // Cdn folder exists so we empty it
                EmptyDirectoryContents(cdnPath);
            }

            if (this.config.ImageOptimizationsEnabled) {
                SetupImageOptimizations();
            }

            // Copy all the cdn enabled directories to the cdn directory
            foreach (var directory in this.config.CdnEnabledFolders) {
                var dest = directory.Replace("~/", "~/cdn/");
                CopyDirectoryAndFiles(HostingEnvironment.MapPath(directory), HostingEnvironment.MapPath(dest));
            }

            // If blob backing store is enabled copy all the files
            // in the cdn folder to the blob.
            if (this.config.BlobStorageBackingEnabled) {
                CopyCdnFilesToBlob();
            }

        }

        public void RegisterCombinedCssFiles(string name, params string[] files) {
            var parser = new CssGenerator();
            RegisterCombinedDocument(parser, cssFiles, name, "css", files);
        }

        public void RegisterCombinedJsFiles(string name, params string[] files) {
            var parser = new JsGenerator();
            RegisterCombinedDocument(parser, jsFiles, name, "js", files);
        }

        private void RegisterCombinedDocument(FileGenerator parser, Dictionary<string, string> fileCollection, string name, string fileExtension, params string[] files) {
            var parseResult = parser.Parse(files);
            var filePath = this.config.StorageFolderName + "/" + parseResult.Key + "." + fileExtension;
            SaveFile(filePath, parseResult.Content);
            fileCollection.Add(name, filePath.Replace(this.config.StorageFolderName + "/", string.Empty));
        }

        public string GetCssRelativePath(string key) {
            return "/" + cssFiles[key];
        }

        public string GetJsRelativePath(string key) {
            return "/" + jsFiles[key];
        }

        public string GetSpriteCssRelativePath(string virtualPath) {
            return "/" + spriteCssFiles[virtualPath];
        }

        private void SetupImageOptimizations() {
            // Initialize
            var module = new ImageOptimizationModule();
            module.Init(HttpContext.Current.ApplicationInstance);

            var appSpritesPath = HostingEnvironment.MapPath(ImageOptimizations.SpriteDirectoryRelativePath);
            var appStritesCdnPath = HostingEnvironment.MapPath("~/" + Path.Combine(this.config.StorageFolderName, appStritesFolderName));
            if (!Directory.Exists(appStritesCdnPath)) {
                Directory.CreateDirectory(appStritesCdnPath);
            }

            CopySpriteFiles(appSpritesPath, appSpritesPath);
            var directories = Directory.EnumerateDirectories(appSpritesPath);
            foreach (var directory in directories) {
                CopySpriteFiles(appSpritesPath, directory);
            }
        }

        private void CopySpriteFiles(string appSpritesPath, string path) {
            var files = Directory.EnumerateFiles(path);

            var timeStamp = File.ReadAllText(Path.Combine(path, "lowCompat.css"));
            var folderKey = FileGenerator.GenerateKey(timeStamp);

            foreach (var file in files) {
                var fileName = Path.GetFileName(file);
                var fileRelativePath = file.Substring(appSpritesPath.Length, file.Length - appSpritesPath.Length);
                var keyedFolderPath = Path.Combine(this.config.StorageFolderName, appStritesFolderName, folderKey);
                var keyedFolder = HostingEnvironment.MapPath("~/" + keyedFolderPath);
                if (!Directory.Exists(keyedFolder)) {
                    Directory.CreateDirectory(keyedFolder);
                }
                var filePathStore = Path.Combine(keyedFolderPath, fileRelativePath);
                if (fileRelativePath.Length > fileName.Length) {
                    // Create the subfolder if it doesn't exist
                    var folder = HostingEnvironment.MapPath("~/" + filePathStore.Substring(0, filePathStore.Length - fileName.Length));
                    if (!Directory.Exists(folder)) {
                        Directory.CreateDirectory(folder);
                    }
                }

                if (fileName.StartsWith("sprite") || fileName.StartsWith("blank.gif")) {
                    var destFile = HostingEnvironment.MapPath("~/" + filePathStore);
                    if (File.Exists(destFile)) {
                        File.Delete(destFile);
                    }
                    File.Copy(file, destFile);
                }
                if (fileName == "highCompat.css" || fileName == "lowCompat.css") {
                    var parser = new CssGenerator();
                    var originalVirtualPath = "~/" + appStritesFolderName + "/" + fileRelativePath.Replace("\\", "/");
                    var parserResult = parser.Parse(originalVirtualPath);
                    //var cssFilePath = filePathStore.Replace(fileName, parserResult.Key + ".css");
                    var fixedContent = parserResult.Content.Replace(@"\5c\2f", @"\/").Replace(@"\5c\2e", @"\.");
                    SaveFile(filePathStore, fixedContent);
                    spriteCssFiles.Add(originalVirtualPath, filePathStore.Replace("\\", "/").Replace(this.config.StorageFolderName + "/", string.Empty));
                }
            }
        }

        private void SaveFile(string filePath, string content) {
            // Save to local directory for local debugging or when using web role as backing store
            var fullPath = HostingEnvironment.MapPath("~/" + filePath);
            File.WriteAllText(fullPath, content);
        }

        private void CopyCdnFilesToBlob() {
            // Create blob container if does not exist
            var blobClient = new CloudBlobClient(this.config.CloudStorageAccount.BlobEndpoint, this.config.CloudStorageAccount.Credentials);
            var container = blobClient.GetContainerReference(this.config.StorageFolderName);
            container.CreateIfNotExist();

            var cdnPath = HostingEnvironment.MapPath("~/" + this.config.StorageFolderName);
            CopyDirectoryToBlob(blobClient, cdnPath);
        }

        private void CopyDirectoryToBlob(CloudBlobClient blobClient, string path) {
            var files = Directory.EnumerateFiles(path);
            var rootPath = HostingEnvironment.MapPath("~/");
            foreach (var file in files) {
                var container = file.Substring(rootPath.Length, file.Length - rootPath.Length);
                var blobReference = blobClient.GetBlobReference(container);
                try {
                    blobReference.FetchAttributes();
                } catch (StorageClientException e) {
                    if (e.ErrorCode == StorageErrorCode.ResourceNotFound) {
                        var content = File.ReadAllText(file);
                        blobReference.UploadText(content);
                    }
                }
            }
            var directories = Directory.EnumerateDirectories(path);
            foreach (var directory in directories) {
                CopyDirectoryToBlob(blobClient, directory);
            }
        }

        private void CopyDirectoryAndFiles(string source, string dest) {
            if (!Directory.Exists(dest)) {
                Directory.CreateDirectory(dest);
            }

            var directories = Directory.GetDirectories(source);
            foreach (var directory in directories) {
                CopyDirectoryAndFiles(directory, directory.Replace(source, dest));
            }

            var files = Directory.EnumerateFiles(source);
            foreach (var file in files) {
                var destFile = file.Replace(source, dest);
                if (File.Exists(destFile)) {
                    File.Delete(destFile);
                }
                File.Copy(file, destFile);
                //var extension = Path.GetExtension(file).ToLowerInvariant();
                //if (extension == ".js") {
                //    try {
                //        var jsGenerator = new JsGenerator();
                //        var jsResult = jsGenerator.Parse(file);
                //        File.WriteAllText(destFile, jsResult.Content);
                //    } catch {
                //        // Error, just use the normal file
                //        File.Copy(file, destFile);
                //    }
                //} else if (extension == ".css") {
                //    try {
                //        var cssGenerator = new CssGenerator();
                //        var cssResult = cssGenerator.Parse(file);
                //        File.WriteAllText(destFile, cssResult.Content);
                //    } catch {
                //        // Error, just use the normal file
                //        File.Copy(file, destFile);
                //    }
                //} else {
                //    File.Copy(file, destFile);
                //}
            }
        }

        private void EmptyDirectoryContents(string path) {
            var directories = Directory.EnumerateDirectories(path);
            foreach (var directory in directories) {
                Directory.Delete(directory, true);
            }
            var files = Directory.EnumerateFiles(path);
            foreach (var file in files) {
                if (!Path.GetFileName(file).Equals("web.config", StringComparison.InvariantCultureIgnoreCase)) {
                    File.Delete(file);
                }
            }
        }

    }

}
