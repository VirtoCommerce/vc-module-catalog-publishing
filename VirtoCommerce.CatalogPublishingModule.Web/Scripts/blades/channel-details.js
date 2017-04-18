angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelDetailsController', ['$scope', function ($scope) {
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
        }];

        $scope.setForm = function (form) {
            $scope.formChannel = form;
        }

        function isDirty() {
            return !angular.equals(blade.currentEntity, blade.originalEntity);
        }

        function canSave() {
            return isDirty() && $scope.formChannel && $scope.formChannel.$valid;
        }
    }]);