namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System.Configuration;

    public class ConfigurationHelper
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
