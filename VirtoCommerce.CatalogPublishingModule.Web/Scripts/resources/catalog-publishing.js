angular.module('virtoCommerce.catalogPublishingModule')
    .factory('virtoCommerce.catalogPublishingModule.catalogPublishing', ['$resource', function ($resource) {
        return $resource('api/readiness', {}, {
            searchChannels: { url: 'api/readiness/channels/search', method: 'POST' },
            getChannel: { url: 'api/readiness/channels/:id', method: 'GET' },
            addChannel: { url: 'api/readiness/channels', method: 'POST' },
            updateChannel: { url: 'api/readiness/channels', method: 'PUT' },
            deleteChannels: { url: 'api/readiness/channels', method: 'DELETE' },
            getEvaluators: { url: 'api/readiness/evaluators', method: 'GET', isArray: true },
            evaluateChannel: { url: 'api/readiness/channels/:id/evaluate', method: 'POST', params: { id: '@id' } },
            evaluateChannelProducts: { url: 'api/readiness/channels/:id/products/evaluate', method: 'POST', params: { id: '@id' }, isArray: true },
            saveEntries: { url: 'api/readiness/entries', method: 'PUT' }
        });
    }]);