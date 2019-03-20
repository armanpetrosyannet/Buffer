using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Reciver.Data
{
    public abstract class DataCommand
    {

        private static NameValueCollection Settings = (NameValueCollection)ConfigurationManager.GetSection("CommandSettings") as NameValueCollection;

        protected CommandType CommandType { get { return CommandType.Text; } }

        protected int CommandTimeout;

        protected int UpdateCommandParametersCount;
        protected int InsertCommandParametersCount;

        private SqlConnection connection = null;

        public DataCommand()
        {
            CommandTimeout = Int32.Parse(Settings["CommandTimeOut"]);
            UpdateCommandParametersCount = Int32.Parse(Settings["UpdateCommandParameterCount"]);
            InsertCommandParametersCount = Int32.Parse(Settings["InsertCommandParameterCount"]);
        }

        protected virtual string ConnectionString
        {
            get
            {
                return ConfigurationManager.ConnectionStrings["CRMDWConnection"].ConnectionString;
            }
        }
        protected internal SqlConnection Connection
        {
            get
            {
                if (this.connection != null && this.connection.State != ConnectionState.Closed) return this.connection;
                SqlConnection dbConnection = new SqlConnection(ConnectionString);
                dbConnection.Open();
                this.connection = dbConnection;
                return connection;
            }
        }



    }
}
