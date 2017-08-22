using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace BootCamp.Web.Handler
{
    public class MessageLoggingHandler : MessageHandler
    {
        private readonly ILogger logger;
        public MessageLoggingHandler()
        {
            logger = LogManager.GetCurrentClassLogger();
        }
        protected override async Task IncommingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                logger.Log(LogLevel.Info, string.Format("{0} - Request: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message)));
            });
                
        }
        protected override async Task OutgoingMessageAsync(string correlationId, string requestInfo, byte[] message)
        {
            await Task.Run(() =>
            {
                logger.Log(LogLevel.Info, string.Format("{0} - Response: {1}\r\n{2}", correlationId, requestInfo, Encoding.UTF8.GetString(message)));
            });
        }
    }
}
