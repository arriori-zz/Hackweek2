module.controller('MyMeetingController', function ($scope, $data) {
    $scope.items = $data.items;

    $scope.showDetail = function (index) {
        var selectedItem = $data.items[index];
        $data.selectedItem = selectedItem;
        $scope.myMeetingsNavigator.pushPage('detail.html', { title: selectedItem.title });
    };
});