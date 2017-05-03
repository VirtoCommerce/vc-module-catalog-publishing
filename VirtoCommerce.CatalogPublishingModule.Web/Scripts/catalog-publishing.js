var moduleName = 'virtoCommerce.catalogPublishingModule';

if (AppDependencies != undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider', function ($stateProvider) {
        $stateProvider.state('workspace.catalogPublishingModule', {
            url: '/catalogPublishing',
            controller: ['$scope', 'platformWebApp.bladeNavigationService', function ($scope, bladeNavigationService) {
                var blade = {
                    id: 'catalogPublishing',
                    title: 'catalog-publishing.blades.channel-list.title',
                    headIcon: 'fa fa-tasks',
                    controller: 'virtoCommerce.catalogPublishingModule.channelListController',
                    template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-list.tpl.html',
                    isClosingDisabled: true
                }
                bladeNavigationService.showBlade(blade);
                $scope.moduleName = 'vc-catalog-publishing';
            }],
            templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html'
        });
    }])
    .factory('virtoCommerce.catalogPublishingModule.widgetMapperService', function() {
        var mapping = {};

        function map(detailName, widgetController) {
            mapping[detailName] = widgetController;
        }

        function get(detailName) {
            return mapping[detailName];
        }

        var retVal = {
            map: map,
            get: get
        };
        return retVal;
    })
    .run(['$rootScope', '$state', 'platformWebApp.mainMenuService', 'platformWebApp.pushNotificationTemplateResolver', 'platformWebApp.widgetService', 'virtoCommerce.catalogPublishingModule.widgetMapperService', 'platformWebApp.bladeNavigationService', function ($rootScope, $state, mainMenuService, pushNotificationTemplateResolver, widgetService, widgetMapperService, bladeNavigationService) {
        mainMenuService.addMenuItem({
            path: 'browse/channels',
            icon: 'fa fa-tasks',
            title: 'catalog-publishing.main-menu-title',
            prioritty: 0,
            permission: 'channel:access',
            action: function () {
                $state.go('workspace.catalogPublishingModule');
            }
        });
        widgetService.registerWidget({
            controller: 'virtoCommerce.catalogPublishingModule.catalogPublishingWidgetController',
            template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/widgets/catalog-publishing-widget.tpl.html',
            permission: 'channel:access'
        }, 'itemDetail');
        
        widgetMapperService.map("Properties", "virtoCommerce.catalogModule.itemPropertyWidgetController");
        widgetMapperService.map("Descriptions", "virtoCommerce.catalogModule.editorialReviewWidgetController");
        widgetMapperService.map("Prices", "virtoCommerce.pricingModule.itemPricesWidgetController");
        widgetMapperService.map("Seo", "virtoCommerce.coreModule.seo.seoWidgetController");

        var menuExportTemplate = {
            priority: 900,
            satisfy: function (notify, place) { return place == 'menu' && (notify.notifyType == 'EvaluateReadiness'); },
            template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/notifications/menuEvaluation.tpl.html',
            action: function (notify) { $state.go('workspace.pushNotificationsHistory', notify) }
        };
        pushNotificationTemplateResolver.register(menuExportTemplate);

        var historyExportTemplate = {
            priority: 900,
            satisfy: function (notify, place) {
                return place == 'history' && (notify.notifyType == 'EvaluateReadiness');
            },
            template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/notifications/historyEvaluation.tpl.html',
            action: function (notify) {
                var blade = {
                    id: 'evaluateProgress',
                    title: 'catalog-publishing.blades.channel-evaluate-details.title',
                    notification: notify,
                    controller: 'virtoCommerce.catalogPublishingModule.channelEvaluateDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-evaluate-details.tpl.html'
                };
                bladeNavigationService.showBlade(blade);
            }
        };
        pushNotificationTemplateResolver.register(historyExportTemplate);
    }]);