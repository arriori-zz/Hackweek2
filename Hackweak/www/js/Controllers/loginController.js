module.controller('LoginController', function ($scope, $data) {
    $scope.data = {};
    $scope.data.username = "";
    $scope.data.password = "";

    $scope.signIn = function () {

        if ($scope.data && $scope.data.username && $scope.data.password)
        {
            $scope.$parent.startLoading();

            $data.login($scope.data.username, $scope.data.password).then(function (result) {
                $scope.$parent.endLoading();
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