module.controller('AppController', function ($scope, $data, $localStorage) {

    ons.ready(function () {

        if ($localStorage.credentials) {
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
   
});



