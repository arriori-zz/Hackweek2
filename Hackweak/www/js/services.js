var serverUrl = "http://localhost:26573";

module.factory('$data', function ($http, $q, $localStorage) {

      var data = {};

     //data.login = function (user, password) {
     //     data.credentials = {};
     //     data.credentials.UserName = user;
     //     data.credentials.Password = password;

     //     var deferred = $q.defer();

     //     $http.post(serverUrl + "/api/exchange/login", data.credentials)
     //       .success(function (response) {
     //           var credentials = {};
     //           credentials.userName = user;
     //           credentials.password = password;
     //           $localStorage.credentials = credentials;
     //           deferred.resolve(true);


     //       }).error(function () {
     //           deferred.resolve(false);
     //       });

     //     return deferred.promise;
     // };

     data.login = function (user, password) {

         var deferred = $q.defer();
         var params = { grant_type: "password", userName: user, password: password };

         var transform = function (obj) {
             var str = [];
             for (var p in obj)
                 str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
             return str.join("&");
         };

         $http.post(serverUrl + "/token", params, {
             headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
             transformRequest: transform
         }).success(function (response) {

             var credentials = {};
             credentials.userName = user;
             credentials.password = password;
             $localStorage.credentials = credentials;
             $localStorage.token = response.access_token;

             $http.defaults.headers.common.Authorization = 'Bearer ' + response.access_token;
                     
             deferred.resolve(true);
         }).error(function () {

                deferred.resolve(false);
             }
             
         );

         return deferred.promise;
     };

      data.getLocations = function () {
          var deferred = $q.defer();

          $http.get(serverUrl + "/api/exchange/getLocations", { cache: true })
           .success(function (response) {
               deferred.resolve(response);
           }).error(function () {
               deferred.reject();
           });

          return deferred.promise;
      }

      data.bookRoom = function (lifesize, time) {
          var deferred = $q.defer();

          var param = {
              Lifesize: lifesize,
              Time : time
          }

          $http.post(serverUrl + "/api/exchange/bookRoom", param)
           .success(function (response) {
               deferred.resolve(response);
           }).error(function () {
               deferred.reject();
           });

          return deferred.promise;
      }

      data.bookThisRoom = function (book) {
          var deferred = $q.defer();

          $http.post(serverUrl + "/api/exchange/bookThisRoom", book)
           .success(function (response) {
               deferred.resolve(response);
           }).error(function () {
               deferred.reject();
           });

          return deferred.promise;
      }

      return data;
 });

module.factory('authInterceptor', [
     '$q', '$injector', function authInterceptor($q, $injector, $http) {
         return {
             // optional method
             'responseError': function (rejection) {

                 if (isForbiddenResponse(rejection)) {

                     var deferred = $q.defer();

                     var $localStorage = $injector.get('$localStorage');
                     var $http = $injector.get('$http');

                     if ($localStorage.credentials) {


                         var params = { grant_type: "password", userName: $localStorage.credentials.userName, password: $localStorage.credentials.password };

                         var transform = function (obj) {
                             var str = [];
                             for (var p in obj)
                                 str.push(encodeURIComponent(p) + "=" + encodeURIComponent(obj[p]));
                             return str.join("&");
                         };

                         $http.post(serverUrl + "/token", params, {
                             headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
                             transformRequest: transform
                         }).success(function (response) {

                             var credentials = {};
                             credentials.userName = $localStorage.credentials.userName;
                             credentials.password = $localStorage.credentials.password;
                             $localStorage.token = response.access_token;

                             $http.defaults.headers.common.Authorization = 'Bearer ' + response.access_token;

                             function successCallback(response){
                                 deferred.resolve(response);
                             }

                             function errorCallback(response){
                                 deferred.reject(response);
                             }

                             rejection.config.headers.Authorization = 'Bearer ' + response.access_token;

                             $http(rejection.config).then(successCallback, errorCallback);

                         }).error(function () {

                             $localStorage.credentials = null;
                             $localStorage.token = null;

                             return deferred.reject(rejection);
                         });

                         return deferred.promise;
                     }
                     else {
                         return $q.reject(rejection);
                     }
                 }

                 return $q.reject(rejection);
             }
         };

         function isForbiddenResponse(rejection) {
             return rejection.status === 401;
         }
     }
 ]);
