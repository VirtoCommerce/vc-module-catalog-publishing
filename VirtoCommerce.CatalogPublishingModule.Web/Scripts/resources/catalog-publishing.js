angular.module('virtoCommerce.catalogPublishingModule')
    .factory('virtoCommerce.catalogPublishingModule.catalogPublishing', ['$resource', function ($resource) {
        return $resource('api/readiness', {
            searchChannel: { url: 'api/readiness/search', method: 'POST' },
            getChannel: { url: 'api/readiness/:id' },
            addChannel: { url: 'api/readiness/channels', method: 'POST' },
            updateChannel: { url: 'api/readiness/channels', method: 'PUT' },
            deleteChannels: { url: 'api/readiness/channels', method: 'DELETE' },
            evaluateChannel: { url: 'api/readiness/channels/:id/evaluate', method: 'POST' },
            evaluateChannelProducts: { url: 'api/readiness/channels/:id/products/evaluate', method: 'POST' }
        });
    }]);