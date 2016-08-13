var configApp = angular.module("UFSPLoader", []);

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

        $http.post(postDataUrl, $scope.parameters)
            .success(function (data) {

                var status;
                if (typeof data === "string") {
                    status = JSON.parse(data);
                } else {
                    status = data;
                }

                $scope.status = status;

                if (status !== "ConnectionError") {
                    $scope.saved = true;
                }

            });
    };

    $scope.capitalizeFirstLetter = function (string) {
        return string.charAt(0).toUpperCase() + string.slice(1);
    };

    $scope.getInputType = function (param) {
        return param.toUpperCase() === "USEDEFAULTROUTE" ? "checkbox" : "text";
    };
});
