using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class ResourceCache
    {
        public List<Resource> Resources { get; set; }
        object dummy = new object();

        private ResourceCache()
        {
            lock (dummy)
            {
                if (Resources == null)
                {
                    Resources = new List<Resource>();
                }
                Resources?.Clear();
                Resource resource;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, Code, Type, Rate, FullAddress, Location from Resources"))
                {
                    while (reader.Read())
                    {
                        resource = new Resource();
                        resource.Id = (Int64)reader[0];
                        resource.Name = reader[1].ToString();
                        resource.Code = reader[2].ToString();
                        resource.Type = Enum.Parse<Resource.ResourceType>(reader[3].ToString());
                        resource.Rate =  reader[4].ToString();//new EmployeeRole { Id = (Int64)reader[4] };
                        resource.FullAddress = reader[5].ToString();
                        resource.Location = reader[6].ToString();
                        Resources.Add(resource);
                    }
                    reader.Close();
                }
            }
        }
        private static ResourceCache instance;
        public static ResourceCache GetInstance()
        {
            if (instance == null)
            {
                instance = new ResourceCache();
            }
            return instance;
        }
        public List<Resource> UpdateAllQSizes()
        {
            foreach (var res in Resources)
            {
                res.QSize = ProcessCache.GetInstance().GetByResourceId(res.Id).FindAll(p=>p.Status != Process.ProcessStatus.Completed).Count;
            }
            return Resources;
        }

        public Resource GetByName(string name)
        {
            return Resources.Find(delegate (Resource p) { return p.Name.Trim().ToLower() == name.Trim().ToLower(); });
        }

        public Resource GetById(Int64 id)
        {
            return Resources.Find(delegate (Resource p) { return p.Id == id; });
        }

        public void Insert(Resource resource)
        {
            if (GetByName(resource.Name) == null)
            {
                InsertToDb(resource);
                //then add it to cache with db ID
                Resources.Add(GetFromDbByName(resource.Name));
            }
        }

        private Resource GetFromDbByName(string name)
        {
            var res = new Resource();
            res.Name = name;
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,Code,Rate,Location,FullAddress,Type From Resources Where (Name) = (?)");
            command.Parameters.AddWithValue("Name", res.Name);
            using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command))
            {
                if (reader.Read())
                {
                    res.Id = (Int64)reader[0];
                    res.Name = reader[1].ToString();
                    res.Code = reader[2].ToString();
                    res.Rate = reader[3].ToString();
                    res.Location = reader[4].ToString();//RoleCache.GetInstance().GetRoleById((Int64)reader[4]);
                    res.FullAddress = reader[5].ToString();
                    res.Type = Enum.Parse<Resource.ResourceType>(reader[6].ToString());
                }
                else
                {
                    res = null;
                }
                reader.Close();
            }
            return res;
        }
        private void InsertToDb(Resource res)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Resources " +
                "(Name,Code,Rate,Location,FullAddress,Type) Values(?,?,?,?,?,?)");
            command.Parameters.AddWithValue("Name", res.Name);
            command.Parameters.AddWithValue("Code", res.Code);
            command.Parameters.AddWithValue("Rate", res.Rate);
            command.Parameters.AddWithValue("Location", res.Location);
            command.Parameters.AddWithValue("FullAddress", res.FullAddress);
            command.Parameters.AddWithValue("Type", (int)res.Type);
            command.ExecuteNonQuery();
        }

        public void UpdateWithName(Resource resource)
        {
            var itemInCache = GetByName(resource.Name);
            //get the id
            resource.Id = itemInCache.Id;
            UpdateToDb(resource);
            //update item to cache
            Resources[Resources.IndexOf(itemInCache)] = resource;
        }

        private void UpdateToDb(Resource res)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Resources " +
                "Set Name=?, Code=?, Rate=?,Location=?,FullAddress=?,Type=? Where Id=?");
            command.Parameters.AddWithValue("Name", res.Name);
            command.Parameters.AddWithValue("Code", res.Code);
            command.Parameters.AddWithValue("Rate", res.Rate);
            command.Parameters.AddWithValue("Location", res.Location);
            command.Parameters.AddWithValue("FullAddress", res.FullAddress);
            command.Parameters.AddWithValue("Type", (int)res.Type);
            command.Parameters.AddWithValue("Id", res.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Resources != null)
            {
                Resources.Clear();
            }
            instance = null;
        }

        public void Update(Resource res)
        {
            var itemInCache = GetById(res.Id);
            UpdateToDb(res);
            //update item to cache
            Resources[Resources.IndexOf(itemInCache)] = res;
        }

        public void Delete(Int64 selectedItemId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Resources Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Resources.Remove(GetById(selectedItemId));
        }
        public void Delete(Resource resourceToDelete)
        {
            Delete(resourceToDelete.Id);
        }
    }
}
