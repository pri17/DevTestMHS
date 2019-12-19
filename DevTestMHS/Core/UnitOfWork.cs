namespace Core
{
    using System;
    using System.Diagnostics;

    using DomainMappings;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using NHibernate;

    /// <summary>
    /// The unit of work.
    /// </summary>
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        #region Constants:

        private const string ConnectionString =
            @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=DEVTESTMHS;Data Source=oceanomain";

        #endregion

        #region Fields:

        private static readonly ILogger Logger;
        private static readonly ISessionFactory SessionFactory;

        private readonly ISession session;
        private readonly ITransaction transaction;
        private bool isDisposed;

        #endregion

        #region Constructors:

        /// <summary>
        /// Initializes static members of the <see cref="UnitOfWork"/> class.
        /// </summary>
        static UnitOfWork()
        {
            Logger = new Logger();
            SessionFactory = CreateSessionFactory();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        public UnitOfWork()
        {
            this.session = SessionFactory.OpenSession();
            //this.session.CreateQuery("update statistics [dbo].[Message];").ExecuteUpdate();
            this.transaction = this.session.BeginTransaction();
        }

        #endregion

        #region Properties:

        /// <summary>
        /// Gets the session.
        /// </summary>
        public ISession Session
        {
            get
            {
                if (this.isDisposed || !this.session.IsOpen || !this.transaction.IsActive)
                {
                    throw new InvalidOperationException("Attempting to access an invalid unit-of-work.");
                }

                return this.session;
            }
        }

        #endregion

        #region Methods:

        /// <summary>
        /// Closes the unit of work.
        /// </summary>
        public void Close()
        {
            Logger.Log("BEGIN: UnitOfWork.Close()");

            // Validate state:
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("UnitOfWork");
            }
            
            Logger.Log("INFO: Closing unit-of-work.");

            // Commit transaction:
            if (this.transaction.IsActive)
            {
                Logger.Log("INFO: Committing db transaction.");
                Debug.Assert(this.session.IsOpen, "session.IsOpen");

                this.transaction.Commit();
                Debug.Assert(!this.transaction.IsActive, "!transaction.IsActive");
            }

            // Close session:
            if (this.session.IsOpen)
            {
                Logger.Log("INFO: Closing db session.");

                this.session.Close();
                Debug.Assert(!this.session.IsOpen, "!session.IsOpen");
            }
        }

        /// <summary>
        /// Disposes the unit of work.
        /// </summary>
        public void Dispose()
        {
            Logger.Log("BEGIN: UnitOfWork.Dispose()");
            
            // Validate state:
            if (this.isDisposed)
            {
                return;
            }

            Logger.Log("INFO: Disposing unit-of-work.");

            // Rollback transaction:
            if (this.transaction.IsActive)
            {
                Logger.Log("WARN: Disposing open db transaction.");
                Logger.Log("INFO: Rolling-back db transaction.");
                Trap(this.transaction.Rollback, e => Logger.Log("ERROR: Failed to rollback db transaction.\r\n" + e));
            }

            // Close session:
            if (this.session.IsOpen)
            {
                Logger.Log("WARN: Disposing open db session.");
                Trap(() => this.session.Close(), e => Logger.Log("ERROR: Failed to close db session.\r\n" + e));
            }

            // Dispose resources:
            Trap(this.transaction.Dispose, e => Logger.Log("ERROR: Failed to dispose db transaction.\r\n" + e));
            Trap(this.session.Dispose, e => Logger.Log("ERROR: Failed to dispose db session.\r\n" + e));
            this.isDisposed = true;
        }

        #endregion
        
        #region Helper Methods:

        /// <summary>
        /// Creates a session factory.
        /// </summary>
        /// <returns>The session factory.</returns>
        private static ISessionFactory CreateSessionFactory()
        {
            return Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2012.ConnectionString(ConnectionString))
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<NHSMessageMap>())
                .BuildSessionFactory();
        }

        /// <summary>
        /// Traps any exceptions raised within a specific action.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="handler">The handler.</param>
        private static void Trap(Action action, Action<Exception> handler)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                handler(e);
            }
        }

        #endregion
    }
}
