using NUnit.Framework;
using Cache.Models;
using Cache.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Cache.Tests
{
    [ExcludeFromCodeCoverage]
    public class InMemoryCacheServiceTests
    {
        private InMemoryCacheService _unit;
        private Uri _testUri;

        private List<int> _productIds = Enumerable.Range(1, 1000).Select(x => x).ToList();

        [SetUp]
        public void Setup()
        {
            _unit = new InMemoryCacheService();
            _testUri = new System.Uri("https://service/api/products/1");
            foreach (var id in _productIds)
            {
                _unit.Set(new System.Uri($"https://service/api/products/{id}"),
                        new CacheItem()
                        {
                            Created = DateTimeOffset.UtcNow.Subtract(new TimeSpan(id, 1, 1)),
                            Content = new { }
                        });
            }
        }

        [Test]
        public void Unit_Setup_NotNull()
        {
            Assert.IsNotNull(_unit);
        }


        [Test]
        public async Task SetShouldReturnTheUpdatedItemAfterAnItemUpdate()
        {
            var updatedItem = new CacheItem() { Created = DateTimeOffset.UtcNow.Add(new TimeSpan(1, 1, 1)), Content = new { } };

            await _unit.Set(_testUri, updatedItem);

            Assert.AreEqual(updatedItem.Created, (await _unit.Get(_testUri)).Created);
        }

        [Test]
        public void GetShouldThrowNullArgumentExceptionWhenPassedANullUri()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _unit.Get(null as System.Uri));
        }

        [Test]
        public async Task GetShouldNotReturnNullWhenPassedAMatchingUri()
        {
            Assert.IsNotNull(await _unit.Get(_testUri));
        }

        [Test]
        public async Task RemoveAllShouldEmptyTheCache()
        {
            Assert.IsNotNull(await _unit.Get(_testUri));

            await _unit.RemoveAll();

            Assert.IsNull(await _unit.Get(_testUri));
        }

        [Test]
        public async Task RemoveShouldRemoveAnItemByItsUri()
        {
            Assert.IsNotNull(await _unit.Get(_testUri));

            await _unit.Remove(_testUri);

            Assert.IsNull(await _unit.Get(_testUri));
        }

        [Test]
        public async Task SetAllShouldReplaceCacheFromACollectionOfSameResources()
        {
            // Arrange
            var item = await _unit.Get(_testUri);
            var cacheItems = CreateCacheItemsList();

            // Act
            await _unit.SetAll(cacheItems);

            // Assert
            Assert.AreNotEqual(item.Created, (await _unit.Get(_testUri)).Created, "The expected item update did not occur.");
        }

        private Dictionary<Uri, CacheItem> CreateCacheItemsList()
        {
            var items = new Dictionary<Uri, CacheItem>();
            foreach (var id in _productIds)
            {
                items.Add(new Uri($"https://service/api/products/{id}"),
                            new CacheItem()
                            {
                                Created = DateTimeOffset.UtcNow.Subtract(new TimeSpan(id, 1, 1)),
                                Content = new { }
                            });
            }
            return items;
        }

    }

}