using Moq;
using NUnit.Framework;
using Cache.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Cache.Services;

namespace Cache.Tests
{
    /// <summary>
    /// Describes and validates the desired behaviour of the <see cref="CacheOrchestrator"/>.
    /// Test cases
    /// x Null Uri -> throws exception
    /// x cache item found -> returns cache item
    /// x No cache item, storage item found -> returns storage item + cache update
    /// x Expired cache item, storage item -> returns storage item + cache update
    /// x No cache item, no storage item, source item -> returns api item + storage and cache update
    /// x No cache item, no storage item, no source item -> returns null
    /// x Expired cache item, expired storage item, source item -> returns api item + storage and cache update
    /// x Expired cache item, expired storage item, no source item -> returns storage item + cache update
    /// 
    /// Test are DAMP - Descriptive and Meaningful Phrases.
    /// Code is DRY - Dont repeat yourself
    /// 
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CacheOrchestratorTests
    {
        private CacheOrchestrator _unit;

        private Mock<ICachingPolicy> _cachingPolicy;
        private Mock<ICacheService> _primaryCacheService;
        private Mock<ICacheStorageService> _secondaryCacheService;
        private Mock<ISourceItemService> _sourceItemService;
        private CacheItem _testCacheItem;
        private CacheItem _expiredTestCacheItem;
        private CacheItem _latestExpiredTestCacheItem;
        private Uri _testUri;

        [SetUp]
        public void Setup()
        {
            _testUri = new Uri("https://localhost/api/v1/products/1");
            
            _expiredTestCacheItem = new CacheItem() { Created = DateTimeOffset.MinValue, Content = new byte[] { 0x00, 0x00 } };
            _latestExpiredTestCacheItem = new CacheItem() { Created = DateTimeOffset.MinValue.Add(new TimeSpan(1,1,1,1)), Content = new byte[]{ 0x00, 0x03 } };
            _testCacheItem = new CacheItem() { Created = DateTimeOffset.UtcNow, Content = new byte[] { 0x00, 0x01 } };
            
            _primaryCacheService = new Mock<ICacheService>();
            _secondaryCacheService = new Mock<ICacheStorageService>();
            _sourceItemService = new Mock<ISourceItemService>();
            _cachingPolicy = new Mock<ICachingPolicy>();
            _cachingPolicy.SetupGet<TimeSpan>(x => x.ExpireAfter).Returns(new TimeSpan(23, 59, 59));
            _cachingPolicy.SetupGet<bool>(x => x.ReUseLatestExpired).Returns(true);

            _unit = new CacheOrchestrator(_primaryCacheService.Object, _secondaryCacheService.Object, _sourceItemService.Object, _cachingPolicy.Object);
        }

        /// <summary>
        /// Verify that no services calls were made other than already specified.
        /// </summary>
        private void VerifyNoOtherCallsMade()
        {
            _primaryCacheService.VerifyNoOtherCalls();
            _secondaryCacheService.VerifyNoOtherCalls();
            _sourceItemService.VerifyNoOtherCalls();
        }

        private void VerifyPrimaryCacheServiceCalledWith(Uri uri)
        {
            _primaryCacheService.Verify(x => x.Get(uri), Times.Once);
        }

        private void VerifySecondaryCacheServiceCalledWith(Uri uri)
        {
            _secondaryCacheService.Verify(x => x.Get(uri), Times.Once);
        }

        private void VerifySourceItemServiceCalledWith(Uri uri)
        {
            _sourceItemService.Verify(x => x.Get(uri), Times.Once);
        }

        private void VerifySecondaryCacheServiceUpdatedWith(Uri uri, CacheItem item)
        {
            if (item == null)
            {
                _secondaryCacheService.Verify(x => x.Set(uri, It.IsAny<CacheItem>()), Times.Once);
            }
            else
            {
                _secondaryCacheService.Verify(x => x.Set(uri, item), Times.Once);
            }
        }

        private void VerifyPrimaryCacheServiceUpdatedWith(Uri uri, CacheItem item)
        {
            if (item == null)
            {
                _primaryCacheService.Verify(x => x.Set(uri, It.IsAny<CacheItem>()), Times.Once);
            }
            else
            {
                _primaryCacheService.Verify(x => x.Set(uri, item), Times.Once);
            }
        }

        [Test]
        public void Unit_Setup_NotNull()
        {
            Assert.IsNotNull(_unit);
        }

        [Test]
        public void Handle_WithNullResourceIdentifier_ThrowsCorrectException()
        {
            // AAA
            Assert.ThrowsAsync<ArgumentNullException>(async () => { var response = await _unit.Handle(null as Uri); });
        }

        [Test]
        public async Task Handle_AllEmpty_ReturnsNullResponse()
        {
            // Arrange
            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);

            // Act
            var response = await _unit.Handle(_testUri);

            // Assert
            Assert.IsNull(response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);
            VerifyNoOtherCallsMade();
        }

        [Test]
        public async Task Handle_NonCachedButSourceAvailable_ReturnsContentAndUpdatesCaches()
        {
            // Arrange
            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            // Act
            var response = await _unit.Handle(_testUri);

            // Arrange
            Assert.IsNotNull(response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);
            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifySecondaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifyNoOtherCallsMade();

        }

        [Test]
        public async Task Handle_PrimaryCacheHit_ReturnsResponse()
        {
            // Arrange
            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(_testCacheItem.Content, response);

            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifyNoOtherCallsMade();
        }


        [Test]
        public async Task Handle_NullPrimarySecondaryCacheHit_ReturnsResponseAndUpdatesPrimaryCache()
        {
            // Arrange

            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(_testCacheItem.Content, response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifyNoOtherCallsMade();
        }

        [Test]
        public async Task Handle_ExpiredPrimarySecondaryCacheHit_ReturnsResponseUpdatesPrimaryCache()
        {
            // Arrange
            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(_testCacheItem.Content, response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifyNoOtherCallsMade();
        }

        [Test]
        public async Task Handle_OnlySourceHit_ReturnsResponseAndUpdatesPrimaryAndSecondaryCaches()
        {
            // Arrange

            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(_testCacheItem.Content, response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);
            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifySecondaryCacheServiceUpdatedWith(_testUri, _testCacheItem);

            VerifyNoOtherCallsMade();
        }


        [Test]
        public async Task Handle_AllExpiredHitsSource_ReturnsSourceResponseUpdatesPrimaryAndSecondaryCaches()
        {
            // Arrange

            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(_testCacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // 
            Assert.IsNotNull(response);
            Assert.AreEqual(_testCacheItem.Content, response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceUpdatedWith(_testUri, _testCacheItem);
            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _testCacheItem);

            VerifyNoOtherCallsMade();
        }

        [Test]
        public async Task Handle_WithReUseExpiredAllExpiredNoSource_ReturnsLatestResponseUpdatesCaches()
        {
            // Arrange

            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // 
            Assert.IsNotNull(response);

            Assert.AreEqual(_expiredTestCacheItem.Content, response);

            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);

            VerifyPrimaryCacheServiceUpdatedWith(_testUri, _expiredTestCacheItem);
            VerifySecondaryCacheServiceUpdatedWith(_testUri, _expiredTestCacheItem);

            VerifyNoOtherCallsMade();
        }

        [Test]
        public async Task Handle_WithReUseExpiredAllExpiredSecondaryIsLatestNoSource_ReturnsLatestResponseUpdatesCaches()
        {
            // Arrange
            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_latestExpiredTestCacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);

            //Act
            var response = await _unit.Handle(_testUri);

            // 
            Assert.IsNotNull(response);

            Assert.AreEqual(_latestExpiredTestCacheItem.Content, response);

            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);

            VerifyPrimaryCacheServiceUpdatedWith(_testUri, null);
            VerifySecondaryCacheServiceUpdatedWith(_testUri, null);

            VerifyNoOtherCallsMade();
        }


        [Test]
        public async Task Handle_WithNotReUseExpiredAllExpiredNoSource_ReturnsNullResponseNoCacheUpdates()
        {
            // Arrange

            _primaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(_expiredTestCacheItem);
            _secondaryCacheService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _sourceItemService.Setup(x => x.Get(_testUri)).ReturnsAsync(null as CacheItem);
            _cachingPolicy.SetupGet<bool>(x => x.ReUseLatestExpired).Returns(false);

            //Act
            var response = await _unit.Handle(_testUri);

            // 
            Assert.IsNull(response);
            VerifyPrimaryCacheServiceCalledWith(_testUri);
            VerifySecondaryCacheServiceCalledWith(_testUri);
            VerifySourceItemServiceCalledWith(_testUri);
            VerifyNoOtherCallsMade();
        }
    }
}