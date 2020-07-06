using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class DBFunctions
    {
        private DBFunctions()
        {

        }
        static DBFunctions instance = new DBFunctions();
        public static DBFunctions GetInstance()
        {
            return instance;
        }
        private SQLiteConnection connection;

        public void OpenConnection()
        {
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }
        public void CloseConnection()
        {
            if (connection.State != System.Data.ConnectionState.Closed)
            {
                connection.Close();
            }
        }
        private SQLiteCommand command;
        public SQLiteCommand GetCommand(string commandText)
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
            command.Parameters.Clear();
            command.CommandText = commandText;
            return command;
        }
        private SQLiteDataReader reader;
        public SQLiteDataReader GetReader(string commandText)
        {
            command.CommandText = commandText;
            return this.GetReader(command);
        }
        public SQLiteDataReader GetReader(SQLiteCommand command)
        {
            if (reader != null)
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
            }
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
            reader = command.ExecuteReader();
            return reader;
        }

        public void EndDbAction()
        {
            if (reader != null)
            {
                if (!reader.IsClosed)
                {
                    reader.Close();
                }
                reader.Dispose();
            }
            if (command != null)
            {
                command.Dispose();
            }
            if (connection != null)
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
        }
        SQLiteTransaction dataTransaction;
        public void BeginBatchOperation()
        {
            OpenConnection();

            dataTransaction = connection.BeginTransaction();
            command.Transaction = dataTransaction;
        }
        public void CommitBatchOperation()
        {
            if (dataTransaction != null)
            {
                dataTransaction.Commit();
                dataTransaction.Dispose();
            }
        }
        public void RollBackBatchOperation()
        {
            if (dataTransaction != null)
            {
                dataTransaction.Rollback();
            }
        }
        //private List<string> GetAllTableNames()
        //{
        //    List<string> tableNames = new List<string>();
        //    OpenConnection();
        //    object[] objArrRestrict;
        //    //select just TABLE in the Object array of restrictions.
        //    //Remove TABLE and insert Null to see tables, views, and other objects.
        //    objArrRestrict = new object[] { null, null, null, "TABLE" };
        //    DataTable schemaTbl = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, objArrRestrict);
        //    // Display the table name from each row in the schema
        //    foreach (DataRow row in schemaTbl.Rows)
        //    {
        //        tableNames.Add(row["TABLE_NAME"].ToString());
        //    }
        //    return tableNames;
        //}
        /// <summary>
        /// Check if DB contains all the required tables, fields
        /// </summary>
        /// <returns></returns>
        public bool IsDataFileValid()
        {
            return true; //TODO: check thoroughly every  table and fields is present
        }
        string _dataLocationFullPath;
        public void SetConnectionString(string dataLocationFullPath)
        {
            _dataLocationFullPath = dataLocationFullPath;
            if (connection != null)
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Dispose();
            }
            if (Environment.OSVersion.Version.Major >= 6)
            {
                connection = new SQLiteConnection(string.Format(
                    //"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Persist Security Info=False;Jet OLEDB:Database Password=prjr8oZv32iul0", dataLocationFullPath));
                    "Data Source={0}", dataLocationFullPath));
            }//top secret password for mdb connection is here
            else
            {
                connection = new SQLiteConnection(string.Format(
                    //"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Persist Security Info=False;Jet OLEDB:Database Password=prjr8oZv32iul0", dataLocationFullPath));
                    "Data Source={0}", dataLocationFullPath));
            }
            command = new SQLiteCommand();
            command.Connection = connection;
            connection.Open();
        }

        internal bool CheckIfColumnExistsInTable(string columnName, string tableName)
        {
            OpenConnection();

            DataSet dTable = new DataSet();

            // Get the table definition loaded in a table adapter
            string strSql = "Select TOP 1 * from " + tableName;
            SQLiteDataAdapter dbAdapater = new SQLiteDataAdapter(strSql, connection);
            dbAdapater.Fill(dTable);

            // Get the index of the field name
            //Dim i As Integer = 
            if (dTable.Tables[0].Columns.IndexOf(columnName) == -1)
            {
                //field is missing
                return false;
            }
            else
            {
                return true;
            }
        }

        public string GetDBName()
        {
            return System.IO.Path.GetFileName(_dataLocationFullPath);
        }

        //internal bool CheckIfTableExistsInDB(string tableName)
        //{
        //    return GetAllTableNames().Contains(tableName);
        //}
    }
}
