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
using System.Web.Mvc;
using Microsoft.Web.Samples;

namespace CdnHelpers {
    public static class CdnSpriteHelpers {

        public static string CdnSpriteCss(this UrlHelper urlHelper, string virtualPath) {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);
            string cssFileName = ImageOptimizations.LinkCompatibleCssFile(httpContext.Request.Browser) ?? ImageOptimizations.LowCompatibilityCssFileName;
            return CdnUrlUtility.BuildCdnUrl(urlHelper, CdnHelpersContext.Current.GetSpriteCssRelativePath(virtualPath + cssFileName));
        }

        public static IHtmlString CdnSpriteImage(this HtmlHelper htmlHelper, string virtualPath) {
            return Sprite.Image(virtualPath);
        }

        public static IHtmlString CdnSpriteImage(this HtmlHelper htmlHelper, string virtualPath, object htmlAttributes) {
            return Sprite.Image(virtualPath, htmlAttributes);
        }

        public static IHtmlString CdnSpriteImage(this HtmlHelper htmlHelper, string virtualPath, IDictionary<string, object> htmlAttributes) {
            return Sprite.Image(virtualPath, htmlAttributes);
        }

    }
}
