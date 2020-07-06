using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class CustomerCache
    {
        public List<Customer> Customers { get; set; }
        object dummy = new object();

        private CustomerCache()
        {
            lock (dummy)
            {
                if (Customers == null)
                {
                    Customers = new List<Customer>();
                }
                Customers?.Clear();
                Customer customer;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, FullAddress, Location, ContactPerson, Phone, ManagerId from Customers"))
                {
                    while (reader.Read())
                    {
                        customer = new Customer();
                        customer.Id = (Int64)reader[0];
                        customer.Name = reader[1].ToString();
                        customer.FullAddress = reader[2].ToString();
                        customer.Location = reader[3].ToString();
                        customer.ContactPerson = reader[4].ToString();
                        customer.Phone = reader[5].ToString();
                        customer.AccountManager = new Employee { Id = (Int64)reader[6] };
                        Customers.Add(customer);
                    }
                    reader.Close();
                }
                foreach (var c in Customers)
                {
                    c.AccountManager = EmployeeCache.GetInstance().GetById(c.AccountManager.Id);
                }
            }
        }
        private static CustomerCache instance;
        public static CustomerCache GetInstance()
        {
            if (instance == null)
            {
                instance = new CustomerCache();
            }
            return instance;
        }

        public Customer GetByName(string name)
        {
            return Customers.Find(delegate (Customer p) { return p.Name.Trim().ToLower() == name.Trim().ToLower(); });
        }

        public Customer GetById(Int64? id)
        {
            return Customers.Find(delegate (Customer p) { return p.Id == id; });
        }

        public void Insert(Customer cust)
        {
            if (GetByName(cust.Name) == null)
            {
                InsertToDb(cust);
                //then add it to cache with db ID
                Customers.Add(GetFromDbByName(cust.Name));
            }
        }

        private Customer GetFromDbByName(string name)
        {
            var cust = new Customer();
            cust.Name = name;
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,FullAddress,Location,ContactPerson,Phone,ManagerId From Customers Where (Name) = (?)");
            command.Parameters.AddWithValue("Name", cust.Name);
            using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command))
            {
                if (reader.Read())
                {
                    cust.Id = (Int64)reader[0];
                    cust.Name = reader[1].ToString();
                    cust.FullAddress = reader[2].ToString();
                    cust.Location = reader[3].ToString();
                    cust.ContactPerson = reader[4].ToString();
                    cust.Phone = reader[5].ToString();
                    cust.AccountManager = EmployeeCache.GetInstance().GetById((Int64)reader[6]);
                }
                else
                {
                    cust = null;
                }
                reader.Close();
            }
            return cust;
        }
        private void InsertToDb(Customer cust)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Customers " +
                "(Name,FullAddress,Location,ContactPerson,Phone,ManagerId) Values(?,?,?,?,?,?)");
            command.Parameters.AddWithValue("Name", cust.Name);
            command.Parameters.AddWithValue("FullAddress", cust.FullAddress);
            command.Parameters.AddWithValue("Location", cust.Location);
            command.Parameters.AddWithValue("ContactPerson", cust.ContactPerson);
            command.Parameters.AddWithValue("Phone", cust.Phone);
            command.Parameters.AddWithValue("ManagerId", cust.AccountManager.Id);
            command.ExecuteNonQuery();
        }

        public void UpdateWithName(Customer customer)
        {
            var itemInCache = GetByName(customer.Name);
            //get the id
            customer.Id = itemInCache.Id;
            UpdateToDb(customer);
            //update item to cache
            Customers[Customers.IndexOf(itemInCache)] = customer;
        }

        private void UpdateToDb(Customer customer)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Customers " +
                "Set Name=?, FullAddress=?, Location=?, ContactPerson=?, Phone=?, ManagerId=? Where Id=?");
            command.Parameters.AddWithValue("Name", customer.Name);
            command.Parameters.AddWithValue("FullAddress", customer.FullAddress);
            command.Parameters.AddWithValue("Location", customer.Location);
            command.Parameters.AddWithValue("ContactPerson", customer.ContactPerson);
            command.Parameters.AddWithValue("Phone", customer.Phone);
            command.Parameters.AddWithValue("ManagerId", customer.AccountManager.Id);
            command.Parameters.AddWithValue("Id", customer.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Customers != null)
            {
                Customers.Clear();
            }
            instance = null;
        }

        public void Update(Customer cust)
        {
            var itemInCache = GetById(cust.Id);
            UpdateToDb(cust);
            //update item to cache
            Customers[Customers.IndexOf(itemInCache)] = cust;
        }

        public void Delete(Int64 selectedItemId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Customers Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Customers.Remove(GetById(selectedItemId));
        }
        public void Delete(Customer itemToDelete)
        {
            if (itemToDelete.Id.HasValue)
            {
                Delete(itemToDelete.Id.Value);
            }
        }
    }
}
