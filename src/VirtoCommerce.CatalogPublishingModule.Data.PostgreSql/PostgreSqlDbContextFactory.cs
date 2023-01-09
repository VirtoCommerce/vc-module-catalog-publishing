using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;

namespace VirtoCommerce.CatalogPublishingModule.Data.PostgreSql
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<CatalogPublishingDbContext>
    {
        public CatalogPublishingDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CatalogPublishingDbContext>();
            var connectionString = args.Any() ? args[0] : "User ID = postgres; Password = password; Host = localhost; Port = 5432; Database = virtocommerce3;";

            builder.UseNpgsql(
                connectionString,
                db => db.MigrationsAssembly(typeof(PostgreSqlDbContextFactory).Assembly.GetName().Name));

            return new CatalogPublishingDbContext(builder.Options);
        }
    }
}
