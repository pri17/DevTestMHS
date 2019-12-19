namespace Core
{
    /// <summary>
    /// The Logger interface.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log the message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Log(string message);
    }
}
