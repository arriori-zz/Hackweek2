module.controller('DetailController', function ($scope, $data) {
    $scope.item = $data.selectedItem;

     $scope.doSomething = function () {
            setTimeout(function () {
                ons.notification.alert({ message: 'tapped' });
            }, 100);
        };
});