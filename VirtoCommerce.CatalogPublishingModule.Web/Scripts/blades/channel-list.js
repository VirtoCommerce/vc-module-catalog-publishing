angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelListController', ['$scope', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, uiGridHelper, bladeNavigationService, dialogService, bladeUtils, catalogPublishingApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        blade.refresh = function () {
            blade.isLoading = true;
            catalogPublishingApi.searchChannels({
                sort: uiGridHelper.getSortExpression($scope),
                skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                take: $scope.pageSettings.itemsPerPageCount
            }, function (response) {
                blade.currentEntities = response.results;
                $scope.pageSettings.totalItems = response.totalCount;
                blade.isLoading = false;
            });
        }

        $scope.selectNode = function (entity) {
            bladeNavigationService.showBlade({
                id: 'catalogPublishingChannel',
                title: entity ? 'catalog-publishing.blades.channel-details.title' : 'catalog-publishing.blades.channel-details.new-title',
                headIcon: 'fa fa-tasks',
                currentEntity: entity,
                originalEntity: angular.copy(entity),
                controller: 'virtoCommerce.catalogPublishingModule.channelDetailsController',
                template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-details.tpl.html'
            }, blade);
        }

        $scope.deleteChannels = function (selectedItems) {
            var dialog = {
                id: 'deleteCatalogPublishingChannelsDialog',
                title: 'catalog-publishing.dialogs.channel-delete.title',
                message: 'catalog-publishing.dialogs.channel-delete.message',
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            var ids = _.pluck(selectedItems, 'id');
                            catalogPublishingApi.deleteChannels({ ids: ids }, blade.refresh);
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