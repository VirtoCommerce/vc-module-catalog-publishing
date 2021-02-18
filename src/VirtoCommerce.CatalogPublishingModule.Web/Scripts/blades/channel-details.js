angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelDetailsController', ['$scope', 'platformWebApp.settings', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.coreModule.currency.currencyApi', 'virtoCommerce.coreModule.currency.currencyUtils', 'virtoCommerce.catalogModule.catalogs', function ($scope, settings, bladeNavigationService, catalogPublishingApi, currencyApi, currencyUtils, catalogApi) {
        var blade = $scope.blade;
        blade.isLoading = false;
        blade.isNew = !blade.currentEntity || !blade.currentEntity.id;
        blade.updatePermission = 'channel:update';
        blade.selectedCatalog = {};

        blade.refresh = function () {
            catalogPublishingApi.getChannel({
                id: blade.currentEntity.id
            }, function (response) {
                initializeBlade(response);
            });
        }

        if (!blade.isNew) {
            blade.toolbarCommands = [{
                name: 'platform.commands.save',
                icon: 'fas fa-save',
                permission: blade.updatePermission,
                canExecuteMethod: function () {
                    return canSave();
                },
                executeMethod: function () {
                    saveChanges();
                }
            }, {
                name: 'catalog-publishing.blades.channel-details.labels.evaluate',
                icon: 'fa fa-calculator',
                permission: 'channel:update',
                canExecuteMethod: function () {
                    return blade.currentEntity && blade.currentEntity.id && $scope.formScope && $scope.formScope.$valid;
                },
                executeMethod: function () {
                    catalogPublishingApi.evaluateChannel({
                        id: blade.currentEntity.id
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
            }];
        }

        $scope.setForm = function (form) {
            $scope.formScope = form;
        }

        $scope.currencyUtils = currencyUtils;
        currencyApi.query(function (response) {
            $scope.currencies = response;
        });
        $scope.languages = [];
        settings.getValues({
            id: 'VirtoCommerce.Core.General.Languages'
        }, function (response) {
            $scope.languages = response;
        });
        $scope.catalogDataSource = (criteria) => catalogApi.search(criteria).$promise;
        $scope.evaluators = catalogPublishingApi.getEvaluators();

        $scope.openLanguagesDictionarySettingManagement = function () {
            var newBlade = {
                id: 'settingDetailChild',
                isApiSave: true,
                currentEntityId: 'VirtoCommerce.Core.General.Languages',
                parentRefresh: function (data) { $scope.languages = data; },
                controller: 'platformWebApp.settingDictionaryController',
                template: '$(Platform)/Scripts/app/settings/blades/setting-dictionary.tpl.html'
            };
            bladeNavigationService.showBlade(newBlade, blade);
        };

        $scope.saveChanges = function () {
            saveChanges();
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.originalEntity) && !blade.isNew && blade.hasUpdatePermission();
        }

        function canSave() {
            return isDirty() && $scope.formScope && $scope.formScope.$valid;
        }

        function initializeBlade(data) {
            blade.currentEntity = angular.copy(data);
            blade.originalEntity = data;
            blade.isLoading = false;
        }

        function saveChanges() {
            blade.currentEntity.catalogName = blade.selectedCatalog.name;
            blade.isLoading = true;
            if (!blade.isNew) {
                catalogPublishingApi.updateChannel(blade.currentEntity, function () {
                    blade.refresh();
                    blade.parentBlade.refresh();
                    blade.isLoading = false;
                });
            } else {
                catalogPublishingApi.addChannel(blade.currentEntity, function (response) {
                    angular.copy(blade.currentEntity, blade.originalentity);
                    blade.parentBlade.refresh();
                    blade.parentSelectNode(response);
                    blade.isLoading = false;
                });
            }
        }

        blade.onClose = function (closeCallback) {
            bladeNavigationService.showConfirmationIfNeeded(isDirty(), canSave(), blade, saveChanges, closeCallback, "catalog-publishing.dialogs.channel-save.title", "catalog-publishing.dialogs.channel-save.message");
        };
    }]);
