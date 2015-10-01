module.controller('RoomsController', function ($scope, $data, $http) {

    $scope.selectedLocation = $data.selectedLocation;
    $scope.rooms = $data.rooms;
    $scope.lifesize = false;

    $scope.changeLocation = function () {
        $scope.locationModal.show();
    }

    $scope.selectRoom = function (room) {

    }
});


/*$scope.items = $data.items;

   $scope.showDetail = function (index) {
       var delegate = new cordova.plugins.locationManager.Delegate();

       delegate.didDetermineStateForRegion = function (pluginResult) {
           $scope.text = '[DOM] didDetermineStateForRegion: ' + JSON.stringify(pluginResult);
       };

       delegate.didStartMonitoringForRegion = function (pluginResult) {
           $scope.text = 'didStartMonitoringForRegion: ' + pluginResult;
       };

       delegate.didRangeBeaconsInRegion = function (pluginResult) {

           $scope.text = '[DOM] didRangeBeaconsInRegion: ' + JSON.stringify(pluginResult);
       };

       var region = createBeacon();

       cordova.plugins.locationManager.setDelegate(delegate);

       // required in iOS 8+
       cordova.plugins.locationManager.requestWhenInUseAuthorization();
       // or cordova.plugins.locationManager.requestAlwaysAuthorization()

       cordova.plugins.locationManager.startMonitoringForRegion(region)
           .fail(console.error)
           .done();
      };*/


/*
function createBeacon() {

    var uuid = 'FDA50693-A4E2-4FB1-AFCF-C6EB07647825'; // mandatory
    var identifier = 'IG MVD'; // mandatory
  //  var minor = 1000; // optional, defaults to wildcard if left empty
    var major = 10004; // optional, defaults to wildcard if left empty

    // throws an error if the parameters are not valid
    var beaconRegion = new cordova.plugins.locationManager.BeaconRegion(identifier, uuid, major);

    return beaconRegion;
}*/

