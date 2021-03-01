using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogPublishingModule.Core;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPublishingModule.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            serviceCollection.AddDbContext<CatalogPublishingDbContext>((provider, options) =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                options.UseSqlServer(configuration.GetConnectionString(ModuleInfo.Id) ?? configuration.GetConnectionString("VirtoCommerce"));
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
            var permissionsProvider = appBuilder.ApplicationServices.GetRequiredService<IPermissionsRegistrar>();
            permissionsProvider.RegisterPermissions(ModuleConstants.Security.Permissions.AllPermissions.Select(x => new Permission() { GroupName = "CatalogPublishing", Name = x }).ToArray());

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
                    if (configuration.RelatedSources == null)
                    {
                        configuration.RelatedSources = new List<IndexDocumentSource>();
                    }

                    configuration.RelatedSources.Add(productCompletenessDocumentSource);
                }
            }

            using (var serviceScope = appBuilder.ApplicationServices.CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetRequiredService<CatalogPublishingDbContext>();
                dbContext.Database.MigrateIfNotApplied(MigrationName.GetUpdateV2MigrationName(ModuleInfo.Id));
                dbContext.Database.EnsureCreated();
                dbContext.Database.Migrate();
            }
        }

        public void Uninstall()
        {
            // Not needed
        }
    }
}
