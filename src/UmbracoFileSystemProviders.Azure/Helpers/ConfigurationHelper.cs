namespace Our.Umbraco.FileSystemProviders.Azure
{
    using System.Configuration;

    public class ConfigurationHelper
    {
        public static string GetAppSetting(string key, string providerAlias)
        {
            var settingValue = ConfigurationManager.AppSettings[key];
            if (settingValue != null)
            {
                return $"{settingValue}:{providerAlias}";
            }

            return null;
        }
    }
}
