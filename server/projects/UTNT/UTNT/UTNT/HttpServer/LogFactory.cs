using UTNT.HttpServer.Logging;
using System;
using UTNT.HttpServer.Logging;

namespace Wing.Tools.WebServer
{
    public class Log : ILogger
    {
        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Debug(string message, Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Error(string message, Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public void Fatal(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        public void Fatal(string message, Exception exception)
        {
			UnityEngine.Debug.LogException(exception);
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Info(string message, Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public void Trace(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Trace(string message, Exception exception)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public void Warning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Warning(string message, Exception exception)
        {
			UnityEngine.Debug.LogException(exception);
        }
    }

    public class LogFactory : ILogFactory
    {
        public ILogger CreateLogger(Type type)
        {
            return new Log();
        }
    }
}
