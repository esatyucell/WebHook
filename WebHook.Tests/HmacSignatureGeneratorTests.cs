using System;
using WebHook.Infrastructure.Security;
using Xunit;

namespace WebHook.Tests
{
    public class HmacSignatureGeneratorTests
    {
        private readonly HmacSignatureGenerator _generator = new();

        [Fact]
        public void GenerateSignature_ValidInputs_ReturnsExpectedSha256Format()
        {
            // Arrange (Hazırlık)
            var payload = "{\"orderId\":2048,\"amount\":1490.5}";
            var secretKey = "whsec_test_secret_key";

            // Act (Çalıştırma)
            var signature1 = _generator.GenerateSignature(payload, secretKey);
            var signature2 = _generator.GenerateSignature(payload, secretKey);

            // Assert (Doğrulama)
            Assert.StartsWith("sha256=", signature1);
            Assert.Equal(71, signature1.Length); // "sha256=" (7) + 64 karakter hex
            Assert.Equal(signature1, signature2); // Aynı veri her zaman aynı mührü üretmeli
        }

        [Fact]
        public void GenerateSignature_ModifiedPayload_ProducesDifferentSignature()
        {
            var secretKey = "whsec_test_secret_key";
            var originalPayload = "{\"amount\":100}";
            var tamperedPayload = "{\"amount\":9999}";

            var originalSig = _generator.GenerateSignature(originalPayload, secretKey);
            var tamperedSig = _generator.GenerateSignature(tamperedPayload, secretKey);

            Assert.NotEqual(originalSig, tamperedSig);
        }

        [Theory]
        [InlineData("", "whsec_key")]
        [InlineData("   ", "whsec_key")]
        [InlineData("{\"id\":1}", "")]
        [InlineData("{\"id\":1}", "   ")]
        public void GenerateSignature_EmptyOrWhitespaceInput_ThrowsArgumentException(string payload, string secretKey)
        {
            Assert.Throws<ArgumentException>(() => _generator.GenerateSignature(payload, secretKey));
        }
    }
}