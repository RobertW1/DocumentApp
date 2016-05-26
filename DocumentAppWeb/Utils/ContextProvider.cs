using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;

namespace DocumentAppWeb.Utils
{
    public class ContextProvider
    {
        private static readonly string SPHostUrl = WebConfigurationManager.AppSettings.Get("SPHostUrl");

        public static ClientContext CreateAppOnlyContext()
        {
            //https://asdfit.sharepoint.com/test"
            var hostWeb = new Uri(SPHostUrl);
            var sharepointPrincipalId = "00000003-0000-0ff1-ce00-000000000000";

            var token =
                TokenHelper.GetAppOnlyAccessToken(sharepointPrincipalId, hostWeb.Authority, null).AccessToken;

            return TokenHelper.GetClientContextWithAccessToken(hostWeb.ToString(), token);
        }
    }
}