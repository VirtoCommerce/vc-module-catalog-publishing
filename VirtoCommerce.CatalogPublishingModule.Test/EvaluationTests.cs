using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Pricing.Model;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [Trait("Category", "CI")]
    public class EvaluationTests
    {
        private string catalogId;
        private CatalogProduct[] products;
        private string pricelistId;
        private Pricelist pricelist;
        private string[] editorialReviewTypes;

        [Fact]
        public void InvalidParameters()
        {
            var evaluator = GetReadinessEvaluator();

            // catalog
            Assert.Throws<ArgumentNullException>(() => evaluator.EvaluateReadiness(null, new[] { new CatalogProduct() }));
            // products
            Assert.Throws<ArgumentException>(() => evaluator.EvaluateReadiness(new ReadinessChannel(), null));
            Assert.Throws<ArgumentException>(() => evaluator.EvaluateReadiness(new ReadinessChannel(), new CatalogProduct[0]));
            // pricelist
            Assert.Throws<NullReferenceException>(() => evaluator.EvaluateReadiness(new ReadinessChannel(), new[] { new CatalogProduct() }));
        }

        [Fact]
        public void PartiallyLoadedProducts()
        {
            var evaluator = GetReadinessEvaluator();
            PrepareData();
            evaluator.EvaluateReadiness(GetChannel(), new[] { new CatalogProduct { Id = "Valid" } });
        }

        public void PropertiesValidation()
        {
        }

        public void DescriptionsValidation()
        {
        }


        public static IEnumerable<object[]> Prices
        {
            get
            {
                yield return new object[] { null, 0 };
                yield return new object[] { new Price[0], 0 };
                foreach (var productId in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    foreach (var list in new[] { -1m, 0m, 1m })
                    {
                        yield return new object[]
                        {
                            new[] { new Price { ProductId = productId, List = list }, new Price { ProductId = "Invalid", List = 0 } },
                            productId == "Valid" && list == 1m ? 100 : 0
                        };
                    }
                }
            }
        }

        [Theory]
        [MemberData("Prices")]
        public void PricesValidation(Price[] prices, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            PrepareData();

            pricelist.Prices = prices;
            var readiness = evaluator.EvaluateReadiness(GetChannel(), products);
            Assert.True(readiness[0].Details.First(x => x.Name == "Prices").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> SeoInfos
        {
            get
            {
                yield return new object[] { null, 0 };
                yield return new object[] { new SeoInfo[0], 0 };
                foreach (var languageCode in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    foreach (var semanticUrl in new[] { null, string.Empty, "Invalid!", "Valid" })
                    {
                        yield return new object[]
                        {
                            new[] { new SeoInfo { LanguageCode = languageCode, SemanticUrl = semanticUrl }, new SeoInfo { LanguageCode = "Invalid", SemanticUrl = "Invalid" } },
                            languageCode == "Valid" && semanticUrl == "Valid" ? 100 : 0
                        };
                    }
                }
            }
        }

        [Theory]
        [MemberData("SeoInfos")]
        public void SeoValidation(SeoInfo[] seoInfos, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            PrepareData();

            products[0].SeoInfos = seoInfos;
            var readiness = evaluator.EvaluateReadiness(GetChannel(), products);
            Assert.True(readiness[0].Details.First(x => x.Name == "Seo").ReadinessPercent == readinessPercent);
        }

        private void PrepareData()
        {
            products = new[]
            {
                new CatalogProduct
                {
                    Id = "Valid",
                    Properties = new List<Property>(),
                    PropertyValues = new List<PropertyValue>(),
                    Reviews = new List<EditorialReview>(),
                    SeoInfos = new List<SeoInfo>()
                }
            };
            pricelistId = "Valid";
            pricelist = new Pricelist { Id = pricelistId, Prices = new List<Price>() };
            editorialReviewTypes = new[] { "Valid" };
        }

        private ReadinessChannel GetChannel()
        {
            return new ReadinessChannel
            {
                Name = "Test",
                Language = "Valid",
                PricelistId = pricelistId,
                CatalogId = catalogId
            };
        }

        private DefaultReadinessEvaluator GetReadinessEvaluator()
        {
            return new DefaultReadinessEvaluator(GetReadinessService(), GetProductService(), GetPricingService(), GetSettingManager());
        }

        private IReadinessService GetReadinessService()
        {
            var service = new Mock<IReadinessService>();
            service.Setup(x => x.SaveEntries(It.IsAny<ReadinessEntry[]>()));
            return service.Object;
        }

        private IItemService GetProductService()
        {
            var service = new Mock<IItemService>();
            service.Setup(x => x.GetById(
                    It.Is<string>(id => products.Any(p => id == p.Id)),
                    It.Is<ItemResponseGroup>(r => r.HasFlag(ItemResponseGroup.ItemProperties | ItemResponseGroup.ItemEditorialReviews | ItemResponseGroup.Seo)),
                    It.Is<string>(id => id == catalogId)))
                .Returns<string, ItemResponseGroup, string>((pId, r, cId) => products.First(x => x.Id == pId));
            service.Setup(x => x.Update(It.Is<CatalogProduct[]>(p => p == products)));
            return service.Object;
        }

        private IPricingService GetPricingService()
        {
            var service = new Mock<IPricingService>();
            service.Setup(x => x.GetPricelistsById(It.Is<string[]>(ids => ids[0] == pricelistId)))
                .Returns<string[]>(ids => new[] { pricelist });
            return service.Object;
        }

        private ISettingsManager GetSettingManager()
        {
            const string editorialReviewTypesSettingName = "Catalog.EditorialReviewTypes";
            var service = new Mock<ISettingsManager>();
            service.Setup(x => x.GetSettingByName(It.Is<string>(n => n == editorialReviewTypesSettingName)))
                .Returns<string>(x => new SettingEntry { AllowedValues = editorialReviewTypes });
            return service.Object;
        }
    }
}