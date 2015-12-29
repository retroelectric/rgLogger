using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace rgLogger {
    public class DbLogger : BaseLogger {
        private string _table = "SysLog";

        public string Table {
            get { return _table; }
            set { _table = value; }
        }

        private string _dateColumn = "dT";

        public string DateColumn {
            get { return _dateColumn; }
            set { _dateColumn = value; }
        }

        private string _messageColumn = "logMessage";

        public string MessageColumn {
            get { return _messageColumn; }
            set { _messageColumn = value; }
        }

        private string _messageLevelColumn = "logLevel";

        public string MessageLevelColumn {
            get { return _messageLevelColumn; }
            set { _messageLevelColumn = value; }
        }

        private DbConnection db;

        public DbLogger(DbConnection NewDbConnection) {
            db = NewDbConnection;
        }

        public DbLogger(DbConnection NewDbConnection, logLevel messageLevel)
            : this(NewDbConnection) {
                level = messageLevel;
        }

        public override void Write(string message, logLevel messageLevel) {
            if (sendMessage(messageLevel)) {
                DbCommand dbCmd = db.CreateCommand();
                dbCmd.CommandText = String.Format(
                    "INSERT INTO {0} ({1},{2},{3}) VALUES ({4},{5},{6});",
                    Table,
                    DateColumn,
                    MessageColumn,
                    MessageLevelColumn,
                    DateTime.Now,
                    messageLevel.ToString().ToUpper(),
                    message);
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
