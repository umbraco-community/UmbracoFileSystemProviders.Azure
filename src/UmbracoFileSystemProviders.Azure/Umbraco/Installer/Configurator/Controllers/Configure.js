var configApp = angular.module('UFSPLoader', []);

configApp.controller("Loader", function ($scope, $http) {
    $scope.greeting = 'hi from the controller';
});
