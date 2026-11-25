using System;
using System.Collections.Generic;
using System.Text;

namespace WebHook.Application.Interfaces
{
    public interface IWebhookSender
    {
        Task<(bool IsSuccess, int? StatusCode, string? ErrorMessage)> SendAsync(
    string endpointUrl,
    string eventType,
    string payload,
    string signature,
    CancellationToken cancellationToken = default);
    }
}
