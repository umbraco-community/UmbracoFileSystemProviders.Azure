# UmbracoFileSystemProviders.Azure v2

**This version is for Umbraco v8 only**. For the v1 package for **Umbraco v7** please visit the [main branch](https://github.com/umbraco-community/UmbracoFileSystemProviders.Azure)

![Image Alt](build/assets/logo/azure-logo-256.png)

[![Build status](https://ci.appveyor.com/api/projects/status/oicfg95tvptrhntn/branch/develop-umbraco-version-8?svg=true)](https://ci.appveyor.com/project/Umbraco-Community/umbracofilesystemproviders-azure/branch/develop-umbraco-version-8)

An [Azure Blob Storage](http://azure.microsoft.com/en-gb/develop/net/) IFileSystem provider for [Umbraco](https://umbraco.com) 
Used to offload static files in the media section to the cloud.

This package allows the storage and retrieval of media items using Azure Blob Storage while retaining the relative paths to the files expected in the back office.

**v2 requires Umbraco v8.1.0+**

## Installation

Both NuGet and Umbraco packages are available. If you use NuGet but would like the benefit of the Umbraco configuration wizard you can install the Umbraco package first, use the wizard, then install the NuGet package, the configuration will be maintained.

From **v2.0.0-alpha3** onwards this package was split into 2 NuGet packages and an additional one was added to support Umbraco Forms. When using NuGet install the `UmbracoFileSystemProviders.Azure.Media` package to swap Media storage to Blobs.


|NuGet Packages    |Version           |
|:-----------------|:-----------------|
|**Pre-Release**|[![NuGet download](http://img.shields.io/nuget/vpre/UmbracoFileSystemProviders.Azure.svg)](https://www.nuget.org/packages/UmbracoFileSystemProviders.Azure/)|[![NuGet count](https://img.shields.io/nuget/dt/UmbracoFileSystemProviders.Azure.svg)](https://www.nuget.org/packages/UmbracoFileSystemProviders.Azure/)|
|**Bleeding edge Core**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/UmbracoFileSystemProviders.Azure.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure)|[![MyGet count](https://img.shields.io/myget/umbraco-packages/dt/UmbracoFileSystemProviders.Azure.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure)|
|**Bleeding edge Media**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/UmbracoFileSystemProviders.Azure.Media.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure.Media)|[![MyGet count](https://img.shields.io/myget/umbraco-packages/dt/UmbracoFileSystemProviders.Azure.Media.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure.Media)|
|**Bleeding edge Forms**|[![MyGet download](https://img.shields.io/myget/umbraco-packages/vpre/UmbracoFileSystemProviders.Azure.Forms.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure.Forms)|[![MyGet count](https://img.shields.io/myget/umbraco-packages/dt/UmbracoFileSystemProviders.Azure.Forms.svg)](https://www.myget.org/feed/umbraco-packages/package/nuget/UmbracoFileSystemProviders.Azure.Forms)|

|Umbraco Packages  |                  |
|:-----------------|:-----------------|
|**Release**|[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-orange.svg)](https://our.umbraco.org/projects/collaboration/umbracofilesystemprovidersazure/) 
|**Pre-release**| [![AppVeyor Artifacts](https://img.shields.io/badge/appveyor-umbraco-orange.svg)](https://ci.appveyor.com/project/Umbraco-Community/umbracofilesystemproviders-azure/build/artifacts)

## Manual build

If you prefer, you can compile UmbracoFileSystemProviders.Azure yourself, you'll need:

* Visual Studio 2019 (or above)

To clone it locally click the "Clone in Windows" button above or run the following git commands.

```bash
git clone https://github.com/umbraco-community/UmbracoFileSystemProviders.Azure
cd UmbracoFileSystemProviders.Azure
.\build.cmd
```

In the interim code reviews and pull requests would be most welcome!

## Media

### Configuration via Web.Config

In `Web.config` create the new application keys 

```xml
<add key="AzureBlobFileSystem.ConnectionString:media" value="DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]" />
<add key="AzureBlobFileSystem.ContainerName:media" value="media" />
<add key="AzureBlobFileSystem.RootUrl:media" value="https://[myAccountName].blob.core.windows.net/" />
<add key="AzureBlobFileSystem.MaxDays:media" value="365" />
<add key="AzureBlobFileSystem.UseDefaultRoute:media" value="true" />
<add key="AzureBlobFileSystem.UsePrivateContainer:media" value="false" />
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

### Virtual Path Provider
By default the plugin will serve files transparently from your domain or serve media directly from Azure. This is made possible by using a custom [Virtual Path Provider](https://msdn.microsoft.com/en-us/library/system.web.hosting.virtualpathprovider%28v=vs.110%29.aspx) included and automatically initialised upon application startup. This can be disabled by adding the configuration setting noted above.

**Note:** Virtual Path Providers may affect performance/caching depending on your setup as the process differs from IIS's [unmanaged handler](http://www.paraesthesia.com/archive/2011/05/02/when-staticfilehandler-is-not-staticfilehandler.aspx/). Virtual files sent via the provider though are correctly cached in the browser so this shouldn't be an issue. VPP providers also **don't work** with **Precompiled sites** or when used in a **virtual directory/application**.

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

Also add this configuration to the `web.config` inside the `Media` folder

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
	<system.webServer>
		<handlers>
			<clear />
			<add name="StaticFileHandler" path="*" verb="*" preCondition="integratedMode" type="System.Web.StaticFileHandler" />
			<add name="StaticFile" path="*" verb="*" modules="StaticFileModule,DefaultDocumentModule,DirectoryListingModule" resourceType="Either" requireAccess="Read" />
		</handlers>
	</system.webServer>
</configuration>
```
  
### Combining with ImageProcessor

ImageProcessor.Web contains a [`IImageService`](http://imageprocessor.org/imageprocessor-web/extending/#iimageservice) called `CloudImageService`, to enable that service and pull images directly from 
the cloud replace the `CloudImageService`setting with the following:

```xml
<?xml version="1.0"?>
<security>
  <services>
    <service name="LocalFileImageService" type="ImageProcessor.Web.Services.LocalFileImageService, ImageProcessor.Web"/>
    <service prefix="media/" name="CloudImageService" type="ImageProcessor.Web.Services.CloudImageService, ImageProcessor.Web">
      <settings>
        <setting key="Container" value="media"/>
        <setting key="MaxBytes" value="8194304"/>
        <setting key="Timeout" value="30000"/>
        <setting key="Host" value="https://[myAccountName].blob.core.windows.net/media"/>
      </settings>
    </service>
  </services>  
</security>
```
**Note** The `CloudImageService`is not compatible with the FileSystemProvider when using private storage. You can instead use the `AzureImageService` which is included with the [AzureBlobCache](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/) package

Optionally install the [AzureBlobCache](http://imageprocessor.org/imageprocessor-web/plugins/azure-blob-cache/) plugin to get the most out of the package.

## Umbraco Forms

Currently this package is available only via NuGet

    Install-Package UmbracoFileSystemProviders.Azure.Forms -pre

### Configuration via Web.Config

In `Web.config` update the new application keys with the required credentials

```xml
<add key="AzureBlobFileSystem.ContainerName:forms" value="forms-data" />
<add key="AzureBlobFileSystem.RootUrl:forms" value="https://[myAccountName].blob.core.windows.net/" />
<add key="AzureBlobFileSystem.ConnectionString:forms" value="DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]" />
<add key="AzureBlobFileSystem.UsePrivateContainer:forms" value="false" />
```

## Authors

 - James Jackson-South
 - Dirk Seefeld
 - Lars-Erik Aabech
 - Jeavon Leopold

## Thanks
 - Elijah Glover for writing the [Umbraco S3 Provider](https://github.com/ElijahGlover/Umbraco-S3-Provider) which provided inspiration and some snazzy unit testing code for this project.

