using System.Net.Http.Headers;
using System.Text.Json;
using System.Web;

namespace WBImport
{
    public static class WBClient
    {
        #region Methods

        public static async Task<T> GetAsync<T>(string path, Dictionary<string, string> query = null, object body = null)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(path);

            var accessToken = Settings.Default?.Wildberries?.AccessToken;
            if (string.IsNullOrEmpty(accessToken))
                throw new InvalidOperationException("Wildberries access token was empty.");

            if (query?.Count > 0)
            {
                var parsedQuery = HttpUtility.ParseQueryString(string.Empty);

                foreach (var keyValuePair in query)
                    parsedQuery[keyValuePair.Key] = keyValuePair.Value;

                path += $"?{parsedQuery}";
            }

            if (!Uri.TryCreate(path, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"Cannot create URI from \"{path}\".");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);

            request.Headers.Add("Authorization", accessToken);
            request.Headers.Add("Accept", "application/json");

            if (body != null)
            {
                request.Content = new StreamContent(
                    new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(body))
                );
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            using var response = await Defaults.HttpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        #endregion Methods
    }
}