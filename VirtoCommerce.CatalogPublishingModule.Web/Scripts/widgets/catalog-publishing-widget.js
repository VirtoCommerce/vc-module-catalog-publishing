angular.module('virtoCommerce.sitemapsModule')
    .controller('virtoCommerce.catalogPublishingModule.catalogPublishingWidgetController', ['$scope', '$localStorage', 'platformWebApp.widgetService', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.catalogPublishingModule.widgetMapperService', function ($scope, $localStorage, widgetService, bladeNavigationService, catalogPublishingApi, widgetMapperService) {
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
                            var widget = _.find(widgetService.widgetsMap['itemDetail'], function (w) { return w.controller === widgetControllerName });
                            if (widget && detail.readinessPercent < 100) {
                                widget.UIclass = 'error';
                            }
                        }
                    });
                } else {
                    $scope.readinessPercent = 0;
                }
            });
    }]);