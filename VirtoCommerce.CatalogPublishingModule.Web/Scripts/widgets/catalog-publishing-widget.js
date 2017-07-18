angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.catalogPublishingWidgetController', ['$scope', '$localStorage', 'platformWebApp.authService', 'platformWebApp.widgetService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.catalogPublishingModule.widgetMapperService', function ($scope, $localStorage, authService, widgetService, bladeNavigationService, catalogPublishingApi, widgetMapperService) {
        var blade = $scope.blade;
        var channel = $localStorage.catalogPublishingChannel;

        $scope.$watch("blade.currentEntity", function (currentEntity, oldCurrentEntity, scope) {
            if (currentEntity) {
                var catalogIds = [];
                _.each(currentEntity.outlines, function(outline) {
                    var catalogItem = _.find(outline.items, function(item) { return item.seoObjectType === 'Catalog' });
                    if (catalogItem) {
                        catalogIds.push(catalogItem.id);
                    }
                });

                catalogPublishingApi.searchChannels({
                    skip: 0,
                    take: Math.pow(2, 31) - 1,
                    catalogIds: catalogIds
                }, function(response) {
                    var allChannels = response.results;
                    if (allChannels && allChannels.length) {
                        var existingChannel = null;
                        if (channel) {
                            existingChannel = _.find(allChannels, function(c) { return c.id === channel.id });
                        }
                        $scope.channels = angular.copy(allChannels);
                        $scope.channel = existingChannel || allChannels[0];
                        evaluate($scope.channel.id, currentEntity.id);
                    }
                });
            }
        });

        $scope.evaluate = function () {
            evaluate($scope.channel.id, blade.currentEntityId, true);
        }

        function evaluate(channelId, productId, saveEntities) {
            catalogPublishingApi.evaluateChannelProducts({ id: channelId }, [productId],
                function (response) {
                    if (response.length) {
                        var entry = response[0];
                        $scope.completenessPercent = entry.completenessPercent;
                        _.each(entry.details, function (detail) {
                            var widgetControllerName = widgetMapperService.get(detail.name);
                            if (widgetControllerName) {
                                var widget = _.find($scope.widgets, function (w) { return w.controller === widgetControllerName });
                                if (widget && detail.completenessPercent < 100) {
                                    widget.UIclass = 'error';
                                } else {
                                    widget.UIclass = null;
                                }
                            }
                        });
                        if (saveEntities && authService.checkPermission('channel:update')) {
                            catalogPublishingApi.saveEntries([entry]);
                        }
                    } else {
                        $scope.completenessPercent = 0;
                    }
                });
        }

        $scope.changeChannel = function() {
            var channel = _.find($scope.channels, function (c) { return c.id === $scope.channel.id });
            $localStorage.catalogPublishingChannel = channel;
            evaluate(channel.id, blade.currentEntityId, true);
        };
    }]);