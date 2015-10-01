module.controller('DetailController', function ($scope, $data) {
   
    $scope.roomName = "Mon A";
    $scope.begins = "10:30";
    $scope.ends = "11:00";

    $scope.freeRoom = function () {

        $scope.$parent.startLoading();

        $data.freeRoom($data.roomId).then(function (result) {

            $scope.$parent.endLoading();
           
        });
    }

    $scope.addMinutes = function () {
        $data.addMinutes($data.roomId, 15).then(function (result) {
            $scope.$parent.endLoading();
           
        });
    }

});