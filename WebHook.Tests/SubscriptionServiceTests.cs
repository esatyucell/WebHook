using System;
using System.Threading.Tasks;
using Moq;
using WebHook.Application.DTOs;
using WebHook.Application.Services;
using WebHook.Domain.Entities;
using WebHook.Domain.Interfaces;
using Xunit;

namespace WebHook.Tests
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<ISubscriptionRepository> _mockRepo;
        private readonly SubscriptionService _service;

        public SubscriptionServiceTests()
        {
            _mockRepo = new Mock<ISubscriptionRepository>();
            _service = new SubscriptionService(_mockRepo.Object);
        }

        [Fact]
        public async Task CreateAsync_ValidDto_CreatesActiveSubscriptionWithSecretKey()
        {
            // Arrange
            var dto = new SubscriptionCreateDto
            {
                Name = "Sipariş Servisi",
                EndpointUrl = "https://example.com/webhook",
                EventType = "order.created"
            };

            _mockRepo
                .Setup(r => r.AddAsync(It.IsAny<Subscription>()))
                .ReturnsAsync((Subscription s) => s);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.EndpointUrl, result.EndpointUrl);
            Assert.True(result.Status);
            Assert.StartsWith("whsec_", result.SecretKey);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Subscription>()), Times.Once);
        }

        [Theory]
        [InlineData("not-a-valid-url")]
        [InlineData("ftp://example.com/webhook")]
        [InlineData("")]
        public async Task CreateAsync_InvalidUrl_ThrowsArgumentException(string invalidUrl)
        {
            var dto = new SubscriptionCreateDto
            {
                Name = "Hatalı Abonelik",
                EndpointUrl = invalidUrl,
                EventType = "order.created"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto));
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Subscription>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ExistingActiveSubscription_SetsStatusFalseAndReturnsTrue()
        {
            // Arrange
            var subId = Guid.NewGuid();
            var existingSub = new Subscription
            {
                Id = subId,
                Name = "Test",
                EndpointUrl = "https://example.com/hook",
                EventType = "order.created",
                Status = true
            };

            _mockRepo.Setup(r => r.GetByIdAsync(subId)).ReturnsAsync(existingSub);
            _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Subscription>())).ReturnsAsync((Subscription s) => s);

            // Act
            var deleted = await _service.DeleteAsync(subId);

            // Assert
            Assert.True(deleted);
            Assert.False(existingSub.Status);
            _mockRepo.Verify(r => r.UpdateAsync(existingSub), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NonExistentSubscription_ReturnsFalse()
        {
            var unknownId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByIdAsync(unknownId)).ReturnsAsync((Subscription)null);

            var deleted = await _service.DeleteAsync(unknownId);

            Assert.False(deleted);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Subscription>()), Times.Never);
        }
    }
}