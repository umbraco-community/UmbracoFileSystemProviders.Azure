using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Our.Umbraco.FileSystemProviders.Azure.Helpers
{
    public static class ConfigurationHelper
    {
        public static string GetAppSetting(string key)
        {
            var settings = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrEmpty(settings))
            {
                return settings;
            }

            return ConfigurationManager.AppSettings[key.Replace(".", "-")];
        }

        public static string GetAppSetting(string key, string providerAlias)
        {
            var settings = ConfigurationManager.AppSettings[$"{key}:{providerAlias}"];

            if (!string.IsNullOrEmpty(settings))
            {
                return settings;
            }

            return ConfigurationManager.AppSettings[$"{key.Replace(".", "-")}-{providerAlias}"];
        }
    }
}
