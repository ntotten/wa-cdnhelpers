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

namespace CdnHelpers {
    using System;
    using System.Linq;
    using System.Web.Mvc;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Web;
    using System.Text;
    using System.Xml.Linq;
    using System.Web.Hosting;

    public static class CdnUrlHelpers {

        private static IDictionary<string, string> cdnFiles;

        static CdnUrlHelpers() {
            LoadCdnFiles();
        }

        private static void LoadCdnFiles() {
            var doc = XDocument.Load(HostingEnvironment.MapPath("~/App_Data/CdnHelpers.xml"));
            cdnFiles = (from f in doc.Root.Elements()
                        select new {
                            Name = (string)f.Attribute("name"),
                            Url = (string)f.Attribute("url"),
                        }).ToDictionary(n => n.Name, k => k.Url);
        }

        //public static IHtmlString MvcScripts(this HtmlHelper htmlHelper) {

        //    var scripts = new string[] {
        //        "jquery-1.6.1.min.js",
        //        "jquery.unobtrusive-ajax.min.js",
        //        "jquery.validate.min.js",
        //        "jquery.validate.unobtrusive.min.js",
        //    };

        //    var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

        //    var sb = new StringBuilder();
        //    foreach (var script in scripts) {
        //        var tagBuiler = new TagBuilder("script");
        //        tagBuiler.Attributes.Add("type", "text/javascript");
        //        tagBuiler.Attributes.Add("src", urlHelper.CdnContent(script));
        //        sb.Append(tagBuiler.ToString());
        //    }
          
        //    return htmlHelper.Raw(sb.ToString());
        //}

        public static string CdnContent(this UrlHelper urlHelper, string contentPath) {
            if (String.IsNullOrEmpty(contentPath))
                throw new ArgumentNullException("contentPath");

            string[] parts = contentPath.Split('/');

            string file = parts[parts.Length - 1].ToLowerInvariant();

            if (!CdnHelpersContext.Current.Configuration.DebuggingEnabled() && cdnFiles.ContainsKey(file)) {
                return String.Concat(urlHelper.RequestContext.HttpContext.Request.Url.Scheme, "://", cdnFiles[file]);
            }

            return CdnUrlUtility.BuildCdnUrl(urlHelper, contentPath);
        }

        public static string CdnCombinedJs(this UrlHelper urlHelper, string key) {
            var relativePath = CdnHelpersContext.Current.GetJsRelativePath(key);
            return CdnUrlUtility.BuildCdnUrl(urlHelper, relativePath);
        }

        public static string CdnCombinedCss(this UrlHelper urlHelper, string key) {
           var relativePath = CdnHelpersContext.Current.GetCssRelativePath(key);
           return CdnUrlUtility.BuildCdnUrl(urlHelper, relativePath);
        }

        public static string CdnAction(this UrlHelper urlHelper, string actionName) {
            return CdnAction(urlHelper, actionName, null);
        }

        public static string CdnAction(this UrlHelper urlHelper, string actionName, string controllerName) {
            return CdnAction(urlHelper, actionName, controllerName, null);
        }

        public static string CdnAction(this UrlHelper urlHelper, string actionName, string controllerName, object routeValues) {
            var contentPath = urlHelper.Action(actionName, controllerName, routeValues);
            return CdnUrlUtility.BuildCdnUrl(urlHelper, contentPath);
        }
    }
}
