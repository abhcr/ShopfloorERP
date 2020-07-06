using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class ProcessQCache
    {
        public List<ProcessQ> Qs { get; set; }
        object dummy = new object();

        private ProcessQCache()
        {
            lock (dummy)
            {
                if (Qs == null)
                {
                    Qs = new List<ProcessQ>();
                }
                Qs?.Clear();
                ProcessQ q;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, ProcessId, QNumber from ProcessQ"))
                {
                    while (reader.Read())
                    {
                        q = new ProcessQ();
                        q.Id = (Int64)reader[0];
                        q.ProcessId = (Int64)reader[1];
                        q.QNumber = (Int64)reader[2];
                        Qs.Add(q);
                    }
                    reader.Close();
                }
            }
        }
        private static ProcessQCache instance;
        public static ProcessQCache GetInstance()
        {
            if (instance == null)
            {
                instance = new ProcessQCache();
            }
            return instance;
        }

        public ProcessQ GetByProcess(Process process)
        {
            return Qs.Find(p => p.ProcessId == process.Id);
        }

        //public ProcessQCache GetRoleByName(string roleName)
        //{
        //    return Qs.Find(delegate (EmployeeRole p) { return p.Name.Trim().ToLower() == roleName.Trim().ToLower(); });
        //}

        public ProcessQ GetById(Int64 id)
        {
            return Qs.Find(delegate (ProcessQ p) { return p.Id == id; });
        }

        public ProcessQ Insert(ProcessQ q)
        {
            //if (GetRoleByName(role.Name) == null)
            {
                InsertToDb(q);
                //then add it to cache with db ID
                q.Id = GetLastInsertedId();
                Qs.Add(q);
                return q;
            }
        }

        private Int64 GetLastInsertedId()
        {
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("select last_insert_rowid()");
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            return LastRowID64;
        }

        //private EmployeeRole GetRoleFromDbByName(string name)
        //{
        //    var item = new EmployeeRole();
        //    item.Name = name;
        //    SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,Description From Roles Where (Name) = (?)");
        //    command.Parameters.AddWithValue("Name", item.Name);
        //    using SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command);
        //    if (reader.Read())
        //    {
        //        item.Id = (Int64)reader[0];
        //        item.Name = reader[1].ToString();
        //        //item.Unit = UnitCache.GetInstance().GetUnitById((int)reader[2]);
        //        item.Description = reader[2].ToString();
        //        reader.Close();
        //    }
        //    else
        //    {
        //        item = null;
        //    }
        //    return item;
        //}

        private void InsertToDb(ProcessQ q)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into ProcessQ " +
                "(ProcessId, QNumber) Values(?,?)");
            command.Parameters.AddWithValue("ProcessId", q.ProcessId);
            command.Parameters.AddWithValue("QNumber", q.QNumber);
            command.ExecuteNonQuery();
        }

        //public void UpdateRoleWithName(EmployeeRole role)
        //{
        //    var itemInCache = GetRoleByName(role.Name);
        //    //get the id
        //    role.Id = itemInCache.Id;
        //    UpdateRoleToDb(role);
        //    //update item to cache
        //    Qs[Qs.IndexOf(itemInCache)] = role;
        //}

        private void UpdateToDb(ProcessQ q)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update ProcessQ " +
                "Set ProcessId=?, QNumber=? Where Id=?");
            command.Parameters.AddWithValue("ProcessId", q.ProcessId);
            command.Parameters.AddWithValue("QNumber", q.QNumber);
            command.Parameters.AddWithValue("Id", q.Id);
            _ = command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Qs != null)
            {
                Qs.Clear();
            }
            instance = null;
        }

        public void Update(ProcessQ item)
        {
            var itemInCache = GetById(item.Id);
            UpdateToDb(item);
            //update item to cache
            Qs[Qs.IndexOf(itemInCache)] = item;
        }
        /// <summary>
        /// Update ProcessQ object of the mentioned process
        /// </summary>
        /// <param name="proc"></param>
        public void Update(Process proc)
        {
            Update(GetInstance().GetByProcess(proc));
        }

        public void Delete(Int64 selectedItemID)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From ProcessQ Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemID);
            command.ExecuteNonQuery();
            //remove from cache
            Qs.Remove(GetById(selectedItemID));
        }
        public void Delete(ProcessQ itemToDelete)
        {
            Delete(itemToDelete.Id);
        }
    }
}
