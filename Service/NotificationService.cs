using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using RentingBooking.Response;
using RentingBooking.Service.Interfaces;

namespace RentingBooking.Service;

public class NotificationService : INotificationService
{
    private readonly HttpClient _httpClient;

    public NotificationService(HttpClient httpClient, IOptions<N8nSettings> settings)
    {
        _httpClient = httpClient;
        var baseUrl = settings.Value.BaseUrl?.TrimEnd('/');

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("N8nSettings:BaseUrl is not configured.");

        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<ServiceResponse<bool>> SendNotificationAsync(NotificationRequest request)
    {
        var response = new ServiceResponse<bool>
        {
            Data = false
        };

        try
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync("webhook/renting-notifications", request);

            if (httpResponse.IsSuccessStatusCode)
            {
                response.Success = true;
                response.Data = true;
                response.Message = "Notification dispatched to n8n.";
                return response;
            }

            var failureBody = await httpResponse.Content.ReadAsStringAsync();
            response.Success = false;
            response.Message = string.IsNullOrWhiteSpace(failureBody)
                ? $"n8n request failed with status code {httpResponse.StatusCode}."
                : failureBody;

            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = ex.Message;
            return response;
        }
    }
}
