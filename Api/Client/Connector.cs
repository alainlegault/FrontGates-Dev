using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FrontGatesDev.Api.Client
{
	public interface IConnector
	{
		Task<T> GetValueAsync<T>(string route, Dictionary<string, object> headers);
		Task<TResponse> PostValueAsync<TRequest, TResponse>(string route, TRequest requestObject, Dictionary<string, object> headers = null);
		Task<T> PutValueAsync<T>(string route, T requestObject, Dictionary<string, object> headers = null);
		Task<TResponse> DeleteValueAsync<TResponse>(string route, Dictionary<string, object> headers = null);
		Action<string, Exception> OnHttpRequestException { get; set; }
	}

	internal class Connector : IConnector
	{
		private string EndPointUrl { get; set; }

		public string AuthorizationHeader { get; set; }

		public Connector(string endPointUrl, string authorizationHeader = null)
		{
			EndPointUrl = endPointUrl;
			AuthorizationHeader = authorizationHeader;
		}

		public async Task<T> GetValueAsync<T>(string route, Dictionary<string, object> headers)
		{
			T response;

			using (var client = GetHttpClient(headers))
				response = await ExecuteResponse<T>(client, new HttpClientRequest { Method = ExecuteResponseType.Get, Route = route });

			return response;
		}

		public async Task<TResponse> PostValueAsync<TRequest, TResponse>(string route, TRequest requestObject, Dictionary<string, object> headers = null)
		{
			TResponse response;

			using (var client = GetHttpClient(headers))
				response = await ExecuteResponse<TRequest, TResponse>(client, new HttpClientRequest<TRequest> { Method = ExecuteResponseType.Post, Route = route, Object = requestObject });

			return response;
		}

		public async Task<T> PutValueAsync<T>(string route, T requestObject, Dictionary<string, object> headers = null)
		{
			T response;

			using (var client = GetHttpClient(headers))
				response = await ExecuteResponse<T>(client, new HttpClientRequest<T> { Method = ExecuteResponseType.Put, Route = route, Object = requestObject });

			return response;
		}

		public async Task<TResponse> DeleteValueAsync<TResponse>(string route, Dictionary<string, object> headers = null)
		{
			TResponse response;

			using (var client = GetHttpClient(headers))
				response = await ExecuteResponse<TResponse>(client, new HttpClientRequest { Method = ExecuteResponseType.Delete, Route = route });

			return response;
		}

		private async Task<TResponse> ExecuteResponse<TResponse>(HttpClient client, HttpClientRequest request)
		{
			ValidateArguments(client, request, request.Route);

			HttpResponseMessage response = null;

			try
			{
				switch (request.Method)
				{
					default:
					case ExecuteResponseType.Get:
						response = await client.GetAsync(request.Route);
						break;
					case ExecuteResponseType.Delete:
						response = await client.DeleteAsync(request.Route);
						break;
				}
			}
			catch (HttpRequestException ex)
			{
				WebExceptionHandling(ex);
			}

			await ValidateResponseStatusCode(response);

			return await GetResponseObject<TResponse>(response);
		}

		private async Task<TResponse> ExecuteResponse<TRequest, TResponse>(HttpClient client, HttpClientRequest<TRequest> request)
		{
			ValidateArguments(client, request, request.Route);

			HttpResponseMessage response = null;

			try
			{
				switch (request.Method)
				{
					default:
					case ExecuteResponseType.Post:
						response = await client.PostAsJsonAsync(request.Route, request.Object);
						break;
					case ExecuteResponseType.Put:
						response = await client.PutAsJsonAsync(request.Route, request.Object);
						break;
				}
			}
			catch (HttpRequestException ex)
			{
				WebExceptionHandling(ex);
			}

			await ValidateResponseStatusCode(response);

			return await GetResponseObject<TResponse>(response);
		}

		private HttpClient GetHttpClient(Dictionary<string, object> headers = null)
		{
			if (string.IsNullOrEmpty(EndPointUrl))
				throw new NullReferenceException(nameof(EndPointUrl));

			var client = new HttpClient();

			if (!string.IsNullOrEmpty(AuthorizationHeader))
				client.DefaultRequestHeaders.Add("Authorization", AuthorizationHeader);

			if (headers != null)
				foreach (var h in headers.Where(o => o.Value != null && !string.IsNullOrEmpty(o.Value.ToString())))
					client.DefaultRequestHeaders.Add(h.Key, h.Value.ToString());

			client.BaseAddress = new Uri(EndPointUrl);

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
		}

		private void ValidateArguments(params object[] args)
		{
			foreach (var arg in args)
				if (arg == null)
					throw new ArgumentNullException(nameof(arg));
		}

		private async Task ValidateResponseStatusCode(HttpResponseMessage response)
		{
			if (!response?.IsSuccessStatusCode ?? true)
			{
				var error = await response.Content.ReadAsStringAsync();

				throw new Exception(error);
			}
		}

		private async Task<TResponse> GetResponseObject<TResponse>(HttpResponseMessage response)
		{
			var content = await response.Content.ReadAsStringAsync();

			return JsonConvert.DeserializeObject<TResponse>(content);
		}

		private static void WebExceptionHandling(HttpRequestException ex)
		{
			//for now, we're just interested in no connection to the endpoint error handling
			if (ex.InnerException is WebException b)
				if (b.Status == WebExceptionStatus.NameResolutionFailure || b.Status == WebExceptionStatus.SendFailure)
					throw ex;
		}

		public Action<string, Exception> OnHttpRequestException { get; set; }
	}
}
