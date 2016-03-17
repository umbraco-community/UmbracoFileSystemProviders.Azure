# UmbracoFileSystemProviders.Azure

![Image Alt](build/assets/logo/azure-logo-256.png)

[![Build status](https://ci.appveyor.com/api/projects/status/mn5hxj5ijurwih7q?svg=true)](https://ci.appveyor.com/project/JamesSouth/umbracofilesystemproviders-azure)

An [Azure Blob Storage](http://azure.microsoft.com/en-gb/develop/net/) IFileSystem provider for [Umbraco](https://umbraco.com) 6.25+. 
Used to offload static files in the media section to the cloud.

Designed to supersede [UmbracoAzureBlobStorage](https://github.com/idseefeld/UmbracoAzureBlobStorage) by [Dirk Seefeld](https://twitter.com/dseefeld65) (With his blessing) this package allows the storage and retrieval of media items using Azure Blob Storage while retaining the relative paths to the files expected in the back office.

## Installation

Both NuGet and Umbraco packages are available. If you use NuGet but would like the benefit of the Umbraco configuration wizard you can install the Umbraco package first, use the wizard, then install the NuGet package, the configuration will be maintained.

|NuGet Packages    |Version           |
|:-----------------|:-----------------|
|**Release**|[![NuGet download](http://img.shields.io/nuget/v/UmbracoFileSystemProviders.Azure.svg)](https://www.nuget.org/packages/UmbracoFileSystemProviders.Azure/)|[![NuGet count](https://img.shields.io/nuget/dt/UmbracoFileSystemProviders.Azure.svg)](https://www.nuget.org/packages/UmbracoFileSystemProviders.Azure/)|
|**Pre-release**|[![MyGet download](https://img.shields.io/myget/umbracofilesystemproviders-azure/vpre/UmbracoFileSystemProviders.Azure.svg)](https://www.myget.org/gallery/umbracofilesystemproviders-azure)|[![MyGet count](https://img.shields.io/myget/umbracofilesystemproviders-azure/dt/UmbracoFileSystemProviders.Azure.svg)](https://www.myget.org/gallery/umbracofilesystemproviders-azure)|

|Umbraco Packages  |                  |
|:-----------------|:-----------------|
|**Release**|[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/collaboration/umbracofilesystemprovidersazure/) 
|**Pre-release**| [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/JamesSouth/umbracofilesystemproviders-azure/build/artifacts)

## Manual build

If you prefer, you can compile UmbracoFileSystemProviders.Azure yourself, you'll need:

* Visual Studio 2015 (or above)

To clone it locally click the "Clone in Windows" button above or run the following git commands.

```bash
git clone https://github.com/JimBobSquarePants/UmbracoFileSystemProviders.Azure
cd UmbracoFileSystemProviders.Azure
.\build.cmd
```

In the interim code reviews and pull requests would be most welcome!

## Usage

**Note:** Upon release most of configuration this will be automated.

Update `~/Config/FileSystemProviders.config` replacing the default provider with the following:

```xml
<?xml version="1.0"?>
<FileSystemProviders>
  <Provider alias="media" type="Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure" />
</FileSystemProviders>
```

And set the connection string and other configuration properties in the appsettings in the `web.config`.

```xml
<configuration>
  <appSettings>
    <!--Disables the built in Virtual Path Provider which allows for relative paths-->
    <add key="AzureBlobFileSystem.DisableVirtualPathProvider" value="true" />
	<add key="AzureBlobFileSystem.ConnectionString" value="DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]" />
	<add key="AzureBlobFileSystem.ContainerName" value="media" />
	<add key="AzureBlobFileSystem.MaxDays" value="365" />
  </appSettings>
</configuration>
```

Developmental mode configuration using the [Azure Storage Emulator](https://azure.microsoft.com/en-us/documentation/articles/storage-use-emulator/) for testing is as follows:

```xml
<configuration>
  <appSettings>
    <!--Disables the built in Virtual Path Provider which allows for relative paths-->
    <add key="AzureBlobFileSystem.DisableVirtualPathProvider" value="true" />
	<add key="AzureBlobFileSystem.ConnectionString" value="UseDevelopmentStorage=true" />
	<add key="AzureBlobFileSystem.ContainerName" value="media" />
	<add key="AzureBlobFileSystem.MaxDays" value="365" />
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

As of ImageProcessor.Web version [4.3.2](https://www.nuget.org/packages/ImageProcessor.Web/4.3.2) a new [`IImageService`](http://imageprocessor.org/imageprocessor-web/extending/#iimageservice) implementation has been available called `CloudImageService`. To enable that service and pull images directly from the cloud simply install the [configuration package](https://www.nuget.org/packages/ImageProcessor.Web.Config/) and replace the `CloudImageService`setting with the following:

```xml
<?xml version="1.0"?>
<security>
  <services>
    <service name="LocalFileImageService" type="ImageProcessor.Web.Services.LocalFileImageService, ImageProcessor.Web"/>
    <service prefix="media/" name="CloudImageService" type="ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web">
      <settings>
        <setting key="MaxBytes" value="8194304"/>
        <setting key="Timeout" value="30000"/>
        <setting key="Host" value="http://[myAccountName].blob.core.windows.net/media/"/>
      </settings>
    </service>
  </services>  
</security>
```

Be sure to install the [AzureBlobCache](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/) plugin to get the most out of the package.

## Authors

 - James Jackson-South
 - Dirk Seefeld
 - Lars-Erik Aabech
 - Jeavon Leopold

## Thanks
 - Elijah Glover for writing the [Umbraco S3 Provider](https://github.com/ElijahGlover/Umbraco-S3-Provider) which provided inspiration and some snazzy unit testing code for this project.

