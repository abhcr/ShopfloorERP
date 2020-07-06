using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class VendorCache
    {
        public List<Vendor> Vendors { get; set; }
        object dummy = new object();

        private VendorCache()
        {
            lock (dummy)
            {
                if (Vendors == null)
                {
                    Vendors = new List<Vendor>();
                }
                Vendors?.Clear();
                Vendor vendor;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, FullAddress, Location, ContactPerson, Phone from Vendors"))
                {
                    while (reader.Read())
                    {
                        vendor = new Vendor();
                        vendor.Id = (Int64)reader[0];
                        vendor.Name = reader[1].ToString();
                        vendor.FullAddress = reader[2].ToString();
                        vendor.Location = reader[3].ToString();
                        vendor.ContactPerson = reader[4].ToString();
                        vendor.Phone = reader[5].ToString();
                        Vendors.Add(vendor);
                    }
                    reader.Close();
                }
            }
        }
        private static VendorCache instance;
        public static VendorCache GetInstance()
        {
            if (instance == null)
            {
                instance = new VendorCache();
            }
            return instance;
        }

        public Vendor GetByName(string name)
        {
            return Vendors.Find(delegate (Vendor p) { return p.Name.Trim().ToLower() == name.Trim().ToLower(); });
        }

        public Vendor GetById(Int64 id)
        {
            return Vendors.Find(delegate (Vendor p) { return p.Id == id; });
        }

        public void Insert(Vendor vendor)
        {
            if (GetByName(vendor.Name) == null)
            {
                InsertToDb(vendor);
                //then add it to cache with db ID
                Vendors.Add(GetFromDbByName(vendor.Name));
            }
        }

        private Vendor GetFromDbByName(string name)
        {
            var vendor = new Vendor();
            vendor.Name = name;
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,FullAddress,Location,ContactPerson,Phone From Vendors Where (Name) = (?)");
            command.Parameters.AddWithValue("Name", vendor.Name);
            using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command))
            {
                if (reader.Read())
                {
                    vendor.Id = (Int64)reader[0];
                    vendor.Name = reader[1].ToString();
                    vendor.FullAddress = reader[2].ToString();
                    vendor.Location = reader[3].ToString();
                    vendor.ContactPerson = reader[4].ToString();
                    vendor.Phone = reader[5].ToString();
                }
                else
                {
                    vendor = null;
                }
                reader.Close();
            }
            return vendor;
        }
        private void InsertToDb(Vendor vendor)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Vendors " +
                "(Name,FullAddress,Location,ContactPerson,Phone) Values(?,?,?,?,?)");
            command.Parameters.AddWithValue("Name", vendor.Name);
            command.Parameters.AddWithValue("FullAddress", vendor.FullAddress);
            command.Parameters.AddWithValue("Location", vendor.Location);
            command.Parameters.AddWithValue("ContactPerson", vendor.ContactPerson);
            command.Parameters.AddWithValue("Phone", vendor.Phone);
            command.ExecuteNonQuery();
        }

        public void UpdateWithName(Vendor vendor)
        {
            var itemInCache = GetByName(vendor.Name);
            //get the id
            vendor.Id = itemInCache.Id;
            UpdateToDb(vendor);
            //update item to cache
            Vendors[Vendors.IndexOf(itemInCache)] = vendor;
        }

        private void UpdateToDb(Vendor vendor)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Vendors " +
                "Set Name=?, FullAddress=?, Location=?, ContactPerson=?, Phone=? Where Id=?");
            command.Parameters.AddWithValue("Name", vendor.Name);
            command.Parameters.AddWithValue("FullAddress", vendor.FullAddress);
            command.Parameters.AddWithValue("Location", vendor.Location);
            command.Parameters.AddWithValue("ContactPerson", vendor.ContactPerson);
            command.Parameters.AddWithValue("Phone", vendor.Phone);
            command.Parameters.AddWithValue("Id", vendor.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Vendors != null)
            {
                Vendors.Clear();
            }
            instance = null;
        }

        public void Update(Vendor vendor)
        {
            var itemInCache = GetById(vendor.Id);
            UpdateToDb(vendor);
            //update item to cache
            Vendors[Vendors.IndexOf(itemInCache)] = vendor;
        }

        public void Delete(Int64 selectedItemId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Vendors Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Vendors.Remove(GetById(selectedItemId));
        }
        public void Delete(Vendor vendorToDelete)
        {
            Delete(vendorToDelete.Id);
        }
    }
}
