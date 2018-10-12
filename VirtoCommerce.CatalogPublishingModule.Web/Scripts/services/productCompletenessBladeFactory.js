angular.module('virtoCommerce.catalogPublishingModule')
    .factory('virtoCommerce.catalogPublishingModule.productCompletenessBladeFactory', function () {
        //Nothing to do here. Default factory implementation return null to use default product detail blade as primary view for product completeness
        //To override that factory in custom module just need to redefine it with same name and module
        // return function (product) {
        //    return {
        //       id: "{{blade identifier}}",
        //       product: product,
        //       controller: '{{controller fqn }}',
        //       template: '{{path to template}}'
        //    };
        //};
        return function (product) {
            return null;
        };
    });