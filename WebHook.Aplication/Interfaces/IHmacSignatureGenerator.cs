using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.Interfaces
{
    public interface IHmacSignatureGenerator
    {
        string GenerateSignature(string payload, string secretKey);
    }
}
