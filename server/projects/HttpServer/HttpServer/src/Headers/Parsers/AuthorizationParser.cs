﻿using HttpServer.Tools;
using UnityEngine.Scripting;

namespace HttpServer.Headers.Parsers
{
	[Preserve]
    [ParserFor(AuthorizationHeader.NAME)]
    internal class AuthorizationParser : IHeaderParser
    {
        #region IHeaderParser Members

        public IHeader Parse(string name, ITextReader reader)
        {
            var header = new AuthorizationHeader();
            reader.ConsumeWhiteSpaces();
            header.Scheme = reader.ReadWord();
            reader.ConsumeWhiteSpaces();
            header.Data = reader.ReadToEnd();
            return header;
        }

        #endregion
    }
}