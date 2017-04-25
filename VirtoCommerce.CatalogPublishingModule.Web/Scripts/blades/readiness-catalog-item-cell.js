angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.readinessCatalogItemCellController', ['$scope', '$localStorage', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, $localStorage, catalogPublishingApi) {
        if ($scope.row.entity.type === 'product') {
            catalogPublishingApi.evaluateChannelProducts({ id: $localStorage.catalogPublishingChannel.id }, [$scope.row.entity.id],
                function (response) {
                    $scope.readinessPercent = response ? response[0].readinessPercent : 0;
                });
        }
    }]);