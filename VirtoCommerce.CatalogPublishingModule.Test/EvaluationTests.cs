using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Core.Services;
using VirtoCommerce.CatalogPublishingModule.Data.Model.Details;
using VirtoCommerce.CatalogPublishingModule.Data.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using Property = VirtoCommerce.CatalogPublishingModule.Test.Model.Property;
using PropertyValue = VirtoCommerce.CatalogPublishingModule.Test.Model.PropertyValue;
using Price = VirtoCommerce.CatalogPublishingModule.Test.Model.Price;

namespace VirtoCommerce.CatalogPublishingModule.Test
{
    [CLSCompliant(false)]
    [Trait("Category", "CI")]
    public class EvaluationTests
    {
        private readonly CatalogProduct _product = new CatalogProduct
        {
            Id = "Valid",
            CatalogId = "Valid",
            Properties = new List<Property>().Cast<Domain.Catalog.Model.Property>().ToList(),
            PropertyValues = new List<PropertyValue>().Cast<Domain.Catalog.Model.PropertyValue>().ToList(),
            Reviews = new List<EditorialReview>(),
            SeoInfos = new List<SeoInfo>()
        };
        private Price[] _pricelistPrices = new Price[0];
        private string[] _editorialReviewTypes = new[] { "Valid" };

        [Fact]
        public void InvalidParameters()
        {
            var evaluator = GetReadinessEvaluator();
            // catalog
            Assert.Throws<ArgumentNullException>(() => evaluator.EvaluateReadiness(null, new[] { new CatalogProduct() }));
            // products
            Assert.Throws<ArgumentException>(() => evaluator.EvaluateReadiness(new ReadinessChannel(), null));
            Assert.Throws<ArgumentException>(() => evaluator.EvaluateReadiness(new ReadinessChannel(), new CatalogProduct[0]));
        }

        [Fact]
        public void PartiallyLoadedProducts()
        {
            var evaluator = GetReadinessEvaluator();
            evaluator.EvaluateReadiness(GetChannel(), new[] { new CatalogProduct { Id = "Valid" } });
        }

        private const bool Mutable = true;

        public static IEnumerable<object[]> Properties
        {
            get
            {
                var valueTypes = new[] { PropertyValueType.LongText, PropertyValueType.ShortText, PropertyValueType.Number, PropertyValueType.DateTime, PropertyValueType.Boolean };
                var dictionaryValues = new Dictionary<PropertyValueType, object[]>
                {
                    { PropertyValueType.LongText, new[] { "Valid1", "Valid2" } },
                    { PropertyValueType.ShortText, new[] { "Valid1", "Valid2" } },
                    { PropertyValueType.Number, new[] { (object)0m, (object)1m } },
                    { PropertyValueType.DateTime, new[] { (object)new DateTime(1970, 1, 1, 0, 0, 0), (object)new DateTime(2000, 1, 1, 0, 0, 0) } },
                    { PropertyValueType.Boolean, new[] { (object)true, (object)true } }
                };
                var dictionaryPropertyValues = new Dictionary<PropertyValueType, string[]>
                {
                    { PropertyValueType.LongText, (string[]) dictionaryValues[PropertyValueType.LongText] },
                    { PropertyValueType.ShortText, (string[]) dictionaryValues[PropertyValueType.ShortText] },
                    { PropertyValueType.Number, dictionaryValues[PropertyValueType.Number].Cast<decimal>().Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray() },
                    { PropertyValueType.DateTime, dictionaryValues[PropertyValueType.DateTime].Cast<DateTime>().Select(x => x.ToString()).ToArray() },
                    { PropertyValueType.Boolean, dictionaryValues[PropertyValueType.Boolean].Cast<bool>().Select(x => x.ToString()).ToArray() }
                };
                var valuesNotInDictionary = new Dictionary<PropertyValueType, object[]>
                {
                    { PropertyValueType.LongText, new[] { "Invalid1", "Invalid2" } },
                    { PropertyValueType.ShortText, new[] { "Invalid1", "Invalid2" } },
                    { PropertyValueType.Number, new[] { (object) 2m, (object) 3m } },
                    { PropertyValueType.DateTime, new[] { (object) new DateTime(1980, 1, 1, 0, 0, 0), (object) new DateTime(2010, 1, 1, 0, 0, 0) } },
                    { PropertyValueType.Boolean, new[] { (object) false, (object) false } }
                };
                var validPropertyValues = new Dictionary<PropertyValueType, PropertyValue[]>
                {
                    { PropertyValueType.LongText, new[] { new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.LongText, Value = "Valid" } } },
                    { PropertyValueType.ShortText, new[] { new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid" } } },
                    {
                        PropertyValueType.Number, new[]
                        {
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.Number, Value = 0m },
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.Number, Value = 1m },
                        }
                    },
                    { PropertyValueType.DateTime, new[] { new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.DateTime, Value = DateTime.Now } } },
                    {
                        PropertyValueType.Boolean, new[]
                        {
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.Boolean, Value = false },
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.Boolean, Value = true }
                        }
                    }
                };
                var invalidPropertyValues = new Dictionary<PropertyValueType, PropertyValue[]>
                {
                    {
                        PropertyValueType.LongText, new[]
                        {
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.LongText, Value = null },
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.LongText, Value = string.Empty }
                        }
                    },
                    {
                        PropertyValueType.ShortText, new[]
                        {
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = null },
                            new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = string.Empty }
                        }
                    },
                    { PropertyValueType.Number, new[] { new PropertyValue { LanguageCode = "Valid", ValueType = PropertyValueType.Number, Value = -1m } } }
                };

                // Only required properties accounted
                var property = new Property { Id = "Valid", ValueType = PropertyValueType.ShortText, Dictionary = false, DictionaryValues = null };
                foreach (var variant in new[] { false, true })
                {
                    var currentProperty = property;
                    currentProperty.Required = variant;
                    yield return Prepend(TestCondition(variant,
                            x => new PropertyValue { Property = currentProperty, LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid" }, x => 100, Mutable),
                        new List<Property> { currentProperty });
                }

                // Only property values with language same as channel language accounted
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "Valid1",
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = true,
                        DictionaryValues = new[] { new PropertyDictionaryValue { LanguageCode = "Valid", Value = dictionaryPropertyValues[PropertyValueType.ShortText][0] } }
                    },
                    new Property { Id = "Valid2", Required = true, ValueType = PropertyValueType.ShortText, Dictionary = false, DictionaryValues = null }
                };
                foreach (var variant in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var value = new PropertyValue { LanguageCode = variant, ValueType = PropertyValueType.ShortText, Property = properties[i], Value = i == 0 ? "Valid1" : "Valid" };
                        yield return Prepend(TestCondition(variant, x => value, x => x == "Valid" ? 100 : 0, Mutable), new List<Property> { properties[i] });
                    }
                }

                // Check case when property is dicrionary, but there is no dictionary values
                property = new Property { Id = "Valid", Required = true, ValueType = PropertyValueType.ShortText, Dictionary = true };
                foreach (var variant in new[] { null, new PropertyDictionaryValue[0] })
                {
                    var currentProperty = property;
                    currentProperty.DictionaryValues = variant;
                    yield return Prepend(TestCondition(variant,
                            x => new PropertyValue { Property = currentProperty, LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid" }, x => 100, Mutable),
                        new List<Property> { currentProperty });
                }

                var propertyIds = new[] { "Valid1", "Valid2" };
                foreach (var type in valueTypes)
                {
                    // Check correct validation of properties with dicrionaries
                    properties = propertyIds.Select(id => new Property
                        {
                            Id = id,
                            Required = true,
                            ValueType = type,
                            Dictionary = true,
                            DictionaryValues = dictionaryValues[type]
                                .Select((x, i) => new PropertyDictionaryValue { LanguageCode = "Valid", Value = dictionaryPropertyValues[type][i] })
                                .ToArray()
                        })
                        .ToList();

                    var validValues = properties
                        .Select((p, i) => new PropertyValue { Property = p, LanguageCode = "Valid", ValueType = type, Value = dictionaryValues[type][i] })
                        .ToList();
                    var invalidValues = properties
                        .Select((p, i) => new PropertyValue { Property = p, LanguageCode = "Valid", ValueType = type, Value = valuesNotInDictionary[type][i] })
                        .ToList();

                    foreach (var data in TestAll(validValues[0], validValues[1], 100, Mutable))
                    {
                        yield return Prepend(data, properties);
                    }
                    foreach (var data in TestAll(invalidValues[0], invalidValues[1], 0, Mutable))
                    {
                        yield return Prepend(data, properties);
                    }

                    // Check correct validation of usual properties
                    properties = propertyIds.Select(id => new Property
                        {
                            Id = id,
                            Required = true,
                            ValueType = type,
                            Dictionary = false,
                            DictionaryValues = null
                        })
                        .ToList();
                    for (var i = 0; i < validPropertyValues[type].Length; i++)
                    {
                        var value = validPropertyValues[type][i];
                        value.Property = properties[i];
                        yield return Prepend(TestCondition(value, x => value, x => 100, Mutable), new List<Property> { properties[i] });
                    }
                    if (invalidPropertyValues.ContainsKey(type))
                    {
                        for (var i = 0; i < invalidPropertyValues[type].Length; i++)
                        {
                            var value = invalidPropertyValues[type][i];
                            value.Property = properties[i];
                            yield return Prepend(TestCondition(value, x => value, x => 0, Mutable), new List<Property> { properties[i] });
                        }
                    }
                }

                // Check correct percentage calculation for properties with dicrionaries
                properties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = true,
                        DictionaryValues = dictionaryValues[PropertyValueType.ShortText]
                            .Select((x, i) => new PropertyDictionaryValue { LanguageCode = "Valid", Value = dictionaryPropertyValues[PropertyValueType.ShortText][i] })
                            .ToArray()
                    })
                    .ToList();
                foreach (var data in TestAll(new PropertyValue { Property = properties[0], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid1" },
                    new PropertyValue { Property = properties[1], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid2" }, 100, Mutable))
                {
                    yield return Prepend(data, properties);
                }

                // Check correct percentage calculation for usual properties
                properties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = false,
                        DictionaryValues = null
                    })
                    .ToList();
                foreach (var data in TestAll(new PropertyValue { Property = properties[0], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid1" },
                    new PropertyValue { Property = properties[1], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid2" }, 100, Mutable))
                {
                    yield return Prepend(data, properties);
                }
            }
        }

        [Theory]
        [MemberData(nameof(Properties))]
        public void PropertiesValidation(List<Property> properties, List<PropertyValue> values, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _product.Properties = properties.Cast<Domain.Catalog.Model.Property>().ToList();
            _product.PropertyValues = values.Cast<Domain.Catalog.Model.PropertyValue>().ToList();
            var readiness = evaluator.EvaluateReadiness(GetChannel(), new [] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Properties").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> Descriptions
        {
            get
            {
                foreach (var variant in new[] { null, new string[0] })
                {
                    yield return Prepend(TestCondition(variant, x => new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid" }, x => 100), new object[] { variant });
                }
                var types = new[] { "Valid" };
                foreach (var variant in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    yield return Prepend(TestCondition(variant,
                        x => new EditorialReview { LanguageCode = x, Content = "Valid", ReviewType = "Valid" }, x => x == "Valid" ? 100 : 0), new object[] { types });
                }
                foreach (var variant in new[] { null, string.Empty, "Valid" })
                {
                    yield return Prepend(TestCondition(variant,
                        x => new EditorialReview { LanguageCode = "Valid", Content = x, ReviewType = "Valid" }, x => x == "Valid" ? 100 : 0), new object[] { types });
                }
                foreach (var data in TestAnyValid(new EditorialReview { LanguageCode = "Invalid", Content = "Invalid", ReviewType = "Invalid" },
                    new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid" }))
                {
                    yield return Prepend(data, new object[] { types });
                }
                types = new[] { "Valid1", "Valid2" };
                foreach (var data in TestAll(new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid1" },
                    new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid2" }, 100))
                {
                    yield return Prepend(data, new object[] { types });
                }
            }
        }

        [Theory]
        [MemberData(nameof(Descriptions))]
        public void DescriptionsValidation(string[] descriptionTypes, EditorialReview[] descriptions, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _editorialReviewTypes = descriptionTypes;
            _product.Reviews = descriptions;
            var readiness = evaluator.EvaluateReadiness(GetChannel(), new[] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Descriptions").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> Prices
        {
            get
            {
                yield return new object[] { null, 0 };
                yield return new object[] { new Price[0], 0 };
                foreach (var variant in new[] { "Invalid", "Valid" })
                {
                    yield return TestCondition(variant, x => new Price { ProductId = "Valid", PricelistId = variant, List = 1m }, x => x == "Valid" ? 100 : 0);
                }
                foreach (var variant in new[] { -1m, 0m, 1m })
                {
                    yield return TestCondition(variant, x => new Price { ProductId = "Valid", PricelistId = "Valid", List = x }, x => x > 0 ? 100 : 0);
                }
                foreach (var data in TestAnyValid(new Price { ProductId = "Valid", PricelistId = "Valid", List = -1m }, new Price { ProductId = "Valid", PricelistId = "Valid", List = 1m }))
                {
                    yield return data;
                }
            }
        }

        [Theory]
        [MemberData(nameof(Prices))]
        public void PricesValidation(Price[] prices, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _pricelistPrices = prices;
            var readiness = evaluator.EvaluateReadiness(GetChannel(), new[] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Prices").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> SeoInfos
        {
            get
            {
                foreach (var variant in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    yield return TestCondition(variant, x => new SeoInfo { LanguageCode = x, SemanticUrl = "Valid" }, x => x == "Valid" ? 100 : 0);
                }
                foreach (var variant in new[] { null, string.Empty, "Invalid!", "Valid" })
                {
                    yield return TestCondition(variant, x => new SeoInfo { LanguageCode = "Valid", SemanticUrl = x }, x => x == "Valid" ? 100 : 0);
                }
                foreach (var data in TestAnyValid(new SeoInfo { LanguageCode = "Invalid", SemanticUrl = "Invalid!" }, new SeoInfo { LanguageCode = "Valid", SemanticUrl = "Valid" }))
                {
                    yield return data;
                }
            }
        }

        [Theory]
        [MemberData(nameof(SeoInfos))]
        public void SeoValidation(SeoInfo[] seoInfos, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _product.SeoInfos = seoInfos;
            var readiness = evaluator.EvaluateReadiness(GetChannel(), new[] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Seo").ReadinessPercent == readinessPercent);
        }

        private static object[] Prepend(object[] original, params object[] additional)
        {
            var list = additional.ToList();
            list.AddRange(original);
            return list.ToArray();
        }

        private static object[] TestCondition<TVariant, TObject>(TVariant variant, Func<TVariant, TObject> factory, Func<TVariant, int> test, bool mutable = false)
        {
            var objectVariant = new[] { factory(variant) };
            return new object[]
            {
                mutable ? (ICollection<TObject>)objectVariant.ToList() : objectVariant,
                test(variant),
            };
        }

        private static IEnumerable<object[]> TestAnyValid<TObject>(TObject invalid, TObject valid)
            where TObject: class
        {
            var variants = new[] { null, invalid, valid };
            foreach (var first in variants)
            {
                foreach (var second in variants)
                {
                    var objects = new List<TObject>();
                    if (first != null)
                    {
                        objects.Add(first);
                    }
                    if (second != null)
                    {
                        objects.Add(second);
                    }
                    yield return new object[]
                    {
                        objects.ToArray(),
                        new[] { first, second }.Any(seoInfo => seoInfo == valid) ? 100 : 0
                    };
                }
            }
        }

        private static IEnumerable<object[]> TestAll<TObject>(TObject first, TObject second, int readinessPercent, bool mutable = false)
        {
            var variants = new[] { new TObject[0], new[] { first }, new[] { first, second } };
            foreach (var variant in (mutable ? (IEnumerable<ICollection<TObject>>)variants.Select(x => x.ToList()) : variants))
            {
                yield return new object[]
                {
                    variant,
                    readinessPercent * variant.Count / 2
                };
            }
        }

        private ReadinessChannel GetChannel()
        {
            return new ReadinessChannel
            {
                Name = "Valid",
                Language = "Valid",
                PricelistId = "Valid",
                CatalogId = "Valid"
            };
        }

        private DefaultReadinessEvaluator GetReadinessEvaluator()
        {
            return new DefaultReadinessEvaluator(GetReadinessService(), () => new DefaultReadinessDetail[]
            {
                new PropertiesDetail(), new DescriptionsDetail(GetSettingManager()), new PricesDetail(), new SeoDetail()
            }, GetProductService(), GetPricingSearchService());
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
            service.Setup(x => x.GetByIds(
                    It.Is<string[]>(ids => ids.Length == 1 && ids.Contains(_product.Id)),
                    It.Is<ItemResponseGroup>(r => r.HasFlag(ItemResponseGroup.ItemProperties | ItemResponseGroup.ItemEditorialReviews | ItemResponseGroup.Seo)),
                    It.Is<string>(id => id == null)))
                .Returns<string[], ItemResponseGroup, string>((pId, r, cId) => new[] { _product });
            service.Setup(x => x.Update(It.Is<CatalogProduct[]>(p => p.Contains(_product))));
            return service.Object;
        }

        private IPricingSearchService GetPricingSearchService()
        {
            var service = new Mock<IPricingSearchService>();
            service.Setup(x => x.SearchPrices(It.IsAny<PricesSearchCriteria>()))
                .Returns<PricesSearchCriteria>(c => new PricingSearchResult<Domain.Pricing.Model.Price> { Results = _pricelistPrices });
            return service.Object;
        }

        private ISettingsManager GetSettingManager()
        {
            const string editorialReviewTypesSettingName = "Catalog.EditorialReviewTypes";
            var service = new Mock<ISettingsManager>();
            service.Setup(x => x.GetSettingByName(It.Is<string>(n => n == editorialReviewTypesSettingName)))
                .Returns<string>(x => new SettingEntry { ArrayValues = _editorialReviewTypes });
            return service.Object;
        }
    }
}