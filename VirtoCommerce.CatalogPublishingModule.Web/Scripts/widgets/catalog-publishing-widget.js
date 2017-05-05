﻿angular.module('virtoCommerce.sitemapsModule')
    .controller('virtoCommerce.catalogPublishingModule.catalogPublishingWidgetController', ['$scope', '$localStorage', 'platformWebApp.authService', 'platformWebApp.widgetService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.catalogPublishingModule.widgetMapperService', function ($scope, $localStorage, authService, widgetService, bladeNavigationService, catalogPublishingApi, widgetMapperService) {
        var blade = $scope.blade;
        var channel = $localStorage.catalogPublishingChannel;

        $scope.$on('product-loaded', function (event, product) {
            var catalogIds = [];
            _.each(blade.currentEntity.outlines, function (outline) {
                var catalogItem = _.find(outline.items, function (item) { return item.seoObjectType === 'Catalog' });
                if (catalogItem) {
                    catalogIds.push(catalogItem.id);
                }
            });

            catalogPublishingApi.searchChannels({
                skip: 0,
                take: 1000,
                catalogIds: catalogIds
            }, function (response) {
                var allChannels = response.results;
                if (allChannels && allChannels.length) {
                    var existingChannel = null;
                    if (channel) {
                        existingChannel = _.find(allChannels, function (c) { return c.id === channel.id });
                    }
                    $scope.channel = existingChannel || allChannels[0];
                    $scope.openChannelSelectBlade = function () {
                        bladeNavigationService.showBlade({
                            id: 'channelSelectBlade',
                            title: 'catalog-publishing.blades.channel-select.title',
                            headIcon: 'fa fa-tasks',
                            channel: $scope.channel,
                            productId: blade.currentEntityId,
                            controller: 'virtoCommerce.catalogPublishingModule.channelSelectController',
                            template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-select.tpl.html'
                        }, blade);
                    }
                    evaluate($scope.channel.id, blade.currentEntityId);
                }
            });
        });

        $scope.$on('product-saved', function (event, product) {
            evaluate(channel.id, product.id, true);
        });

        function evaluate(channelId, productId, saveEntities) {
            catalogPublishingApi.evaluateChannelProducts({ id: channelId }, [productId],
                function (response) {
                    if (response.length) {
                        var entry = response[0];
                        $scope.readinessPercent = entry.readinessPercent;
                        _.each(entry.details, function (detail) {
                            var widgetControllerName = widgetMapperService.get(detail.name);
                            if (widgetControllerName) {
                                var widget = _.find($scope.widgets, function (w) { return w.controller === widgetControllerName });
                                if (widget && detail.readinessPercent < 100) {
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
                        $scope.readinessPercent = 0;
                    }
                });

        }
    }]);