using System;
using UTNT.HttpServer.Tools;
using UnityEngine.Scripting;

namespace UTNT.HttpServer.Headers.Parsers
{
    /// <summary>
    /// Parses numerical values
    /// </summary>
    [Preserve]
    [ParserFor("Content-Length")]
    public class NumericHeaderParser : IHeaderParser
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
            string temp = reader.ReadToEnd();
            int value;
            if (!int.TryParse(temp, out value))
                throw new FormatException("Header '" + name + "' do not contain a numerical value ('" + temp + "').");
            return new NumericHeader(name, value);
        }

        #endregion
    }
}