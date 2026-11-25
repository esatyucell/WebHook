using System;
using System.Security.Cryptography;
using System.Text;
using WebHook.Application.Interfaces;

namespace WebHook.Infrastructure.Security
{
    public class HmacSignatureGenerator : IHmacSignatureGenerator
    {
        public string GenerateSignature(string payload, string secretKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(payload);
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            byte[] hashBytes = HMACSHA256.HashData(keyBytes, payloadBytes);
            string hexHash = Convert.ToHexStringLower(hashBytes);

            return $"sha256={hexHash}";
        }
    }
}