using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.CatalogPublishingModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CatalogPublishingDbContext>
    {
        public CatalogPublishingDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CatalogPublishingDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new CatalogPublishingDbContext(builder.Options);
        }
    }
}
