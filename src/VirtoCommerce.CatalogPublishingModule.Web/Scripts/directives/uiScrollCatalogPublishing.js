//TODO: Use generic directive from the platform 

// generic simple dropdown with infinite scroll.
// tracking by entity.name field 

angular.module('virtoCommerce.catalogPublishingModule')
    .directive('uiScrollCatalogPublishing', [function () {
        const defaultPageSize = 20;
        return {
            restrict: 'E',
            replace: true,
            scope: {
                data: '&',
                label: '=',
                placeholder: '=',
                selectedId: '=',
                selectedEntity: '='
            },
            templateUrl: 'Modules/$(VirtoCommerce.CatalogPublishing)/Scripts/directives/uiScrollCatalogPublishing.tpl.html',
            controller: ['$scope', function(scope) {
                scope.list = [];
                
                scope.pageSize = defaultPageSize;

                scope.setValue = (item) => {
                    scope.selectedId = item.id;
                    scope.selectedEntity = item;
                }

                scope.fetch = (select) => {
                    if (scope.list.length === 0) {
                        select.page = 0;
                        if (scope.selectedId) {
                            let criteria = {
                                objectIds: [scope.selectedId]
                            }
                            scope.data({ criteria: criteria }).then((data) => {
                                scope.list = data.results;
                                scope.setValue(data.results[0])
                                scope.fetchNext(select);
                            });
                        }
                        else {
                            scope.fetchNext(select);
                        }
                    }
                };

                scope.fetchNext = (select) => {
                    let criteria = {
                        searchPhrase: select.search,
                        take: scope.pageSize,
                        skip: select.page * scope.pageSize
                    }
            
                    scope.data({criteria: criteria}).then((data) => {
                        scope.list = scope.list.concat(data.results);
                        select.page++;
            
                        if (scope.list.length < data.totalCount) {
                            scope.$broadcast('scrollCompleted');
                        }
                    });
                }
            }]
        }
    }]);
