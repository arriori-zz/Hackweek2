module.controller('QuickRoomController', function ($scope, $data) {

    $scope.init = function () {
        $scope.lifesize = true;
        $scope.time = 30;
        $scope.booked = false;

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
        $data.bookRoom($scope.lifesize, $scope.time).then(function (result) {
            if (result.Booked) {
                $scope.bookedRoom = result.Room.Name;

                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.booked = true;
            }
            else {

                $scope.nextRoom = result.Room.Name;
                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.nextRoomTime = $scope.getTime(result.Start);
                $scope.possibleBook = result;
                $scope.noRoomModal.show();
                
            }
        });
    }

    $scope.dontBook = function(){
        $scope.noRoomModal.hide();
    }

    $scope.pleaseBook = function () {

        $data.bookThisRoom($scope.possibleBook).then(function (result) {
            if (result.Booked) {
                $scope.bookedRoom = result.Room.Name;

                $scope.bookedTimeEnd = $scope.getTime(result.End);
                $scope.booked = true;
            }
            else {
                alert("Ouch! somone just took the room.");
            }
        })

        $scope.noRoomModal.hide();
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

module.controller('NoRoomController', function ($scope, $data) {
   
    $scope.yes = function () {
        $scope.$parent.pleaseBook();
    }

    $scope.no = function () {
        $scope.$parent.dontBook();
    };

});
