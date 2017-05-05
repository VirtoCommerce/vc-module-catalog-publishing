angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelListController', ['$scope', '$localStorage', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.ui-grid.extension', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, $localStorage, uiGridHelper, bladeNavigationService, dialogService, bladeUtils, gridOptionExtension, catalogPublishingApi) {
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
            $scope.selectedNodeId = entity ? entity.id : null;
            bladeNavigationService.showBlade({
                id: 'catalogPublishingChannel',
                title: entity ? 'catalog-publishing.blades.channel-details.title' : 'catalog-publishing.blades.channel-details.new-title',
                headIcon: 'fa fa-tasks',
                currentEntity: angular.copy(entity),
                originalEntity: angular.copy(entity),
                parentSelectNode: $scope.selectNode,
                controller: 'virtoCommerce.catalogPublishingModule.channelDetailsController',
                template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-details.tpl.html'
            }, blade);
        }

        $scope.evaluateChannel = function (channel) {
            catalogPublishingApi.evaluateChannel({
                id: channel.id
            }, function (response) {
                var newBlade = {
                    id: 'evaluateProgress',
                    title: 'catalog-publishing.blades.channel-evaluate-details.title',
                    notification: response,
                    controller: 'virtoCommerce.catalogPublishingModule.channelEvaluateDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-evaluate-details.tpl.html'
                }
                bladeNavigationService.showBlade(newBlade, blade);
            });
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

        $scope.selectCatalogItem = function(channel) {
            $scope.selectedNodeId = channel.id;
            $localStorage.catalogPublishingChannel = channel;
            bladeNavigationService.showBlade({
                id: 'readinessCatalogItems',
                breadcrumbs: [],
                filter: {
                    keyword: 'readiness_' + channel.name.toLowerCase() + ':[0 TO 99]',
                    searchedKeyword: 'readiness_' + channel.name.toLowerCase() + ':[0 TO 99]'
                },
                catalogId: channel.catalogId,
                controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                options: {
                    onItemsLoaded: function(items) {
                        var itemIds = _.map(_.where(items, { type: 'product' }), function(i) { return i.id });
                        if (itemIds && itemIds.length) {
                            catalogPublishingApi.evaluateChannelProducts({ id: channel.id }, itemIds,
                                function(response) {
                                    _.each(response, function(entry) {
                                        var item = _.find(items, function(i) { return i.id === entry.productId });
                                        if (item) {
                                            item.readinessPercent = entry.readinessPercent;
                                        } else {
                                            item.readinessPercent = 0;
                                        }
                                    });
                                });
                        }
                    }
                }
            }, blade);
        }

        blade.toolbarCommands = [{
            name: 'platform.commands.add',
            icon: 'fa fa-plus',
            permission: 'channel:create',
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
            permission: 'channel:delete',
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
    }])
    .run(['platformWebApp.ui-grid.extension', function (gridOptionExtension) {
        gridOptionExtension.registerExtension('catalog-item-select-grid', function (gridOptions) {
            gridOptions.columnDefs.push({
                name: 'readinessPercent',
                displayName: 'catalog-publishing.blades.channel-list.labels.completeness-percent',
                cellTemplate: 'completeness.cell.html'
            });
        });
    }]);