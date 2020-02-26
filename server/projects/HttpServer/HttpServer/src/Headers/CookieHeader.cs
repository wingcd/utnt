﻿using System;
using HttpServer.Messages;

namespace HttpServer.Headers
{
    /// <summary>
    /// Contents of a cookie header.
    /// </summary>
    public class CookieHeader : IHeader
    {
        public const string NAME = "Cookie";

        /// <summary>
        /// Initializes a new instance of the <see cref="CookieHeader"/> class.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <exception cref="ArgumentNullException"><c>collection</c> is <c>null</c>.</exception>
        public CookieHeader(RequestCookieCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            Cookies = collection;
        }

        /// <summary>
        /// Gets cookie collection
        /// </summary>
        public RequestCookieCollection Cookies { get; private set; }

        #region IHeader Members

        /// <summary>
        /// Gets header name
        /// </summary>
        public string Name
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets value as it would be sent back to client.
        /// </summary>
        /// <value></value>
        public string HeaderValue
        {
            get { return Cookies.ToString(); }
        }

        #endregion
    }
}