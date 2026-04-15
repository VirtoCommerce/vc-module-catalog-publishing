angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelListController', ['$scope', '$localStorage', 'platformWebApp.uiGridHelper', 'platformWebApp.bladeNavigationService', 'platformWebApp.dialogService', 'platformWebApp.bladeUtils', 'platformWebApp.ui-grid.extension', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.catalogPublishingModule.productCompletenessBladeFactory', function ($scope, $localStorage, uiGridHelper, bladeNavigationService, dialogService, bladeUtils, gridOptionExtension, catalogPublishingApi, productCompletenessBladeFactory) {
        const blade = $scope.blade;
        blade.isLoading = false;

        blade.refresh = function () {
            blade.isLoading = true;
            catalogPublishingApi.searchChannels({
                sort: uiGridHelper.getSortExpression($scope),
                skip: ($scope.pageSettings.currentPage - 1) * $scope.pageSettings.itemsPerPageCount,
                take: $scope.pageSettings.itemsPerPageCount
            }, function (response) {
                if (response.results?.length > 0) {
                    _.each(response.results, function (entry) {
                        entry.completenessPercent = getFormattedNumber(entry.completenessPercent);
                    });
                }

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
                const newBlade = {
                    id: 'evaluateProgress',
                    title: 'catalog-publishing.blades.channel-evaluate-details.title',
                    notification: response,
                    controller: 'virtoCommerce.catalogPublishingModule.channelEvaluateDetailsController',
                    template: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/channel-evaluate-details.tpl.html'
                };
                bladeNavigationService.showBlade(newBlade, blade);
            });
        }

        $scope.deleteChannels = function (selectedItems) {
            const dialog = {
                id: 'deleteCatalogPublishingChannelsDialog',
                title: 'catalog-publishing.dialogs.channel-delete.title',
                items: [{ key: 'catalog-publishing.dialogs.channel-delete.channel', count: selectedItems.length }],
                callback: function (remove) {
                    if (remove) {
                        bladeNavigationService.closeChildrenBlades(blade, function () {
                            const ids = _.pluck(selectedItems, 'id');
                            catalogPublishingApi.deleteChannels({ids: ids}, blade.refresh);
                        });
                    }
                }
            };
            dialogService.showDeleteConfirmationDialog(dialog);
        }

        $scope.selectCatalogItem = function (channel) {
            $scope.selectedNodeId = channel.id;
            $localStorage.catalogPublishingChannel = channel;
            bladeNavigationService.showBlade({
                id: 'completenessCatalogItems',
                breadcrumbs: [],
                filter: {
                    keyword: 'completeness_' + channel.name.toLowerCase() + ':[0 TO 99]',
                    searchedKeyword: 'completeness_' + channel.name.toLowerCase() + ':[0 TO 99]'
                },
                catalogId: channel.catalogId,
                controller: 'virtoCommerce.catalogModule.catalogItemSelectController',
                template: 'Modules/$(VirtoCommerce.Catalog)/Scripts/blades/common/catalog-items-select.tpl.html',
                options: {
                    gridColumns: [{
                        name: 'completenessPercent',
                        displayName: 'catalog-publishing.blades.channel-list.labels.completeness-percent',
                        cellTemplate: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/blades/completeness.cell.html'
                    }],
                    fnGetBladeForItem: function (product) {
                        //Extension point used for define custom views for product completeness
                        return productCompletenessBladeFactory(product);
                    },
                    onItemsLoaded: function (items) {
                        const itemIds = _.map(_.where(items, {type: 'product'}), function (i) {
                            return i.id
                        });
                        if (itemIds && itemIds.length) {
                            catalogPublishingApi.evaluateChannelProducts({id: channel.id}, itemIds,
                                function (response) {
                                    _.each(response, function (entry) {
                                        const item = _.find(items, function (i) {
                                            return i.id === entry.productId
                                        });
                                        if (item) {
                                            item.completenessPercent = getFormattedNumber(entry.completenessPercent);
                                        } else {
                                            item.completenessPercent = 0;
                                        }
                                    });
                                });
                        }
                    }
                }
            }, blade);
        }

        function getFormattedNumber(entry) {
            const percentNumber = Number(entry);
            if (percentNumber % 1 === 0) {
                return percentNumber.toFixed(0);
            } else {
                return percentNumber.toFixed(1);
            }
        }

        blade.toolbarCommands = [{
            name: 'platform.commands.add',
            icon: 'fas fa-plus',
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
            icon: 'fas fa-trash-alt',
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
    }]);
