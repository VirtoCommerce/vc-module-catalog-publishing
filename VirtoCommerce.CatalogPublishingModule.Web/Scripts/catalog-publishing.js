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
    .run(['$rootScope', '$state', 'platformWebApp.mainMenuService', function ($rootScope, $state, mainMenuService) {
        mainMenuService.addMenuItem({
            path: 'browse/publishing-channels',
            icon: 'fa fa-tasks',
            title: 'catalog-publishing.main-menu-title',
            prioritty: 0,
            action: function () {
                $state.go('workspace.catalogPublishingModule');
            }
        });
    }]);