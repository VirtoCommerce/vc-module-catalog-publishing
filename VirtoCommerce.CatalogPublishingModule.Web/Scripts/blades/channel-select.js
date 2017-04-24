angular.module('virtoCommerce.catalogPublishingModule')
    .controller('virtoCommerce.catalogPublishingModule.channelSelectController', ['$scope', '$localStorage', 'virtoCommerce.catalogPublishingModule.catalogPublishing', function ($scope, $localStorage, catalogPublishingApi) {
        var blade = $scope.blade;
        blade.isLoading = false;

        $scope.channel = $localStorage.catalogPublishingChannel;

        blade.toolbarCommands = [{
            name: 'platform.commands.save',
            icon: 'fa fa-save',
            canExecuteMethod: function () {
                return true;
            },
            executeMethod: function () {
                $localStorage.catalogPublishingChannel = $scope.channel;
                catalogPublishingApi.evaluateChannelProducts({ id: $scope.channel.id }, [blade.productId],
                    function (response) {
                        $scope.readinessPercent = response.length? response[0].readinessPercent : 0;
                    });
            }
        }];

        catalogPublishingApi.searchChannels({
            skip: 0,
            take: 1000
        }, function (response) {
            $scope.channels = response.results;
        });
    }]);