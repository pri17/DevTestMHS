using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    using System.IO;

    /// <summary>
    /// The logger.
    /// </summary>
    public class Logger : ILogger
    {
        private static string location = @"C:\Users\zhou.jiawen\Documents\MHSProjLog";
        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// 
        public Logger()
        {
            if (!Directory.Exists(location))
            {
                Directory.CreateDirectory(location);
            }

            if (!File.Exists(location+@"C:\Exercise.log"))
            {
                File.CreateText(location+@"\Exercise.log").Dispose();
            }
        }

        /// <summary>
        /// Logs the message
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void Log(string message)
        {
            File.AppendAllText(location + @"\Exercise.log", string.Format("{0}: {1}\r\n", DateTime.Now, message));
        }
    }
}
