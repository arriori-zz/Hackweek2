module.controller('AppController', function ($scope, $data, $localStorage, $http) {

    ons.ready(function () {

        if ($localStorage.credentials) {

            if ($localStorage.token) {
                $http.defaults.headers.common.Authorization = 'Bearer ' + $localStorage.token;
            }

            $scope.showPage("quickRoom");

        }
        else {
            $scope.loginmodal.show();
        }
    });

    $scope.closeLoginModal = function () {

        $scope.loginmodal.hide();
        $scope.showPage("quickRoom");
    };

    $scope.startLoading = function () {
        $scope.loadingmodal.show();
    }

    $scope.endLoading = function () {
        $scope.loadingmodal.hide();
    }

    $scope.showPage = function (pageName) {
        $scope.tabbar.loadPage(pageName + '.html');
    };

   /* $rootScope.$on("CookieRefreshed", function () {
        $scope.showPage("discounts");
    });

    $rootScope.$on("InvalidLogin", function () {
        $scope.loginmodal.show();
    });
    */

    $scope.openLoginModal = function () {

        $scope.loginmodal.show();
     };
   
});




