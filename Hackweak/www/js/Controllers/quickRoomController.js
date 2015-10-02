module.controller('QuickRoomController', function ($scope, $data, $localStorage) {

    $scope.init = function () {
        $scope.lifesize = true;
        $scope.time = 30;

        if ($localStorage.bookedRoom) {
            $scope.bookedRoom = $localStorage.bookedRoom;
            $scope.bookedTimeEnd = $scope.getTime($scope.bookedRoom.End);
            $scope.state = 'booked';
        }
        else {
            $scope.state = 'registering';
        }
        

        $data.getLocations().then(function (result) {
            $scope.selectedLocation = result[0];
        });
    }
    
    $scope.closeLocationModal = function () {
        $scope.selectedLocation = $data.selectedLocation;
        $scope.locationModal.hide();
    }

    $scope.changeLocation = function () {
        $scope.locationModal.show();
    }

    $scope.addMinutes = function () {
        $scope.time += 15;
    };

    $scope.removeMinutes = function () {
        if ($scope.time > 15) {
            $scope.time -= 15;
        }
    };

    $scope.getTime = function (datetime) {

        var parts = datetime.split('T')[1].split(':');

        var hours = parts[0] - 3;
        var minutes = parts[1];

        return hours + ":" + minutes;
    };

    $scope.bookRoom = function () {
        $scope.$parent.startLoading();

        $data.bookRoom($scope.lifesize, $scope.time).then(function (result) {
            $scope.$parent.endLoading();

            if (result.Booked) {

                $scope.bookedRoom = result;
                $localStorage.bookedRoom = result;
                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.state = 'booked';
            }
            else {
                $scope.nextRoom = result.Room.Name;
                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.nextRoomTime = $scope.getTime(result.Start);
                $scope.possibleBook = result;
                $scope.state = 'notBooked';
            }
        });
    }

    $scope.dontBook = function() {
        $scope.state = 'registering';
    }

    $scope.pleaseBook = function () {
        $scope.$parent.startLoading();
        $data.bookThisRoom($scope.possibleBook).then(function (result) {
            $scope.$parent.endLoading();
            if (result.Booked) {
                $scope.bookedRoom = result;
                $localStorage.bookedRoom = result;
                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.state = 'booked';
            }
            else {
                alert("Ouch! someone just took the room.");
            }
        })
    }

    $scope.freeRoom = function () {
        $scope.$parent.startLoading();
        $data.freeRoom($scope.bookedRoom.Room.Id, $scope.bookedRoom).then(function (result) {
            $scope.$parent.endLoading();
            $localStorage.bookedRoom = null;
            $scope.state = 'registering';

        });
    }

    $scope.updateMeeting = function () {

        $scope.$parent.startLoading();
        $data.addMinutes($scope.bookedRoom.Room.Id, $scope.bookedRoom, 15).then(function (result) {
            $scope.$parent.endLoading();

            $scope.bookedRoom = result;
            $localStorage.bookedRoom = result;
            $scope.bookedTimeEnd = $scope.getTime(result.End);
        });
    }

    $scope.logoutClick = function () {
        $scope.logoutModal.show();
    }

    $scope.init();
});

module.controller('LocationController', function ($scope, $data) {
    $data.getLocations().then(function (result) {
        $scope.locations = result;
    });
   
    $scope.selectedLocation = $data.selectedLocation;

    $scope.init = function () {

    }

    $scope.selectLocation = function (location) {
        $data.selectedLocation = location;

        $scope.$parent.closeLocationModal();
    }
    
    $scope.init();
});


module.controller('LogoutController', function ($scope, $data, $localStorage) {

    $scope.logout = function () {

     $localStorage.credentials = null;
     $localStorage.token = null;
     $localStorage.bookedRoom = null;
     $scope.$parent.$parent.openLoginModal();
    }

});
