angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelDetailsController', ['$scope', 'platformWebApp.bladeNavigationService', 'virtoCommerce.catalogPublishingModule.catalogPublishing', 'virtoCommerce.pricingModule.pricelists', 'virtoCommerce.catalogModule.catalogs', function ($scope, bladeNavigationService, catalogPublishingApi, pricingApi, catalogApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        blade.refresh = function () {
            catalogPublishingApi.getChannel({
                id: blade.currentEntity.id
            }, function (response) {
                blade.currentEntity = response;
            });
        }

        blade.toolbarCommands = [{
            name: 'platform.commands.save',
            icon: 'fa fa-save',
            canExecuteMethod: function () {
                return canSave();
            },
            executeMethod: function () {
                saveChanges();
            }
        }, {
            name: 'catalog-publishing.blades.channel-details.labels.evaluate',
            icon: 'fa fa-calculator',
            canExecuteMethod: function () {
                return blade.currentEntity && blade.currentEntity.id && $scope.formChannel && $scope.formChannel.$valid;
            },
            executeMethod: function () {
                evaluateChannel();
            }
        }];

        $scope.setForm = function (form) {
            $scope.formChannel = form;
        }

        pricingApi.search({
            skip: 0,
            take: 500
        }, function (response) {
            $scope.pricelists = response.results;
        });
        $scope.catalogs = catalogApi.getCatalogs();
        $scope.evaluators = [{ id: 'Default' }]; //catalogPublishingApi.getEvaluators();

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.originalEntity);
        }

        function canSave() {
            return isDirty() && $scope.formChannel && $scope.formChannel.$valid;
        }

        function saveChanges() {
            blade.isLoading = true;
            if (blade.currentEntity.id) {
                catalogPublishingApi.updateChannel(blade.currentEntity, function () {
                    blade.parentBlade.refresh();
                    blade.isLoading = false;
                });
            } else {
                catalogPublishingApi.addChannel(blade.currentEntity, function () {
                    blade.parentBlade.refresh();
                    blade.isLoading = false;
                });
            }
        }

        function evaluateChannel() {
            catalogPublishingApi.evaluateChannel({
                id: blade.currentEntity.id
            }, function (response) {
                blade.refresh(true);
            });
        }
    }]);