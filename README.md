# UmbracoFileSystemProviders.Azure

![Image Alt](build/assets/logo/azure-logo-256.png)

[![Build status](https://ci.appveyor.com/api/projects/status/mn5hxj5ijurwih7q?svg=true)](https://ci.appveyor.com/project/JamesSouth/umbracofilesystemproviders-azure)

An [Azure Blob Storage](http://azure.microsoft.com/en-gb/develop/net/) IFileSystem provider for [Umbraco](https://umbraco.com) 7.1.9+. 
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
      <!--
        When true this allows the VirtualPathProvider to use the default "media" route prefix regardless 
        of the container name.
      -->
      <add key="useDefaultRoute" value="true" />
      <!--
        When true blob containers will be private instead of public what means that you can't access the original blob file directly from its blob url.
      -->
      <add key="usePrivateContainer" value="false" />
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

### Configuration via Web.Config

**Available in v0.5.4+**

Optionally instead of having the configuration in `FileSystemProviders.config` it can be moved to `Web.config`

In `FileSystemProviders.config` remove the default parameters and add a new one with the key `alias`, the value should match the provider alias

```xml
<?xml version="1.0"?>
<FileSystemProviders>
  
  <!-- Media -->
  <Provider alias="media" type="Our.Umbraco.FileSystemProviders.Azure.AzureBlobFileSystem, Our.Umbraco.FileSystemProviders.Azure">
  	<Parameters>
  		<add key="alias" value="media"/>
  	</Parameters>
  </Provider>
   
</FileSystemProviders>
```

In `Web.config` create the new application keys and post fix each key with the `alias` defined in `FileSystemProviders.config` after a colon.

```xml
<add key="AzureBlobFileSystem.ConnectionString:media" value="DefaultEndpointsProtocol=https;AccountName=[myAccountName];AccountKey=[myAccountKey]" />
<add key="AzureBlobFileSystem.ContainerName:media" value="media" />
<add key="AzureBlobFileSystem.RootUrl:media" value="http://[myAccountName].blob.core.windows.net/" />
<add key="AzureBlobFileSystem.MaxDays:media" value="365" />
<add key="AzureBlobFileSystem.UseDefaultRoute:media" value="true" />
<add key="AzureBlobFileSystem.UsePrivateContainer:media" value="false" />
```

## Virtual Path Provider
By default the plugin will serve files transparently from your domain or serve media directly from Azure. This is made possible by using a custom [Virtual Path Provider](https://msdn.microsoft.com/en-us/library/system.web.hosting.virtualpathprovider%28v=vs.110%29.aspx) included and automatically initialised upon application startup. This can be disable by adding the configuration setting noted above.

**Note:** Virtual Path Providers may affect performance/caching depending on your setup as the process differs from IIS's [unmanaged handler](http://www.paraesthesia.com/archive/2011/05/02/when-staticfilehandler-is-not-staticfilehandler.aspx/). Virtual files sent via the provider though are correctly cached in the browser so this shouldn't be an issue. VVP providers also **don't work** with **Precompiled sites** or when used in a **virtual directory/application**.

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

For **Umbraco v7.5+ you must add the the StaticFileHandler** to the new Web.config inside the `Media` folder instead of the root one or the VPP will not work!

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
  
## Combining with ImageProcessor

As of ImageProcessor.Web version [4.3.2](https://www.nuget.org/packages/ImageProcessor.Web/4.3.2) a new [`IImageService`](http://imageprocessor.org/imageprocessor-web/extending/#iimageservice) implementation has been available called `CloudImageService`. To enable that service and pull images directly from 
the cloud simply install the [configuration package](https://www.nuget.org/packages/ImageProcessor.Web.Config/) and replace the `CloudImageService`setting with the following:

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
**Note** The `CloudImageService`is not compatible with the FileSystemProvider when using private storage. You will have to build your own `IImageService` implementation.

If using a version of ImageProcessor.Web version [4.5.0](https://www.nuget.org/packages/ImageProcessor.Web/4.5.0) the configuration details will need to be configured as follows:

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
        <setting key="Host" value="http://[myAccountName].blob.core.windows.net/media"/>
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

