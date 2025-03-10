using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTogetherAPI.Test.Drivers
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

        /// <summary>
        /// Initializes a new instance of the <see cref="APIDriver"/> class with the specified HttpClient.
        /// </summary>
        /// <param name="client">The HttpClient instance used to send requests.</param>
        public APIDriver(HttpClient client)
        {
            Client = client;
        }

        /// <summary>
        /// Sends an HTTP GET request to the specified API endpoint with optional parameters.
        /// </summary>
        /// <param name="endPointMethodName">The endpoint method name to send the request to.</param>
        /// <param name="parameters">Optional parameters to be included in the request URL.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
        public async Task<HttpResponseMessage> SendRequest(string endPointMethodName, params object[] parameters)
        {
            if (parameters != null && parameters.Length > 0)
            {
                var queryString = string.Join("/", parameters.Select(p => Uri.EscapeDataString(p?.ToString())));
                endPointMethodName = $"{endPointMethodName}/{queryString}";
            }

            var result = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Get, endPointMethodName));
            return result;
        }
    }
}
