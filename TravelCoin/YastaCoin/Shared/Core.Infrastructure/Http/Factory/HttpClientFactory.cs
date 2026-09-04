using System.Text;
using System.Text.Json;
using System.Net.Http.Json;

namespace Core.Infrastructure.Http.Factory
{
    public abstract class HttpClientFactory
    {
        public HttpClientFactory(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient(Name);
        }

        public abstract string Name { get; }

        public virtual JsonSerializerOptions CamelCaseSerialization { get; } = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        protected readonly HttpClient httpClient;

        protected async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            var result = await httpClient.GetAsync(url, cancellationToken);
            return await EvaluateResponse<T>(result);
        }

        protected async Task<T> PostAsync<T, U>(string url, U body, CancellationToken cancellationToken) where T : class where U : class
        {
                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(url, content, cancellationToken);
                return await EvaluateResponse<T>(result);
        }

        protected async Task<T> PostContentAsync<T, U>(string url,  HttpContent httpContent, CancellationToken cancellationToken) where T : class where U : HttpContent
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PostmanRuntime/7.39.0");
            var result = await httpClient.PostAsync(url, httpContent, cancellationToken);
            return await EvaluateResponse<T>(result);
        }

        protected async Task<T> PutAsync<T, U>(string url, U body, CancellationToken cancellationToken) where T : class where U : class
        {
            var result = await httpClient.PutAsJsonAsync(url, body, cancellationToken);
            return await EvaluateResponse<T>(result);
        }

        protected async Task<T?> DeleteAsync<T>(string url, CancellationToken cancellationToken) where T : class
        {
            var result = await httpClient.DeleteAsync(url, cancellationToken);
            return await EvaluateResponse<T>(result);
        }

        private async Task<T> EvaluateResponse<T>(HttpResponseMessage response) where T : class
        {
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, CamelCaseSerialization) ?? default!;
            }
            else
            {
                throw new HttpRequestException(response.ReasonPhrase);
            }
        }
    }
}
