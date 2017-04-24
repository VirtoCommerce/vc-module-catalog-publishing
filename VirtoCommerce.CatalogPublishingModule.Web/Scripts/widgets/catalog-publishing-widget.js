﻿angular.module('virtoCommerce.sitemapsModule')
    .controller('virtoCommerce.catalogPublishingModule.catalogPublishingWidgetController', ['$scope', '$localStorage', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.catalogPublishingModule.widgetMapperService', function ($scope, $localStorage, bladeNavigationService, catalogPublishingApi, widgetMapperService) {
        var blade = $scope.blade;
        var channel = $localStorage.catalogPublishingChannel;

        $scope.channel = channel;

        $scope.openChannelSelectBlade = function () {
            bladeNavigationService.showBlade({
                id: 'channelSelectBlade',
                title: 'catalog-publishing.blades.channel-select.title',
                headIcon: 'fa fa-tasks',
                channel: channel,
                productId: blade.currentEntityId,
                controller: 'virtoCommerce.catalogPublishingModule.channelSelectController',
                template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-select.tpl.html'
            }, blade);
        }

        catalogPublishingApi.evaluateChannelProducts({ id: channel.id }, [blade.currentEntityId],
            function (response) {
                if (response.length) {
                    var entry = response[0];
                    $scope.readinessPercent = entry.readinessPercent;
                    _.each(entry.details, function (detail) {
                        var widgetControllerName = widgetMapperService.get(detail.name);
                        if (widgetControllerName) {
                            var widgetElements = angular.element.find('[ng-controller="' + widgetControllerName + '"]');
                            if (widgetElements && widgetElements.length) {
                                if (detail.readinessPercent < 100) {
                                    widgetElements[0].parentElement.classList.add('error');
                                } else {
                                    widgetElements[0].parentElement.classList.remove('error');
                                }
                            }
                        }
                    });
                } else {
                    $scope.readinessPercent = 0;
                }
            });
    }]);