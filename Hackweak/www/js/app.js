var module = angular.module('app', ['onsen', 'ngStorage']);

module.config(function ($httpProvider) {
    $httpProvider.interceptors.push('authInterceptor');
});


//document.addEventListener("deviceready", function () {

//    window.LocationManager = cordova.plugins.LocationManager;
//    window.locationManager = cordova.plugins.locationManager;
//    window.Regions = locationManager.Regions;

//    window.Region = locationManager.Region;
//    window.Delegate = locationManager.Delegate;
//    window.CircularRegion = locationManager.CircularRegion;
//    window.BeaconRegion = locationManager.BeaconRegion;


//    window._ = cordova.require('com.unarin.cordova.beacon.underscorejs');

//    done();

//}, false);

