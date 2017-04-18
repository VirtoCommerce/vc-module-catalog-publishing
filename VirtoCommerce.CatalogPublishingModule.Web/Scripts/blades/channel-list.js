angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelListController', ['$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', function ($scope, uiGridHelper, bladeNavigationService, dialogService, bladeUtils) {
        var blade = $scope.blade;
        blade.isLoading = false;

        blade.refresh = function () {
            blade.isLoading = true;
            blade.currentEnties = data;
            $scope.pageSettings.totalItems = data.totalCount;
            blade.isLoading = false;
        }

        $scope.selectNode = function (entity) {

        }

        $scope.deleteChannels = function (selectedItems) {
            var dialog = {
                id: 'deleteCatalogPublishingChannelsDialog',
                title: 'catalog-publishing.dialogs.channel-delete.title',
                title: 'catalog-publishing.dialogs.channel-delete.message',
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                        });
                    }
                }
            }
            dialogService.showConfirmationDialog(dialog);
        }

        blade.toolbarCommands = [{
            name: 'platform.commands.add',
            icon: 'fa fa-plus',
            canExecuteMethod: function () {
                return true;
            },
            executeMethod: function () {
                $scope.selectNode();
            }
        }, {
            name: 'platform.commands.refresh',
            icon: 'fa fa-refresh',
            canExecuteMethod: function () {
                return true;
            },
            executeMethod: function () {
                blade.refresh();
            }
        }, {
            name: 'platform.commands.delete',
            icon: 'fa fa-trash-o',
            canExecuteMethod: function () {
                return $scope.gridApi && _.any($scope.gridApi.selection.getSelectedRows());
            },
            executeMethod: function () {
                $scope.deleteChannels($scope.gridApi.selection.getSelectedRows());
            }
        }];

        $scope.setGridOptions = function (gridOptions) {
            $scope.gridOptions = gridOptions;
            gridOptions.onRegisterApi = function (gridApi) {
                gridApi.core.on.sortChanged($scope, function () {
                    if (!blade.isLoading) blade.refresh();
                });
            };
            bladeUtils.initializePagination($scope);
        };
    }]);

var data = {
    results: [{
        id: '1',
        name: 'test-1',
        language: 'en-US',
        pricelistId: '934da94516a74f9ab4ec001343ac928a',
        catalogId: '4974648a41df4e6ea67ef2ad76d7bbd4',
        evaluatorType: '',
        readinessPercent: 95
    }],
    totalCount: 1
};