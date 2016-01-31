using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace rgLogger {
    /// <summary>
    /// Writes log entries to a table in a SQL database.
    /// </summary>
    public class DbLogger : BaseLogger {
        public string Table { get; set; } = "SysLog";
        public string DateColumn { get; set; } = "log_date_recorded";
        public string MessageColumn { get; set; } = "log_message";
        public string MessageLevelColumn { get; set; } = "log_level";

        private DbConnection db;
        private DbProviderFactory dbfactory;

        /// <summary>
        /// Creates a new DbLogger object.
        /// </summary>
        /// <param name="NewDbConnection">The System.Data.Common.DbConnection derived object for the connection to the logging database.</param>
        public DbLogger(DbConnection MyDatabase) {
            dbfactory = DbProviderFactories.GetFactory(MyDatabase);
            db = MyDatabase;

            db.Open();
        }

        public DbLogger(DbConnection NewDbConnection, LogLevel messageLevel)
            : this(NewDbConnection) {
                Level = messageLevel;
        }

        public override void Write(string message, LogLevel messageLevel) {
            if (sendMessage(messageLevel)) {
                DbCommand dbCmd = db.CreateCommand();
                var cmd = $"INSERT INTO [dbo].[{ Table }] ([{ MessageColumn }], [{ MessageLevelColumn }], [{ DateColumn }]) VALUES (@logmessage, @loglevel, @logdate);";

                var sqlcmd = dbfactory.CreateCommandBuilder();
                sqlcmd.GetInsertCommand();
            }
        }

        internal override void WriteToLog(string message) {
            throw new NotImplementedException();
        }

        public override void Dispose() {
            base.Dispose();
            db.Dispose();
        }
    }
}
