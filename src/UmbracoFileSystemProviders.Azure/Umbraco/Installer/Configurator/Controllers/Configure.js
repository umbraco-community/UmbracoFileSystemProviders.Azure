var configApp = angular.module('UFSPLoader', []);

configApp.controller("Loader", function ($scope, $http, $log) {
    var dataUrl = "/Umbraco/backoffice/FileSystemProviders/Installer/GetParameters";
    // Ajax request to controller for data-
    $http.get(dataUrl).success(function (data) {
        $scope.parameters = data;
    });

    $scope.submitForm = function () {
        $log.debug($scope.entity);
    }
});
