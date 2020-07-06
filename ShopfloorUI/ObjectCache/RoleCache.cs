using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class RoleCache
    {
        public List<EmployeeRole> Roles { get; set; }
        object dummy = new object();

        private RoleCache()
        {
            lock (dummy)
            {
                if (Roles == null)
                {
                    Roles = new List<EmployeeRole>();
                }
                Roles?.Clear();
                EmployeeRole role;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, Description from Roles"))
                {
                    while (reader.Read())
                    {
                        role = new EmployeeRole();
                        role.Id = (Int64)reader[0];
                        role.Name = reader[1].ToString();
                        role.Description = reader[2].ToString();
                        Roles.Add(role);
                    }
                    reader.Close();
                }
            }
        }
        private static RoleCache instance;
        public static RoleCache GetInstance()
        {
            if (instance == null)
            {
                instance = new RoleCache();
            }
            return instance;
        }

        public EmployeeRole GetRoleByName(string roleName)
        {
            return Roles.Find(delegate (EmployeeRole p) { return p.Name.Trim().ToLower() == roleName.Trim().ToLower(); });
        }

        public EmployeeRole GetRoleById(Int64 id)
        {
            return Roles.Find(delegate (EmployeeRole p) { return p.Id == id; });
        }

        public void InsertRole(EmployeeRole role)
        {
            if (GetRoleByName(role.Name) == null)
            {
                InsertRoleToDb(role);
                //then add it to cache with db ID
                Roles.Add(GetRoleFromDbByName(role.Name));
            }
        }

        private EmployeeRole GetRoleFromDbByName(string name)
        {
            var item = new EmployeeRole();
            item.Name = name;
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,Description From Roles Where (Name) = (?)");
            command.Parameters.AddWithValue("Name", item.Name);
            using SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command);
            if (reader.Read())
            {
                item.Id = (Int64)reader[0];
                item.Name =reader[1].ToString();
                //item.Unit = UnitCache.GetInstance().GetUnitById((int)reader[2]);
                item.Description = reader[2].ToString();
                reader.Close();
            }
            else
            {
                item = null;
            }
            return item;
        }
        private void InsertRoleToDb(EmployeeRole role)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Roles " +
                "(Name,Description) Values(?,?)");
            command.Parameters.AddWithValue("Name", role.Name);
            command.Parameters.AddWithValue("Description", role.Description);
            command.ExecuteNonQuery();
        }

        public void UpdateRoleWithName(EmployeeRole role)
        {
            var itemInCache = GetRoleByName(role.Name);
            //get the id
            role.Id = itemInCache.Id;
            UpdateRoleToDb(role);
            //update item to cache
            Roles[Roles.IndexOf(itemInCache)] = role;
        }

        private void UpdateRoleToDb(EmployeeRole role)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Roles " +
                "Set Name=?, Description=? Where Id=?");
            command.Parameters.AddWithValue("Name", role.Name);
            command.Parameters.AddWithValue("Description", role.Description);
            command.Parameters.AddWithValue("Id", role.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Roles != null)
            {
                Roles.Clear();
            }
            instance = null;
        }

        public void UpdateRole(EmployeeRole item)
        {
            var itemInCache = GetRoleById(item.Id);
            UpdateRoleToDb(item);
            //update item to cache
            Roles[Roles.IndexOf(itemInCache)] = item;
        }

        public void DeleteRole(Int64 selectedItemID)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Roles Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemID);
            command.ExecuteNonQuery();
            //remove from cache
            Roles.Remove(GetRoleById(selectedItemID));
        }
        public void DeleteRole(EmployeeRole roleToDelete)
        {
            DeleteRole(roleToDelete.Id);
        }
    }
}
