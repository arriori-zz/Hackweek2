module.controller('QuickRoomController', function ($scope, $data) {

    $scope.init = function () {
        $scope.lifesize = true;
        $scope.time = 30;
        $scope.state = 'registering';

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

        var hours = parts[0];
        var minutes = parts[1];

        return hours + ":" + minutes;
    };

    $scope.bookRoom = function () {
        $scope.$parent.startLoading();

        $data.bookRoom($scope.lifesize, $scope.time).then(function (result) {
            $scope.$parent.endLoading();

            if (result.Booked) {
                $scope.bookedRoom = result.Room.Name;

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

    $scope.dontBook = function(){
        $scope.state = 'registering';
    }

    $scope.pleaseBook = function () {
        $scope.$parent.startLoading();
        $data.bookThisRoom($scope.possibleBook).then(function (result) {
            $scope.$parent.endLoading();
            if (result.Booked) {
                $scope.bookedRoom = result.Room.Name;

                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.state = 'booked';
            }
            else {
                alert("Ouch! somone just took the room.");
            }
        })
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
     $scope.$parent.$parent.openLoginModal();
    }

});
