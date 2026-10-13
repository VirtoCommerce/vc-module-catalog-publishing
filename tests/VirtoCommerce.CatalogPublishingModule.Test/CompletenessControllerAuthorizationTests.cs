using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using VirtoCommerce.CatalogPublishingModule.Core;
using VirtoCommerce.CatalogPublishingModule.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    public class CompletenessControllerAuthorizationTests
    {
        [Fact]
        public void Controller_Requires_Authenticated_User()
        {
            Assert.NotNull(typeof(CompletenessController).GetCustomAttribute<AuthorizeAttribute>());
        }

        [Theory]
        [InlineData(nameof(CompletenessController.GetEvaluators), ModuleConstants.Security.Permissions.Read)]
        [InlineData(nameof(CompletenessController.EvaluateProductsCompleteness), ModuleConstants.Security.Permissions.Read)]
        public void Action_Requires_Permission(string actionName, string permission)
        {
            var policies = typeof(CompletenessController).GetMethod(actionName)
                .GetCustomAttributes<AuthorizeAttribute>()
                .Select(x => x.Policy);

            Assert.Contains(permission, policies);
        }

        [Fact]
        public void Every_Http_Action_Requires_A_Permission()
        {
            var actions = GetPublicMethods().Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any()).ToList();

            Assert.NotEmpty(actions);
            Assert.All(actions, action =>
            {
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
                Assert.Contains(action.GetCustomAttributes<AuthorizeAttribute>(), x => !string.IsNullOrEmpty(x.Policy));
            });
        }

        [Fact]
        public void Public_Methods_Without_Http_Attribute_Are_Not_Actions()
        {
            // A public controller method without an HTTP attribute is still routed on the controller route for any verb
            var methods = GetPublicMethods().Where(x => !x.GetCustomAttributes<HttpMethodAttribute>().Any());

            Assert.All(methods, method => Assert.NotNull(method.GetCustomAttribute<NonActionAttribute>()));
        }

        private static MethodInfo[] GetPublicMethods()
        {
            return typeof(CompletenessController).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}
