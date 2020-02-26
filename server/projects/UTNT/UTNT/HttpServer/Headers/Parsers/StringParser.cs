﻿using System;
using UTNT.HttpServer.Tools;
using UnityEngine.Scripting;

namespace UTNT.HttpServer.Headers.Parsers
{
    /// <summary>
    /// Parses <see cref="StringHeader"/>.
    /// </summary>
    [Preserve]
    public class StringParser : IHeaderParser
    {
        #region IHeaderParser Members

        /// <summary>
        /// Parse a header
        /// </summary>
        /// <param name="name">Name of header.</param>
        /// <param name="reader">Reader containing value.</param>
        /// <returns>HTTP Header</returns>
        /// <exception cref="FormatException">Header value is not of the expected format.</exception>
        public IHeader Parse(string name, ITextReader reader)
        {
            return new StringHeader(name, reader.ReadToEnd());
        }

        #endregion
    }
}