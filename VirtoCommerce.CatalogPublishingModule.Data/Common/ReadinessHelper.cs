using System;

namespace VirtoCommerce.CatalogPublishingModule.Data.Common
{
    public static class ReadinessHelper
    {
        public static int CalculateReadiness(int totalCount, int invalidCount)
        {
            int retVal;
            if (totalCount == 0 || invalidCount == 0)
            {
                retVal = 100;
            }
            else if (totalCount == invalidCount)
            {
                retVal = 0;
            }
            else
            {
                retVal = (int)Math.Round((totalCount - invalidCount) / (double)totalCount * 100);
            }
            return retVal;
        }
    }
}