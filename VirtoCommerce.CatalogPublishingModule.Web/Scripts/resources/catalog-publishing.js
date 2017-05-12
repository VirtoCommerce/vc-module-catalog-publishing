angular.module('virtoCommerce.catalogPublishingModule')
    .factory('virtoCommerce.catalogPublishingModule.catalogPublishing', ['$resource', function ($resource) {
        return $resource('api/completeness', {}, {
            searchChannels: { url: 'api/completeness/channels/search', method: 'POST' },
            getChannel: { url: 'api/completeness/channels/:id', method: 'GET' },
            addChannel: { url: 'api/completeness/channels', method: 'POST' },
            updateChannel: { url: 'api/completeness/channels', method: 'PUT' },
            deleteChannels: { url: 'api/completeness/channels', method: 'DELETE' },
            getEvaluators: { url: 'api/completeness/evaluators', method: 'GET', isArray: true },
            evaluateChannel: { url: 'api/completeness/channels/:id/evaluate', method: 'POST', params: { id: '@id' } },
            evaluateChannelProducts: { url: 'api/completeness/channels/:id/products/evaluate', method: 'POST', params: { id: '@id' }, isArray: true },
            saveEntries: { url: 'api/completeness/entries', method: 'PUT' }
        });
    }]);