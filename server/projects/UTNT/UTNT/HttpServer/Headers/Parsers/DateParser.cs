﻿using System;
using System.Globalization;
using UTNT.HttpServer.Tools;
using UnityEngine.Scripting;

namespace UTNT.HttpServer.Headers.Parsers
{
    /// <summary>
    /// Parses "Date" header.
    /// </summary>
    [Preserve]
    [ParserFor(DateHeader.NAME)]
    [ParserFor("If-Modified-Since")]
    internal class DateParser : IHeaderParser
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
            string value = reader.ReadToEnd();

            try
            {
                return new DateHeader(name, DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal));
            }
            catch (FormatException err)
            {
                throw new BadRequestException("'" + name + "' do not contain a valid date", err);
            }
        }

        #endregion
    }
}