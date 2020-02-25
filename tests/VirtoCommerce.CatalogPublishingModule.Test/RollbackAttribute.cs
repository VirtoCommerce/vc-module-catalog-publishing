using System;
using System.Reflection;
using System.Transactions;
using Xunit.Sdk;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RollbackAttribute : BeforeAfterTestAttribute
    {
        private TransactionScope _transactionScope;

        public override void Before(MethodInfo methodUnderTest)
        {
            _transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Unspecified }, TransactionScopeAsyncFlowOption.Enabled);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            _transactionScope.Dispose();
        }
    }
}
