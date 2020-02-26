using System.Net;
using HttpServer.Headers;
using HttpServer.Messages;

namespace HttpServer
{
    /// <summary>
    /// Response to a request.
    /// </summary>
    public interface IResponse : IMessage
    {
        /// <summary>
        /// Gets connection type.
        /// </summary>
        ConnectionHeader Connection { get; }

        /// <summary>
        /// Gets cookies.
        /// </summary>
        ResponseCookieCollection Cookies { get; }

        /// <summary>
        /// Gets HTTP version.
        /// </summary>
        /// <remarks>
        /// Default is HTTP/1.1
        /// </remarks>
        string HttpVersion { get; }

        /// <summary>
        /// Information about why a specific status code was used.
        /// </summary>
        string Reason { get; set; }

        /// <summary>
        /// Status code that is sent to the client.
        /// </summary>
        /// <remarks>Default is <see cref="HttpStatusCode.OK"/></remarks>
        HttpStatusCode Status { get; set; }

		///<summary>
		/// Gets or sets content type
		///</summary>
		ContentTypeHeader ContentType { get; set; }

        /// <summary>
        /// Redirect user.
        /// </summary>
        /// <param name="uri">Where to redirect to.</param>
        /// <remarks>
        /// Any modifications after a redirect will be ignored.
        /// </remarks>
        void Redirect(string uri);
    }
}