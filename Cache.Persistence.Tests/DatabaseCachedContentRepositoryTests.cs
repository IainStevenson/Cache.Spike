using AutoMapper;
using Moq;
using NUnit.Framework;
using Cache.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cache.Services;

namespace Cache.Persistence.Tests
{
    public class DatabaseCachedContentRepositoryTests
    {
        private ICacheItemRepository _unit;
        private Uri _testUri;
        private CacheItem _testCacheItem;
        private CachedContent _testCachedContent;
        private Mock<IDapperWrapper> _db;
        private Mock<IMapper> _mapper;

        [SetUp]
        public void Setup()
        {
            // Arrange default
            _testUri = new Uri("https://service/api/v1/products/1");
            _testCacheItem = new CacheItem() { Id = 1, Created = DateTimeOffset.UtcNow, Content = new byte[] { 0x00 } };
            _testCachedContent = new CachedContent()
            {
                Id = 1,
                Uri = _testUri.ToString(),
                Created = DateTimeOffset.UtcNow,
                Content = new byte[] { 0x00 },
                MediaType = "application/octet-stream"
            };
            _db = new Mock<IDapperWrapper>();
            _mapper = new Mock<IMapper>();


            _unit = new DatabaseCachedContentRepository(_db.Object, _mapper.Object);
        }

        [Test]
        public void GetShouldThrowExceptionWhenPassedANullUri()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _unit.Get(null as Uri));
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();

        }

        [Test]
        public async Task GetShouldReturnAnItemWhenPassedAValidUri()
        {
            // Arrange
            // The ORM connection returns an instance of CachedContent
            _db.Setup(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETCACHEDCONTENT, It.IsAny<object>())).ReturnsAsync(new List<CachedContent>() { _testCachedContent });
            // Mapped to CacheItem
            _mapper.Setup(x => x.Map<CachedContent, CacheItem>(It.IsAny<CachedContent>())).Returns(_testCacheItem);

            // Act
            var result = await _unit.Get(_testUri);

            // Assert
            // The ORM was called
            _db.Verify(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETCACHEDCONTENT, It.IsAny<object>()), Times.Once);
            // The Mapper was called
            _mapper.Verify(x => x.Map<CachedContent, CacheItem>(It.IsAny<CachedContent>()), Times.Once);
            // and we get it 
            Assert.AreSame(_testCacheItem, result);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();

        }

        [Test]
        public async Task GetShouldReturnANullWhenPassedAValidUriThatIsNotFound()
        {
            // Arrange
            // The ORM connection returns an instance of CachedContent
            _db.Setup(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETCACHEDCONTENT, It.IsAny<object>())).ReturnsAsync(new List<CachedContent>() { null as CachedContent });

            // Act
            var result = await _unit.Get(_testUri);

            // Assert
            // The ORM was called            
            _db.Verify(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETCACHEDCONTENT, It.IsAny<object>()), Times.Once);
            // The Mapper was called
            _mapper.Verify(x => x.Map<CachedContent, CacheItem>(It.IsAny<CachedContent>()), Times.Never);
            // and we get null
            Assert.IsNull(result);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();

        }

        [Test]
        public async Task GetAllShouldReturnACollectionOfItems()
        {

            var databaseItems = new List<CachedContent>() {
                new CachedContent() { Id = 1 , Uri = "https://service/api/v1/products/1"},
                new CachedContent() { Id = 2 , Uri = "https://service/api/v1/products/2"} };

            var cacheItems = new Dictionary<Uri, CacheItem>() {
                { new Uri(databaseItems.First().Uri) , new CacheItem() { Id = 1 } } ,
                { new Uri(databaseItems.Last().Uri),  new CacheItem() { Id = 2 } }
            };

            // The ORM connection returns many CachedContent
            _db.Setup(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETALLCACHEDCONTENT)).ReturnsAsync(databaseItems);
            _mapper.Setup(x => x.Map<IEnumerable<CachedContent>, Dictionary<Uri, CacheItem>>(It.IsAny<IEnumerable<CachedContent>>())).Returns(cacheItems);

            // Act
            var result = await _unit.GetAll();

            // Assert
            // The ORM was called            
            _db.Verify(x => x.QueryAsync<CachedContent>(CachedContentQueries.GETALLCACHEDCONTENT), Times.Once);
            // The Mapper was called
            _mapper.Verify(x => x.Map<IEnumerable<CachedContent>, Dictionary<Uri, CacheItem>>(It.IsAny<IEnumerable<CachedContent>>()), Times.Once);
            // and we get null
            CollectionAssert.IsNotEmpty(result);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();

        }

        [Test]
        public void RemoveShouldThrowAnExceptionWhenPassedANullUri()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _unit.Remove(null as Uri));
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RemoveShouldCallTheDatabaseWhenPassedAValidUri()
        {

            // Act
            await _unit.Remove(new Uri("https://service/api/v1/products/1"));

            // Assert
            _db.Verify(x => x.Execute(CachedContentQueries.DELETECACHEDCONTENT, It.IsAny<object[]>()), Times.Once);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RemoveAllShouldCallTheDatabaseOnce()
        {
            // Act
            await _unit.RemoveAll();

            // Assert
            _db.Verify(x => x.Execute(CachedContentQueries.DELETEALLCACHEDCONTENT), Times.Once);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public void SetThrowsExceptionWhenPassedAnInvalidUri()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _unit.Set(null as Uri, new CacheItem()));
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public void SetThrowsExceptionWhenPassedAnNullItem()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _unit.Set(_testUri, null as CacheItem));

            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetShouldCallTheDatabaseWhenPassedAValidUriAndItem()
        {

            _mapper.Setup(x => x.Map<CacheItem, CachedContent>(_testCacheItem)).Returns(_testCachedContent);

            await _unit.Set(_testUri, _testCacheItem);

            _mapper.Verify(x => x.Map<CacheItem, CachedContent>(It.IsAny<CacheItem>()), Times.Once);
            _db.Verify(x => x.Execute(CachedContentQueries.SETCACHEDCONTENT, It.IsAny<CachedContent>()), Times.Once);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }


        [Test]
        public void SetAllThrowsExceptionOnFindingAnNullItem()
        {

            var cacheItems = new List<CacheItem>() {
                new CacheItem() { Id = 1 },
                new CacheItem() { Id = 2 } };

            var cacheItemsDictionary = new Dictionary<Uri, CacheItem>() {
                { new Uri (  $"https://service/api/v1/products/" + $"{cacheItems.First().Id}") , cacheItems.First() } ,
                { new Uri (  $"https://service/api/v1/products/" + $"{cacheItems.Last().Id}") , null } ,
            };

            _mapper.Setup(x => x.Map<CacheItem, CachedContent>(It.IsAny<CacheItem>())).Returns(new CachedContent());

            Assert.ThrowsAsync<ArgumentNullException>(() => _unit.SetAll(cacheItemsDictionary));

            _mapper.Verify(x => x.Map<CacheItem, CachedContent>(It.IsAny<CacheItem>()), Times.Once);
            _db.Verify(x => x.Execute(CachedContentQueries.SETCACHEDCONTENT, It.IsAny<CachedContent>()), Times.Once);
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();
        }

        [Test]
        public async Task SetAllShouldCallTheDatabasOnceForEachItem()
        {


            var cacheItems = new List<CacheItem>() {
                new CacheItem() { Id = 1 },
                new CacheItem() { Id = 2 } };

            var cacheItemsDictionary = new Dictionary<Uri, CacheItem>() {
                { new Uri (  $"https://service/api/v1/products/" + $"{cacheItems.First().Id}") , cacheItems.First() } ,
                { new Uri (  $"https://service/api/v1/products/" + $"{cacheItems.Last().Id}") , cacheItems.Last() } ,
            };

            var cachedContentItems = new List<CachedContent>() {
                new CachedContent() { Id = cacheItemsDictionary.First().Value.Id, Uri = cacheItemsDictionary.First().Key.ToString() },
                new CachedContent() { Id = cacheItemsDictionary.Last().Value.Id, Uri = cacheItemsDictionary.First().Key.ToString() }
            };

            _mapper.Setup(x => x.Map<CacheItem, CachedContent>(It.IsAny<CacheItem>())).Returns(new CachedContent());

            await _unit.SetAll(cacheItemsDictionary);

            _mapper.Verify(x => x.Map<CacheItem, CachedContent>(It.IsAny<CacheItem>()), Times.Exactly(cacheItemsDictionary.Count));
            _db.Verify(x => x.Execute(CachedContentQueries.SETCACHEDCONTENT, It.IsAny<CachedContent>()), Times.Exactly(cacheItemsDictionary.Count));
            _db.VerifyNoOtherCalls();
            _mapper.VerifyNoOtherCalls();

        }
    }
}
