using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogPublishingModule.Core;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.MySql;
using VirtoCommerce.CatalogPublishingModule.Data.PostgreSql;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation;
using VirtoCommerce.CatalogPublishingModule.Data.SqlServer;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPublishingModule.Web
{
    public class Module : IModule, IHasConfiguration
    {
        public ManifestModuleInfo ModuleInfo { get; set; }
        public IConfiguration Configuration { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<CatalogPublishingDbContext>((provider, options) =>
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var connectionString = Configuration.GetConnectionString(ModuleInfo.Id) ?? Configuration.GetConnectionString("VirtoCommerce");

                switch (databaseProvider)
                {
                    case "MySql":
                        options.UseMySqlDatabase(connectionString);
                        break;
                    case "PostgreSql":
                        options.UsePostgreSqlDatabase(connectionString);
                        break;
                    default:
                        options.UseSqlServerDatabase(connectionString);
                        break;
                }
            });

            serviceCollection.AddTransient<ICompletenessRepository, CompletenessRepositoryImpl>();
            serviceCollection.AddSingleton<Func<ICompletenessRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ICompletenessRepository>());

            serviceCollection.AddTransient<ICompletenessService, CompletenessServiceImpl>();
            serviceCollection.AddTransient<ICompletenessEvaluator, DefaultCompletenessEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, PropertiesCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, DescriptionsCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, PricesCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, SeoCompletenessDetailEvaluator>();

            serviceCollection.AddTransient<ProductCompletenessChangesProvider>();
            serviceCollection.AddTransient<ProductCompletenessDocumentBuilder>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var permissionsRegistrar = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsRegistrar.RegisterPermissions(ModuleInfo.Id, "CatalogPublishing", ModuleConstants.Security.Permissions.AllPermissions);

            var productIndexingConfigurations = appBuilder.ApplicationServices.GetServices<IndexDocumentConfiguration>();

            if (productIndexingConfigurations != null)
            {
                var productCompletenessDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetRequiredService<ProductCompletenessChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetRequiredService<ProductCompletenessDocumentBuilder>(),
                };

                foreach (var configuration in productIndexingConfigurations.Where(c => c.DocumentType == KnownDocumentTypes.Product))
                {
                    configuration.RelatedSources ??= new List<IndexDocumentSource>();
                    configuration.RelatedSources.Add(productCompletenessDocumentSource);
                }
            }

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var databaseProvider = Configuration.GetValue("DatabaseProvider", "SqlServer");
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogPublishingDbContext>();
                if (databaseProvider == "SqlServer")
                {
                    dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                }
                dbContext.Database.Migrate();
            }
        }

        public void Uninstall()
        {
            // Not needed
        }
    }
}
