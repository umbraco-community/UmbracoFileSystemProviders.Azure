namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System.Configuration;

    public class ConfigurationHelper
    {
        public static string GetAppSetting(string key, string providerAlias)
        {
            return ConfigurationManager.AppSettings[$"{key}:{providerAlias}"];
        }
    }
}
