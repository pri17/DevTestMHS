namespace Core
{
    using NHibernate;

    /// <summary>
    /// The unit of work interface.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Gets the session.
        /// </summary>
        ISession Session { get; }
    }
}
