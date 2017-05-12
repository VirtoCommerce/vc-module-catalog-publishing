using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Moq;
using VirtoCommerce.CatalogPublishingModule.Core.Model;
using VirtoCommerce.CatalogPublishingModule.Data.Core.Services;
using VirtoCommerce.Domain.Catalog.Model;
using VirtoCommerce.Domain.Catalog.Services;
using VirtoCommerce.Domain.Commerce.Model;
using VirtoCommerce.Domain.Pricing.Model.Search;
using VirtoCommerce.Domain.Pricing.Services;
using VirtoCommerce.Platform.Core.Settings;
using Xunit;
using Property = VirtoCommerce.CatalogPublishingModule.Test.Model.Property;
using PropertyDictionaryValue = VirtoCommerce.CatalogPublishingModule.Test.Model.PropertyDictionaryValue;
using PropertyValue = VirtoCommerce.CatalogPublishingModule.Test.Model.PropertyValue;
using EditorialReview = VirtoCommerce.CatalogPublishingModule.Test.Model.EditorialReview;
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
            Reviews = new List<EditorialReview>().Cast<Domain.Catalog.Model.EditorialReview>().ToList(),
            SeoInfos = new List<SeoInfo>(),
            Outlines = new List<Outline>
            {
                new Outline { Items = new List<OutlineItem>
                {
                    new OutlineItem { Id = "Valid" }
                } }
            }
        };
        private Price[] _prices = new Price[0];
        private string[] _editorialReviewTypes = { "Valid" };
        private readonly ReadinessChannel _channel = new ReadinessChannel
        {
            Name = "Valid",
            Languages = new List<string> { "Valid", },
            Currencies = new List<string> { "Valid", },
            CatalogId = "Valid"
        };

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
            evaluator.EvaluateReadiness(_channel, new[] { new CatalogProduct { Id = "Valid" } });
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

                var languages = new[] { "Valid" };

                // Only required properties accounted
                var property = new Property { Id = "Valid", ValueType = PropertyValueType.ShortText, Dictionary = false, DictionaryValues = null };
                foreach (var required in new[] { false, true })
                {
                    var currentProperty = property;
                    currentProperty.Required = required;
                    yield return Prepend(TestCondition(required,
                            r => new PropertyValue { Property = currentProperty, ValueType = PropertyValueType.ShortText, Value = "Valid" }, r => 100, Mutable),
                        languages, new List<Property> { currentProperty });
                }

                // All property values accounted when property is not multilanguage
                var properties = new List<Property>
                {
                    new Property
                    {
                        Id = "Valid1",
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = true,
                        DictionaryValues = new[] { new PropertyDictionaryValue { Value = dictionaryPropertyValues[PropertyValueType.ShortText][0] } }
                    },
                    new Property { Id = "Valid2", Required = true, ValueType = PropertyValueType.ShortText, Dictionary = false, DictionaryValues = null }
                };
                foreach (var language in new[] { "Invalid", "Valid" })
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var value = new PropertyValue { ValueType = PropertyValueType.ShortText, Property = properties[i], Value = "Valid" + (i + 1) };
                        yield return Prepend(TestCondition(language, l => value, l => 100, Mutable), languages, new List<Property> { properties[i] });
                    }
                }

                // Only property values with language from channel language list accounted when property is multilanguage
                properties = new List<Property>
                {
                    new Property
                    {
                        Id = "Valid1",
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Multilanguage = true,
                        Dictionary = true,
                        DictionaryValues = new[] { new PropertyDictionaryValue { LanguageCode = "Valid", Value = dictionaryPropertyValues[PropertyValueType.ShortText][0] } }
                    },
                    new Property { Id = "Valid2", Required = true, ValueType = PropertyValueType.ShortText, Multilanguage = true, Dictionary = false, DictionaryValues = null }
                };
                foreach (var language in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    for (var i = 0; i < 2; i++)
                    {
                        var value = new PropertyValue { LanguageCode = language, ValueType = PropertyValueType.ShortText, Property = properties[i], Value = "Valid" + (i + 1) };
                        yield return Prepend(TestCondition(language, l => value, l => l == "Valid" ? 100 : 0, Mutable), languages, new List<Property> { properties[i] });
                    }
                }

                // Check case when property is dicrionary, but there is no dictionary values
                property = new Property { Id = "Valid", Required = true, ValueType = PropertyValueType.ShortText, Dictionary = true };
                foreach (var dictionaryValue in new[] { null, new PropertyDictionaryValue[0] })
                {
                    var currentProperty = property;
                    currentProperty.DictionaryValues = dictionaryValue;
                    yield return Prepend(TestCondition(dictionaryValue,
                            dv => new PropertyValue { Property = currentProperty, ValueType = PropertyValueType.ShortText, Value = "Valid" }, dv => 100, Mutable),
                        languages, new List<Property> { currentProperty });
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
                                .Select((x, i) => new PropertyDictionaryValue { Value = dictionaryPropertyValues[type][i] })
                                .ToArray()
                        })
                        .ToList();

                    var validValues = properties
                        .Select((p, i) => new PropertyValue { Property = p, ValueType = type, Value = dictionaryValues[type][i] })
                        .ToArray();
                    var invalidValues = properties
                        .Select((p, i) => new PropertyValue { Property = p, ValueType = type, Value = valuesNotInDictionary[type][i] })
                        .ToArray();

                    foreach (var data in TestAll(validValues, 100, Mutable))
                    {
                        yield return Prepend(data, languages, properties);
                    }
                    foreach (var data in TestAll(invalidValues, 0, Mutable))
                    {
                        yield return Prepend(data, languages, properties);
                    }

                    // Check correct validation of non-dictionary properties
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
                        yield return Prepend(TestCondition(value, x => value, x => 100, Mutable), languages, new List<Property> { properties[i] });
                    }
                    if (invalidPropertyValues.ContainsKey(type))
                    {
                        for (var i = 0; i < invalidPropertyValues[type].Length; i++)
                        {
                            var value = invalidPropertyValues[type][i];
                            value.Property = properties[i];
                            yield return Prepend(TestCondition(value, x => value, x => 0, Mutable), languages, new List<Property> { properties[i] });
                        }
                    }
                }

                // Check correct percentage calculation for non-multilanguage properties with dictionaries
                var dictionaryTestingProperties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = true,
                        DictionaryValues = dictionaryValues[PropertyValueType.ShortText]
                            .Select((x, i) => new PropertyDictionaryValue { Value = dictionaryPropertyValues[PropertyValueType.ShortText][i] })
                            .ToArray()
                    })
                    .ToList();
                foreach (var data in TestAll(languages.SelectMany(l => dictionaryTestingProperties.Select(p =>
                    new PropertyValue { Property = p, LanguageCode = l, ValueType = PropertyValueType.ShortText, Value = dictionaryPropertyValues[PropertyValueType.ShortText][0] }
                )).ToArray(), 100, Mutable))
                {
                    yield return Prepend(data, languages, dictionaryTestingProperties);
                }

                // Check correct percentage calculation for non-multilanguage non-dictionary properties
                var testingProperties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = false,
                        DictionaryValues = null
                    })
                    .ToList();
                foreach (var data in TestAll(testingProperties.Select(p => new PropertyValue { Property = p, ValueType = PropertyValueType.ShortText, Value = "Valid" }).ToArray(), 100, Mutable))
                {
                    yield return Prepend(data, languages, testingProperties);
                }

                languages = new[] { "Valid1", "Valid2" };

                // Check correct percentage calculation for multilanguage properties with dictionaries
                var dictionaryMultiLanguageTestingProperties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Multilanguage = true,
                        Dictionary = true,
                        DictionaryValues = languages.SelectMany(l => dictionaryValues[PropertyValueType.ShortText]
                                .Select((x, i) => new PropertyDictionaryValue { LanguageCode = l, Value = dictionaryPropertyValues[PropertyValueType.ShortText][i] }))
                            .ToArray()
                    })
                    .ToList();
                foreach (var data in TestAll(languages.SelectMany(l => dictionaryMultiLanguageTestingProperties.Select(p =>
                        new PropertyValue { Property = p, LanguageCode = l, ValueType = PropertyValueType.ShortText, Value = dictionaryPropertyValues[PropertyValueType.ShortText][0] }
                    )).ToArray(), 100, Mutable))
                {
                    yield return Prepend(data, languages, dictionaryMultiLanguageTestingProperties);
                }

                // Check correct percentage calculation for non-dictionary multilanguage properties
                var multiLanguageTestingProperties = propertyIds.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Multilanguage = true,
                        Dictionary = false,
                        DictionaryValues = null
                    })
                    .ToList();
                foreach (var data in TestAll(languages.SelectMany(l => multiLanguageTestingProperties.Select(p =>
                        new PropertyValue { Property = p, LanguageCode = l, ValueType = PropertyValueType.ShortText, Value = "Valid" }
                    )).ToArray(), 100, Mutable))
                {
                    yield return Prepend(data, languages, multiLanguageTestingProperties);
                }

                languages = new[] { "Valid" };

                // Check correct calculation for mixed collection of multilanguage and non-multilanguage properties
                var singleLanguageMixedTestingProperties = new[] { "Valid1", "Valid2" }.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Dictionary = false,
                        DictionaryValues = null
                    })
                    .ToList();
                var multiLanguageMixedTestingProperties = new[] { "Valid3", "Valid4" }.Select(id => new Property
                    {
                        Id = id,
                        Required = true,
                        ValueType = PropertyValueType.ShortText,
                        Multilanguage = true,
                        Dictionary = false,
                        DictionaryValues = null
                    })
                    .ToList();
                foreach (var data in TestAllValid(
                    new[]
                    {
                        new PropertyValue { Property = multiLanguageMixedTestingProperties[0], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = null },
                        new PropertyValue { Property = singleLanguageMixedTestingProperties[0], ValueType = PropertyValueType.ShortText, Value = null }
                    },
                    new[]
                    {
                        new PropertyValue { Property = multiLanguageMixedTestingProperties[1], LanguageCode = "Valid", ValueType = PropertyValueType.ShortText, Value = "Valid" },
                        new PropertyValue { Property = singleLanguageMixedTestingProperties[1], ValueType = PropertyValueType.ShortText, Value = "Valid" }
                    },
                    true))
                {
                    yield return Prepend(data, languages, ((List<PropertyValue>) data[0]).Select(pv => (Property) pv.Property).Distinct().ToList());
                }
            }}

        [Theory]
        [MemberData(nameof(Properties))]
        public void PropertiesValidation(string[] languages, List<Property> properties, List<PropertyValue> values, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _channel.Languages = languages;
            _product.Properties = properties.Cast<Domain.Catalog.Model.Property>().ToList();
            _product.PropertyValues = values.Cast<Domain.Catalog.Model.PropertyValue>().ToList();
            var readiness = evaluator.EvaluateReadiness(_channel, new [] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Properties").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> Descriptions
        {
            get
            {
                foreach (var emptyReviewTypes in new[] { null, new string[0] })
                {
                    yield return Prepend(TestCondition(emptyReviewTypes, x => new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid" }, x => 100), new[] { "Valid" }, emptyReviewTypes);
                }
                var languages = new[] { "Valid" };
                var types = new[] { "Valid" };
                foreach (var variant in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    yield return Prepend(TestCondition(variant,
                        l => new EditorialReview { LanguageCode = l, Content = "Valid", ReviewType = "Valid" }, l => l == "Valid" ? 100 : 0), languages, types);
                }
                foreach (var variant in new[] { null, string.Empty, "Valid" })
                {
                    yield return Prepend(TestCondition(variant,
                        c => new EditorialReview { LanguageCode = "Valid", Content = c, ReviewType = "Valid" }, c => c == "Valid" ? 100 : 0), languages, types);
                }
                foreach (var data in TestAnyValid(new EditorialReview { LanguageCode = "Invalid", Content = "Invalid", ReviewType = "Invalid" },
                    new EditorialReview { LanguageCode = "Valid", Content = "Valid", ReviewType = "Valid" }))
                {
                    yield return Prepend(data, languages, types);
                }
                languages = new[] { "Valid1", "Valid2" };
                types = new[] { "Valid1", "Valid2" };
                foreach (var data in TestAll(languages.SelectMany(l => types.Select(t => new EditorialReview { LanguageCode = l, Content = "Valid", ReviewType = t })).ToArray(), 100))
                {
                    yield return Prepend(data, languages, types);
                }
            }
        }

        [Theory]
        [MemberData(nameof(Descriptions))]
        public void DescriptionsValidation(string[] languages, string[] descriptionTypes, EditorialReview[] descriptions, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _channel.Languages = languages;
            _editorialReviewTypes = descriptionTypes;
            _product.Reviews = descriptions;
            var readiness = evaluator.EvaluateReadiness(_channel, new[] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Descriptions").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> Prices
        {
            get
            {
                yield return new object[] { new[] { "Valid" }, null, 0 };
                yield return new object[] { new[] { "Valid" }, new Price[0], 0 };

                var currencies = new[] { "Valid" };
                foreach (var currency in new[] { "Invalid", "Valid" })
                {
                    yield return Prepend(TestCondition(currency, c => new Price { ProductId = "Valid", Currency = c, List = 1m }, c => c == "Valid" ? 100 : 0), new object[] { currencies });
                }
                foreach (var list in new[] { -1m, 0m, 1m })
                {
                    yield return Prepend(TestCondition(list, l => new Price { ProductId = "Valid", Currency = "Valid", List = l }, l => l > 0 ? 100 : 0), new object[] { currencies });
                }
                foreach (var data in TestAnyValid(new Price { ProductId = "Valid", Currency = "Valid", List = -1m }, new Price { ProductId = "Valid", Currency = "Valid", List = 1m }))
                {
                    yield return Prepend(data, new object[] { currencies });
                }
                currencies = new[] { "Valid1", "Valid2" };
                foreach (var data in TestAll(currencies.Select(c => new Price { ProductId = "Valid", Currency = c, List = 1m }).ToArray(), 100))
                {
                    yield return Prepend(data, new object[] { currencies });
                }
            }
        }

        [Theory]
        [MemberData(nameof(Prices))]
        public void PricesValidation(string[] currencies, Price[] prices, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _channel.Currencies = currencies;
            _prices = prices;
            var readiness = evaluator.EvaluateReadiness(_channel, new[] { _product });
            Assert.True(readiness[0].Details.First(x => x.Name == "Prices").ReadinessPercent == readinessPercent);
        }

        public static IEnumerable<object[]> SeoInfos
        {
            get
            {
                yield return new object[] { new[] { "Valid" }, null, 0 };
                yield return new object[] { new[] { "Valid" }, new SeoInfo[0], 0 };
                var languages = new[] { "Valid" };
                foreach (var language in new[] { null, string.Empty, "Invalid", "Valid" })
                {
                    yield return Prepend(TestCondition(language, l => new SeoInfo { LanguageCode = l, SemanticUrl = "Valid" }, l => l == "Valid" ? 100 : 0), new object[] { languages });
                }
                foreach (var semanticUrl in new[] { null, string.Empty, "Invalid!", "Valid" })
                {
                    yield return Prepend(TestCondition(semanticUrl, u => new SeoInfo { LanguageCode = "Valid", SemanticUrl = u }, u => u == "Valid" ? 100 : 0), new object[] { languages });
                }
                foreach (var data in TestAnyValid(new SeoInfo { LanguageCode = "Invalid", SemanticUrl = "Invalid!" }, new SeoInfo { LanguageCode = "Valid", SemanticUrl = "Valid" }))
                {
                    yield return Prepend(data, new object[] { languages });
                }
                languages = new[] { "Valid1", "Valid2" };
                foreach (var data in TestAll(languages.Select(l => new SeoInfo { LanguageCode = l, SemanticUrl = "Valid" }).ToArray(), 100))
                {
                    yield return Prepend(data, new object[] { languages });
                }
            }
        }

        [Theory]
        [MemberData(nameof(SeoInfos))]
        public void SeoValidation(string[] languages, SeoInfo[] seoInfos, int readinessPercent)
        {
            var evaluator = GetReadinessEvaluator();
            _channel.Languages = languages;
            _product.SeoInfos = seoInfos;
            var readiness = evaluator.EvaluateReadiness(_channel, new[] { _product });
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
            for (var firstIndex = 0; firstIndex < variants.Length - 1; firstIndex++)
            {
                var first = variants[firstIndex];
                for (var secondIndex = firstIndex + 1; secondIndex < variants.Length; secondIndex++)
                {
                    var second = variants[secondIndex];
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
        
        private static IEnumerable<object[]> TestAllValid<TObject>(TObject[] invalidObjects, TObject[] validObjects, bool mutable = false)
            where TObject : class
        {
            if (invalidObjects.Length != validObjects.Length)
            {
                throw new InvalidOperationException();
            }
            for (var invalidCount = invalidObjects.Length; invalidCount >= 0; invalidCount--)
            {
                var invalid = invalidObjects.Take(invalidCount).ToArray();
                var valid = validObjects.Skip(invalidCount).ToArray();
                var objects = invalid.Concat(valid).ToArray();
                yield return new object[]
                {
                    mutable ? (ICollection<TObject>)objects.ToList() : objects.ToArray(),
                    (int)Math.Floor((double)valid.Length / (double)objects.Length * 100d)
                };
            }
        }

        private static IEnumerable<object[]> TestAll<TObject>(TObject[] objects, double readinessPercent, bool mutable = false)
        {
            var variants = new List<TObject[]>();
            variants.AddRange(Enumerable.Range(0, objects.Length).Select(i => objects.Take(i).ToArray()));
            foreach (var variant in mutable ? (IEnumerable<ICollection<TObject>>)variants.Select(x => x.ToList()) : variants)
            {
                yield return new object[]
                {
                    variant,
                    (int)Math.Floor(readinessPercent * (double)variant.Count / (double)objects.Length)
                };
            }
        }

        private DefaultReadinessEvaluator GetReadinessEvaluator()
        {
            return new DefaultReadinessEvaluator(new DefaultReadinessDetailEvaluator[]
            {
                new PropertiesReadinessDetailEvaluator(), new DescriptionsReadinessDetailEvaluator(GetSettingManager()), new PricesReadinessDetailEvaluator(GetPricingSearchService()), new SeoReadinessDetailEvaluator()
            }, GetProductService());
        }

        private IItemService GetProductService()
        {
            var service = new Mock<IItemService>();
            service.Setup(x => x.GetByIds(
                    It.Is<string[]>(ids => ids.Length == 1 && ids.Contains(_product.Id)),
                    It.Is<ItemResponseGroup>(r => r == ItemResponseGroup.ItemLarge),
                    It.Is<string>(id => id == null)))
                .Returns<string[], ItemResponseGroup, string>((pId, r, cId) => new[] { _product });
            service.Setup(x => x.Update(It.Is<CatalogProduct[]>(p => p.Contains(_product))));
            return service.Object;
        }

        private IPricingSearchService GetPricingSearchService()
        {
            var service = new Mock<IPricingSearchService>();
            service.Setup(x => x.SearchPrices(It.IsAny<PricesSearchCriteria>()))
                .Returns<PricesSearchCriteria>(c => new PricingSearchResult<Domain.Pricing.Model.Price> { Results = _prices?.Where(p => p.PricelistId == c.PriceListId).ToArray() });
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