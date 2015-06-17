# UmbracoFileSystemProviders.Azure

An [Azure Blob Storage](http://azure.microsoft.com/en-gb/develop/net/) IFileSystem provider for Umbraco 6.25+. 
Used to offload static files in the media section to the cloud.

Designed to supersede [UmbracoAzureBlobStorage](https://github.com/idseefeld/UmbracoAzureBlobStorage) by [Dirk SeeField](https://twitter.com/dseefeld65) (With his blessing) this package allows the storage and retrieval of media items using Azure Blob Storage while retaining the relative paths to the files expected in the back office.

## Installation
At present the code is pre-release but when ready it will be available on [Nuget](http://www.nuget.org), also maybe as a package on [Our Umbraco](https://our.umbraco.org/). 

In the interim code reviews and pull requests would be most welcome! (Appveyor, MyGet, Nuget config etc..)

## Usage

**Note:** Upon release most of configuration this will be automated.

Update `~/Config/FileSystemProviders.config` replacing the default provider with the following:

```xml
<?xml version="1.0"?>
<FileSystemProviders>
  <Provider alias="media" type="Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure">
    <Parameters>
      <add key="containerName" value="media" />
      <add key="rootUrl" value="http://[myAccountName].blob.core.windows.net/" />
      <add key="connectionString" value="DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]"/>
      <!--
        Optional configuration value determining the maximum number of days to cache items in the browser.
        Defaults to 365 days.
      -->
      <add key="maxDays" value="365" />
    </Parameters>
  </Provider>
</FileSystemProviders>
```

Developmental mode configuration using the [Azure Storage Emulator](https://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/) for testing is as follows:

```xml
<?xml version="1.0"?>
<FileSystemProviders>
  <Provider alias="media" type="Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure">
    <Parameters>
      <add key="containerName" value="media" />
      <add key="rootUrl" value="http://127.0.0.1:10000/devstoreaccount1/" />
      <add key="connectionString" value="UseDevelopmentStorage=true"/>
    </Parameters>
  </Provider>
</FileSystemProviders>
```

Additionally the provider can be further configured with the following application setting in the `web.config`.

```xml
<?xml version="1.0"?>
<configuration>
  <appSettings>
    <!--Disables the built in Virtual Path Provider which allows for relative paths-->
    <add key="AzureBlobFileSystem.DisableVirtualPathProvider" value="true" />
    <!--
      Enables the development mode for testing. Addition changes to the FileSystemProviders.config are also required
    -->
    <add key="AzureBlobFileSystem.UseStorageEmulator" value="true" />
  </appSettings>
</configuration>
```

## Virtual Path Provider
By default the plugin will serve files transparently from your domain or serve media directly from Azure. This is made possible by using a custom [Virtual Path Provider](https://msdn.microsoft.com/en-us/library/system.web.hosting.virtualpathprovider%28v=vs.110%29.aspx) included and automatically initialised upon application startup. This can be disable by adding the configuration setting noted above.

**Note:** Virtual Path Providers may affect performance/caching depending on your setup as the process differs from IIS's [unmanaged handler](http://www.paraesthesia.com/archive/2011/05/02/when-staticfilehandler-is-not-staticfilehandler.aspx/). Virtual files sent via the provider though are correctly cached in the browser so this shouldn't be an issue.

The following configuration is required in your `web.config` to enable static file mapping in IIS Express.

```xml
<?xml version="1.0"?>
  <configuration>
    <location path="Media">
      <system.webServer>
        <handlers>
          <remove name="StaticFileHandler" />
          <add name="StaticFileHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.StaticFileHandler" />
        </handlers>
      </system.webServer>
    </location>
  </configuration>
```
  
## Combining with ImageProcessor

As of ImageProcessor.Web version [4.3.2](https://www.nuget.org/packages/ImageProcessor.Web/4.3.2) a new [`IImageService`](http://imageprocessor.org/imageprocessor-web/extending/#iimageservice) implementation has been available called `CloudImageService`. To enable that service and pull images directly from the cloud simply install the [configuration package](https://www.nuget.org/packages/ImageProcessor.Web.Config/) and replace the `LocalFileImageService`setting with the following:

```xml
<?xml version="1.0"?>
<security>
  <services>
    <!--Disable the LocalFileImageService and enable this one when using virtual paths. -->
    <service name="CloudImageService" type="ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web">
      <settings>
        <setting key="MaxBytes" value="8194304"/>
        <setting key="Timeout" value="30000"/>
        <setting key="Host" value="http://[myAccountName].blob.core.windows.net/"/>
      </settings>
    </service>
</security>
```

Be sure to install the [AzureBlobCache](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/) plugin to get the most out of the package.

## Authors

 - James Jackson-South
 - Dirk Seefield
 - Lars Lars-Erik Aabech

## Thanks
 - Elijah Glover for writing the [Umbraco S3 Provider](https://github.com/ElijahGlover/Umbraco-S3-Provider) which provided inspiration and some snazzy unit testing code for this project.

