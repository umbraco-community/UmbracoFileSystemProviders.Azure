<%@ Control Language="c#" AutoEventWireup="True" TargetSchema="http://schemas.microsoft.com/intellisense/ie5"%>
<script src="/umbraco/lib/angular/1.1.5/angular.min.js"></script>
<script src="/App_Plugins/UmbracoFileSystemProviders/Azure/Install/Configurator/Controllers/Configure.js"></script>

<div ng-app ="UFSPLoader">
    <div ng-controller="Loader">
          <fieldset>
            <legend><img src="/App_Plugins/UmbracoFileSystemProviders/Azure/Install/azure-logo-32.png"/> Please enter the required parameters for the Azure storage provider below</legend>             
            <form name="paramForm" class="form-horizontal" role="form">             
                <div ng-repeat="param in parameters" class="control-group">
                    <ng-form name="form">
                    <label class="control-label" for="param.Key">{{ param.Key }}</label>
                        <div class="controls">
                            <input 
                                class ="input-block-level"
                                dynamic-name="param.Key"
                                type="text"
                                ng-model="param.Value"                            
                                required
                            >
                        </div>
                        <span data-ng-show=" {{'form.'+param.Key+'.$dirty && form.'+param.Key+'.$error.required'}}">Required!</span>
                    </ng-form>
                </div>
                <button preventDefault class="btn btn-primary" ng-disabled="paramForm.$invalid" ng-click="submitForm($event)">Save</button>
            </form>
        </fieldset>
    </div>
</div>