using System.Threading;
using System.Threading.Tasks;

namespace AsyncHttpUtils;

public interface IHttpService
{
    Task<ApiResponse<bool>> DeleteAsync(string url, CancellationToken cancellationToken = default);

    Task<ApiResponse<T>> GetAsync<T>(string url, CancellationToken cancellationToken = default);

    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default);
}
