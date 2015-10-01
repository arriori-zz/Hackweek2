module.controller('LoginController', function ($scope, $data) {
    $scope.data = {};
    $scope.data.username = "reportplususer@infragistics.com";
    $scope.data.password = "%baG7cadence";

    $scope.signIn = function () {

        if ($scope.data && $scope.data.username && $scope.data.password)
        {
            $data.login($scope.data.username, $scope.data.password).then(function (result) {
                if (result) {
                    $scope.$parent.closeLoginModal();
                }
                else {
                    ons.notification.alert({ message: "Invalid email / password", title: 'Error' });
                }
            });
        }
        else {
            ons.notification.alert({ message: "Invalid email / password", title: 'Error' });
        }
    };

});