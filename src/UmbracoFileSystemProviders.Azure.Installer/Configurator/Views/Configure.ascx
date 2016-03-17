<%@ Control Language="c#" AutoEventWireup="True" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<%@ Import Namespace="Our.Umbraco.FileSystemProviders.Azure.Installer" %>

<%
    string angularSrc = "/umbraco/lib/angular/1.1.5/angular.min.js";
    string span = string.Empty;
    if (Helpers.GetUmbracoVersion().Major < 7)
    {
        angularSrc = "//cdnjs.cloudflare.com/ajax/libs/angular.js/1.1.5/angular.js";
        span = " class=\"span12\"";
%>
    <link href="//cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/2.3.2/css/bootstrap.css" type="text/css" rel="stylesheet"/>
<%
    }
%>

<script src="<%=angularSrc%>"></script>
<script src="/App_Plugins/UmbracoFileSystemProviders/Azure/Install/Configurator/Controllers/Configure.js"></script>

<div ng-app ="UFSPLoader">
    <div ng-controller="Loader">
        <div class="row">        
            <div class="span1">
                <img src="/App_Plugins/UmbracoFileSystemProviders/Azure/Install/azure-logo-32.png"/>
            </div>
            <div><h4>Umbraco Azure File System Provider</h4></div>
        </div>
        <div class="row">
            <div><hr /></div>
        </div>
        <div class="row" ng-show="!saved">  
            <div<%=span %>>
                <fieldset>
                    <legend><h4>To complete installation, please enter the required parameters for the Azure storage provider below</h4></legend>             
                    <form name="paramForm" class="form-horizontal" role="form">             
                        <div ng-repeat="param in parameters" class="control-group">
                            <ng-form name="form">
                            <label class="control-label" for="param.Key">{{ capitalizeFirstLetter(param.Key) }}</label>
                                <div class="controls">
                                    <span ng-if="getInputType(param.Key) === 'checkbox'" ng-include="'/App_Plugins/UmbracoFileSystemProviders/Azure/Install/Configurator/Views/checkbox.htm'"></span>
                                    <span ng-if="getInputType(param.Key) === 'text'" ng-include="'/App_Plugins/UmbracoFileSystemProviders/Azure/Install/Configurator/Views/textfield.htm'"></span>
                                </div>
                                <span data-ng-show=" {{'form.'+param.Key+'.$dirty && form.'+param.Key+'.$error.required'}}">Required!</span>
                            </ng-form>
                        </div>
                        <button preventDefault class="btn btn-primary" ng-disabled="paramForm.$invalid" ng-click="submitForm($event)">Save</button>
                    </form>
                </fieldset>
            </div>
        </div> 
        <div class="row" ng-show="!saved">
            <div><hr /></div>
         </div>

        <div class="row">
            <div<%=span %>>
                <div class="alert alert-success" ng-show="saved && (status === 'Ok' || status === 'ImageProcessorWebCompatibility' || status === 'ImageProcessorWebConfigError')">
                   The Azure storage provider was sucessfully configured and your media is now as light as candyfloss
                </div>
                <div class="alert alert-info" ng-show="saved && status === 'ImageProcessorWebCompatibility'">
                   <strong>Warning!</strong> You need to upgrade the installed version of ImageProcessor.Web to v4.3.2+ (reinstall this package once you've updated) to be fully compatible with this package <a href="https://our.umbraco.org/projects/collaboration/imageprocessor/" target="_blank">https://our.umbraco.org/projects/collaboration/imageprocessor/</a>
                </div>
                <div class="alert alert-info" ng-show="saved && status === 'ImageProcessorWebConfigError'">
                   <strong>Warning!</strong> The installer was unable to configure ImageProcessor.Web, please see documentation to fix the issue <a href="https://our.umbraco.org/projects/collaboration/umbracofilesystemprovidersazure/" target="_blank">https://our.umbraco.org/projects/collaboration/umbracofilesystemprovidersazure/</a>
                </div>
                <div class="alert alert-error" ng-show="saved && status != 'Ok' && status != 'ImageProcessorWebCompatibility'">
                    <strong>Oh no</strong>, something went wrong saving, please check Umbraco log files for exceptions
                </div>
                <div class="alert alert-error" ng-show="!saved && status === 'ConnectionError'">
                    <strong>Oh no</strong>, there was something wrong with your Azure connection string, please check and try again, more info in the Umbraco log files
                </div>    
            </div>
        </div>
           
    </div>
</div>

