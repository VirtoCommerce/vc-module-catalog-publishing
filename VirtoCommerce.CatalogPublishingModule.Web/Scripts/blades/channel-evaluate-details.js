angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelEvaluateDetailsController', ['$rootScope', '$scope', function ($rootScope, $scope) {
        var blade = $scope.blade;
        blade.headIcon = 'fa fa-calculator';
        blade.isLoading = false;

        $scope.$on('new-notification-event', function (event, notification) {
            if (blade.notification && notification.id == blade.notification.id) {
                angular.copy(notification, blade.notification);
                if (notification.finished) {
                    $rootScope.$broadcast('channel-evaluation-finished');
                }
            }
        });
    }]);