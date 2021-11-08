using Moq;
using NUnit.Framework;
using Cache.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cache.Services;


namespace Cache.Tests
{
    [ExcludeFromCodeCoverage]
    public class PersistedCacheServiceTest
    {
        private PersistedCacheService _unit;
        private Uri _testUri;
        private List<int> _productIds = Enumerable.Range(1, 10).Select(x => x).ToList();
        private Mock<ICacheItemRepository> _repository;
        private CacheItem _testCacheItem;
        private Dictionary<Uri, CacheItem> _testCacheItems;

        [SetUp]
        public void Setup()
        {
            // Default Arrange
            _repository = new Mock<ICacheItemRepository>();
            CreateTestItems();
            
            // Default act
            _unit = new PersistedCacheService(_repository.Object);
        }

        private void CreateTestItems()
        {
            _testUri = new Uri("https://service/api/v1/products/1");
            _testCacheItem = new CacheItem() { Created = DateTimeOffset.UtcNow, Content = new byte[] { 0x00 } };
            _testCacheItems = new Dictionary<Uri, CacheItem>();

            foreach (var id in _productIds)
            {
                _testCacheItems.Add(new Uri($"https://service/api/products/{id}"),
                            new CacheItem()
                            {
                                Created = DateTimeOffset.UtcNow.Subtract(new TimeSpan(id, 1, 1)),
                                Content = new byte[] { 0x00, (byte)id }
                            });
            }
        }

        [Test]
        public void Unit_Setup_NotNull()
        {
            // Assert
            Assert.IsNotNull(_unit);
        }

        [Test]
        public void GetShouldThrowNullArgumentExceptionWhenGettingAnItemWithANullUri()
        {
            // Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _unit.Get(null as System.Uri));

            // Assert
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetShouldReturnTheItemFormTheRepositoryIfItExists()
        {
            // Arrange 
            _repository.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            // Act
            Assert.IsNotNull(await _unit.Get(_testUri));
            
            // Assert
            _repository.Verify(x => x.Get(_testUri), Times.Once);
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RemoveAllShouldCallTheRepositoryToEmptyTheCacheOnRemoveAll()
        {
            // Act
            await _unit.RemoveAll();

            // Assert
            _repository.Verify(x => x.RemoveAll(), Times.Once);
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public void RemoveShouldThrowNullArgumentExceptionWhenRemovingAnItemWithANullUri()
        {
            // Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _unit.Remove(null as System.Uri));

            // Asset
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RemoveShouldCallTheRepositoryWhenRemovingAnItemByItsUri()
        {
            // Act
            await _unit.Remove(_testUri);

            // Assert
            _repository.Verify(x => x.Remove(_testUri), Times.Once);
            _repository.VerifyNoOtherCalls();

        }

        [Test]
        public async Task SetShouldCallTheRepositoryToStoreASingleItem()
        {
            // Act
            await _unit.Set(_testUri, _testCacheItem);

            // Assert
            _repository.Verify(x => x.Set(_testUri, _testCacheItem), Times.Once);
            _repository.VerifyNoOtherCalls();
        }
        [Test]
        public void SetShouldThrowNullArgumentExceptionWhenAddingAnItemWithANullUri()
        {
            // Arrange already done

            // Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _unit.Set(null as System.Uri, _testCacheItem));

            // Assert
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetAllShouldCallTheRepositoryToStoreItemsFromACollection()
        {
            // Arrange
            // Done

            // Act
            await _unit.SetAll(_testCacheItems);

            // Assert
            _repository.Verify(x => x.SetAll(_testCacheItems), Times.Once);
            _repository.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetAllShouldCallTheRepositoryToGetAllItems()
        {
            // Arrange
            _repository.Setup(x => x.GetAll()).ReturnsAsync(_testCacheItems);

            // Act
            var items = await _unit.GetAll();

            
            CollectionAssert.IsNotEmpty(items);
            CollectionAssert.AllItemsAreInstancesOfType(items, typeof(KeyValuePair<Uri, CacheItem>));
            // Assert
            _repository.Verify(x => x.GetAll(), Times.Once);
            _repository.VerifyNoOtherCalls();
        }

    }
}