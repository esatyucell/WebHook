using System;
using System.Collections.Generic;
using System.Text;
using WebHook.Application.Interfaces;

namespace WebHook.Infrastructure.Http
{
    public class HttpWebhookSender(HttpClient httpClient) : IWebhookSender
    {
        public async Task<(bool IsSuccess, int? StatusCode, string? ErrorMessage)> SendAsync(string endpointUrl, string eventType, string payload, string signature, CancellationToken cancellationToken = default)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");
                request.Headers.Add("X-Webhook-Signature", signature);
                request.Headers.Add("X-Webhook-Event", eventType);
                request.Headers.Add("User-Agent", "WebHookEngine/1.0");

                using var response = await httpClient.SendAsync(request, cancellationToken);
                var statusCode = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    return (true, statusCode, null);
                }

                return (false, statusCode, $"HTTP {statusCode}: {response.ReasonPhrase}");
            }
            catch (TaskCanceledException) {
                return (false, null, "İstek zaman aşımına uğradı (Timeout).");
            }catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }
}
