using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace GameTogetherAPI.Test.Old.Drivers
{
    /// <summary>
    /// Provides an abstraction for sending HTTP requests to an API.
    /// </summary>
    internal class APIDriver
    {
        /// <summary>
        /// The HttpClient instance used to send HTTP requests.
        /// </summary>
        private HttpClient Client;

        private Dictionary<string,string> authTokenDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="APIDriver"/> class with the specified HttpClient.
        /// </summary>
        /// <param name="client">The HttpClient instance used to send requests.</param>
        public APIDriver(HttpClient client)
        {
            Client = client;
            if (client==null || client.BaseAddress == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
        }
        
        private string BuildUrl(string endpoint, Dictionary<string,string> urlParams)
        {
            string[] parameters;
            if (urlParams != null && urlParams.Count > 0)
            {
                foreach (var urlParam in urlParams)
                {
                    string buildstring =  "{" + urlParam.Key.ToString()+ "}"; 
                    endpoint = endpoint.Replace(buildstring, urlParam.Value.ToString());
                }
            }
            return endpoint;
        }
        /// <summary>
        /// Sends an HTTP GET request to the specified API endpoint with optional parameters.
        /// </summary>
        /// <param name="endpoint">The endpoint method name to send the request to.</param>
        /// <param name="parameters">Optional parameters to be included in the request URL.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        public async Task<HttpResponseMessage> SendGetRequest(string endpoint,  Dictionary<string,string> parameters, bool authenticate )
        {
            endpoint = BuildUrl(endpoint, parameters);
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            var result = await Client.SendAsync(request);
            return result;
        }
        public async Task<HttpResponseMessage> SendPostRequest(string endpoint, object[] content, Dictionary<string,string> parameters, bool authenticate )
        {
            endpoint = BuildUrl(endpoint, parameters);
            var json = JsonConvert.SerializeObject(content);
            var data = new StringContent(json, Encoding.UTF8, "application/json");
            var result = await Client.PostAsync(endpoint, data);
            return result;
        }
            
        public async Task<HttpResponseMessage> SendDeleteRequest(string endpoint, Dictionary<string, string> parameters, bool authenticate )
        {
            endpoint = BuildUrl(endpoint, parameters);
            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Add(authTokenDictionary.Keys.ToString(), authTokenDictionary.Values.ToString());
            var result = await Client.SendAsync(request);
            return result;
        }

        public void SetAuthToken(string authToken)
        {
            authTokenDictionary = new Dictionary<string, string>
                { { "Authorization", "Bearer " + authToken } };
        }
        
        /// <summary>
        /// Sends an HTTP request to the specified API endpoint with optional query parameters, content, 
        /// and authorization header, using the provided HTTP method.
        /// </summary>
        /// <param name="endpoint">The base API endpoint to send the request to.</param>
        /// <param name="parameters">
        /// A dictionary of query parameters to append to the endpoint URL. 
        /// Keys and values will be URL-encoded. Pass <c>null</c> if no parameters are needed.
        /// </param>
        /// <param name="content">
        /// An array of objects to serialize as JSON and include in the request body.
        /// Pass <c>null</c> if no body is required.
        /// </param>
        /// <param name="authenticate">
        /// A boolean indicating whether to include the authorization header. 
        /// If <c>true</c>, the method will add a bearer token from <c>authTokenDictionary</c>.
        /// </param>
        /// <param name="method">The HTTP method to use (e.g., <see cref="HttpMethod.Get"/>, <see cref="HttpMethod.Post"/>).</param>
        /// <returns>
        /// A task representing the asynchronous operation. The result contains the <see cref="HttpResponseMessage"/> 
        /// returned by the server.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="endpoint"/> is null or if authorization is enabled but <c>authTokenDictionary</c> is not set properly.
        /// </exception>

        public async Task<HttpResponseMessage> SendRequest(HttpMethod method, string endpoint, bool authenticate, 
            Dictionary<string, string>? parameters = null, object[]? content = null)
        { 
            endpoint = BuildUrl(endpoint, parameters);
            var request = new HttpRequestMessage(method, endpoint);
            if (authenticate && authTokenDictionary != null && authTokenDictionary.Count > 0)
            {
                foreach (var authToken in authTokenDictionary)
                {
                    request.Headers.Add(authToken.Key, authToken.Value);
                }
            }
            if (content != null)
            {
                string json = JsonConvert.SerializeObject(content);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                request.Content = data;
            }
            return await Client.SendAsync(request);
        }
    }
}
