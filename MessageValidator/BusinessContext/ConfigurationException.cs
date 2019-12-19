namespace MessageValidator.BusinessContext
{
    using System;

    /// <summary>
    /// The configuration exception.
    /// </summary>
    public class ConfigurationException : Exception
    {
        private static string defaultError = "Configuration Exception has occured. ";
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        public ConfigurationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public ConfigurationException(string message)
            : base(defaultError + message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="inner">
        /// The inner exception
        /// </param>
        public ConfigurationException(string message, Exception inner)
            : base(defaultError + message, inner)
        {
        }
    }
}
