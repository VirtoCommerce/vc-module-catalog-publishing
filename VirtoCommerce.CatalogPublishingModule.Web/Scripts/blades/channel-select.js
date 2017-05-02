angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelSelectController', ['$scope', '$localStorage', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, $localStorage, bladeNavigationService, catalogPublishingApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.channel = blade.channel || $localStorage.catalogPublishingChannel;

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
                bladeNavigationService.closeBlade(blade.parentBlade, function () {
                    var newBlade = angular.extend({}, blade, { disableOpenAnimation: true });
                    var newParentBlade = angular.extend({}, blade.parentBlade, { disableOpenAnimation: true });
                    bladeNavigationService.showBlade(newParentBlade, blade.parentBlade.parentBlade);
                    bladeNavigationService.showBlade(newBlade, newParentBlade);
                });
            }
        }];

        var product = blade.parentBlade.currentEntity;
        var catalogIds = [];
        _.each(product.outlines, function (outline) {
            var catalogItem = _.find(outline.items, function (item) { return item.seoObjectType === 'Catalog' });
            if (catalogItem) {
                catalogIds.push(catalogItem.id);
            }
        });

        catalogPublishingApi.searchChannels({
            skip: 0,
            take: 1000,
            catalogIds: catalogIds,
        }, function (response) {
            $scope.channels = response.results;
        });
    }]);