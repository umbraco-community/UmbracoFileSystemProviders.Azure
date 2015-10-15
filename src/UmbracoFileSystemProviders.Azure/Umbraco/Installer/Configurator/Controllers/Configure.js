var configApp = angular.module('UFSPLoader', []);

configApp.controller("Loader", function ($scope, $http, $log) {
    var getDataUrl = "/Umbraco/backoffice/FileSystemProviders/Installer/GetParameters";
    var postDataUrl = "/Umbraco/backoffice/FileSystemProviders/Installer/PostParameters";   

    $scope.saved = false;

    // Ajax request to controller for data-
    $http.get(getDataUrl).success(function (data) {
        $scope.parameters = data;
    });

    $scope.submitForm = function (e) {
        e.preventDefault();

        $http.post(postDataUrl, $scope.parameters).success(function (data) {
            $scope.saved = true;
            $scope.status = data;            
        });

    }
});
