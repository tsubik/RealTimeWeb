using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RealTimeWeb.PureSSE.Code;

namespace RealTimeWeb.PureSSE.Controllers
{
    public class SSEController : ApiController
    {
		public HttpResponseMessage Get(HttpRequestMessage request)
		{
			//lazy object first using starting app
			StockTicker _ticker = StockTicker.Instance;
			HttpResponseMessage response = request.CreateResponse();
			response.Headers.Add("Cache-Control", "no-cache, must-revalidate");
			response.Headers.Add("Connection", "keep-alive");
			response.Content = new PushStreamContent(OnStreamAvailable, "text/event-stream");

			return response;
		}
		/// <summary>
		/// Registers the connection as a client to <see cref="MvcApplication.Pub"/>
		/// </summary>
		/// <param name="stream">The stream from which the StreamWriter will be created</param>
		/// <param name="content"></param>
		/// <param name="context"></param>
		/// <remarks>This is a callback for <see cref="PushStreamContent"/></remarks>
		private void OnStreamAvailable(Stream stream, HttpContent content, TransportContext context)
		{
			StreamWriter streamWriter = new StreamWriter(stream);
			StockTicker.Instance.Clients.TryAdd(streamWriter, streamWriter);
		}
    }
}
