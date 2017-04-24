angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelSelectController', ['$scope', '$localStorage', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, $localStorage, bladeNavigationService, catalogPublishingApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.channel = $localStorage.catalogPublishingChannel;

        blade.toolbarCommands = [{
            name: 'platform.commands.save',
            icon: 'fa fa-save',
            canExecuteMethod: function () {
                return true;
            },
            executeMethod: function () {
                var channel = _.find($scope.channels, function (c) { return c.id === $scope.channel.id });
                $localStorage.catalogPublishingChannel = channel;
                catalogPublishingApi.evaluateChannelProducts({ id: channel.id }, [blade.productId]);
                bladeNavigationService.closeBlade(blade);
                bladeNavigationService.closeBlade(blade.parentBlade);
                bladeNavigationService.showBlade(blade.parentBlade);
            }
        }];

        catalogPublishingApi.searchChannels({
            skip: 0,
            take: 1000
        }, function (response) {
            $scope.channels = response.results;
        });
    }]);