using UTNT.HttpServer;
using UTNT.HttpServer.Routing;

namespace Wing.Tools.WebServer
{
    public class HandleRouter : IRouter
    {
        public delegate void Handler(IRequest request, IResponse response);

        Handler mHandler;
        private readonly string _fromUrl;
        private readonly bool _shouldRedirect;

        public HandleRouter(string fromUrl, Handler handler)
        {
            _fromUrl = fromUrl;
            mHandler = handler;
        }

        public string FromUrl
        {
            get { return _fromUrl; }
        }

        /// <summary>
        /// Process a request.Not on the main thread
        /// </summary>
        /// <param name="context">Request information</param>
        /// <returns>What to do next.</returns>
        public ProcessingResult Process(RequestContext context)
        {
            IRequest request = context.Request;
            IResponse response = context.Response;

            if (request.Uri.AbsolutePath == FromUrl)
            {
                if(mHandler != null)
                {
                   mHandler(request, response);
                }
                
                return ProcessingResult.SendResponse;
            }

            return ProcessingResult.Continue;
        }
    }
}
