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
    .run(['$rootScope', '$state', 'platformWebApp.mainMenuService', 'virtoCommerce.catalogPublishingModule.widgetMapperService', function ($rootScope, $state, mainMenuService, widgetMapperService) {
        mainMenuService.addMenuItem({
            path: 'browse/publishing-channels',
            icon: 'fa fa-tasks',
            title: 'catalog-publishing.main-menu-title',
            prioritty: 0,
            action: function () {
                $state.go('workspace.catalogPublishingModule');
            }
        });

        widgetMapperService.map("Properties", "virtoCommerce.catalogModule.itemPropertyWidgetController");
        widgetMapperService.map("Descriptions", "virtoCommerce.catalogModule.editorialReviewWidgetController");
        widgetMapperService.map("Prices", "virtoCommerce.pricingModule.itemPricesWidgetController");
        widgetMapperService.map("Seo", "virtoCommerce.coreModule.seo.seoWidgetController");
    }]);