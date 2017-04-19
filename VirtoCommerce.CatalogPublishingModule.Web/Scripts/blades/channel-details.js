angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelDetailsController', ['$scope', 'virtoCommerce.catalogModule.catalogs', function ($scope, catalogApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        blade.toolbarCommands = [{
            name: 'platform.commands.save',
            icon: 'fa fa-save',
            canExecuteMethod: function () {
                return canSave();
            },
            executeMethod: function () {

            }
        }, {
            name: 'catalog-publishing.blades.channel-details.labels.evaluate',
            icon: ''
        }];

        $scope.setForm = function (form) {
            $scope.formChannel = form;
        }

        $scope.catalogs = catalogApi.getCatalogs();

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.originalEntity);
        }

        function canSave() {
            return isDirty() && $scope.formChannel && $scope.formChannel.$valid;
        }
    }]);