using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Repositories;
using VirtoCommerce.CatalogPublishingModule.Data.Search.Indexing;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services.Evaluation;
using VirtoCommerce.Platform.Core.Modularity;
using VirtoCommerce.SearchModule.Core.Model;

namespace VirtoCommerce.CatalogPublishingModule.Web
{
    public class Module : IModule
    {
        public ManifestModuleInfo ModuleInfo { get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            var configuration = serviceCollection.BuildServiceProvider().GetRequiredService<IConfiguration>();
            serviceCollection.AddTransient<ICompletenessRepository, CompletenessRepositoryImpl>();
            var connectionString = configuration.GetConnectionString("VirtoCommerce");
            serviceCollection.AddDbContext<CatalogPublishingDbContext>(options => options.UseSqlServer(connectionString));
            serviceCollection.AddSingleton<Func<ICompletenessRepository>>(provider => () => provider.CreateScope().ServiceProvider.GetRequiredService<ICompletenessRepository>());

            serviceCollection.AddTransient<ICompletenessService, CompletenessServiceImpl>();
            serviceCollection.AddTransient<ICompletenessEvaluator, DefaultCompletenessEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, PropertiesCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, DescriptionsCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, PricesCompletenessDetailEvaluator>();
            serviceCollection.AddTransient<ICompletenessDetailEvaluator, SeoCompletenessDetailEvaluator>();
        }

        public void PostInitialize(IApplicationBuilder appBuilder)
        {
            var productIndexingConfigurations = appBuilder.ApplicationServices.GetServices<IndexDocumentConfiguration>();

            if (productIndexingConfigurations != null)
            {
                var productCompletenessDocumentSource = new IndexDocumentSource
                {
                    ChangesProvider = appBuilder.ApplicationServices.GetService<ProductCompletenessChangesProvider>(),
                    DocumentBuilder = appBuilder.ApplicationServices.GetService<ProductCompletenessDocumentBuilder>(),
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
        }

        public void Uninstall()
        {
            // Not needed
        }
    }
}
