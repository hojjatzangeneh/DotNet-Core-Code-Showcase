
using Microsoft.Extensions.Logging;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;

using System.Threading.Tasks;

namespace AsyncHttpUtils;

public class HttpService : IHttpService
{
    readonly HttpClient _httpClient;
    readonly ILogger<HttpService>? _logger;

    public HttpService(HttpClient httpClient, ILogger<HttpService>? logger = null)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> DeleteAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(url, cancellationToken).ConfigureAwait(false);
            return ApiResponse<bool>.Success(response.IsSuccessStatusCode);
        } catch(Exception ex)
        {
            _logger?.LogError(ex, "DELETE {Url} failed", url);
            return ApiResponse<bool>.Failure(ex.Message);
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(8));

            HttpResponseMessage response = await _httpClient.GetAsync(url, cts.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            T? result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: cts.Token).ConfigureAwait(false);
            return ApiResponse<T>.Success(result!);
        } catch(OperationCanceledException oce)
        {
            _logger?.LogWarning(oce, "Request to {Url} timed out", url);
            return ApiResponse<T>.Failure("Timeout");
        } catch(Exception ex)
        {
            _logger?.LogError(ex, "GET {Url} failed", url);
            return ApiResponse<T>.Failure(ex.Message);
        }
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, data, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            TResponse? result = await response.Content
                .ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return ApiResponse<TResponse>.Success(result!);
        } catch(Exception ex)
        {
            _logger?.LogError(ex, "POST {Url} failed", url);
            return ApiResponse<TResponse>.Failure(ex.Message);
        }
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PutAsJsonAsync(url, data, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            TResponse? result = await response.Content
                .ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return ApiResponse<TResponse>.Success(result!);
        } catch(Exception ex)
        {
            _logger?.LogError(ex, "PUT {Url} failed", url);
            return ApiResponse<TResponse>.Failure(ex.Message);
        }
    }
}
