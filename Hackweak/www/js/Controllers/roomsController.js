module.controller('RoomsController', function ($scope, $data) {
    $scope.items = $data.items;

    $scope.showDetail = function (index) {
        var selectedItem = $data.items[index];
        $data.selectedItem = selectedItem;
        $scope.RoomsNavigator.pushPage('detail.html', { title: selectedItem.title });
    };
});