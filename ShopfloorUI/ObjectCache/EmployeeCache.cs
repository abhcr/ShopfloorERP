using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class EmployeeCache
    {
        public List<Employee> Employees { get; set; }
        object dummy = new object();

        private EmployeeCache()
        {
            lock (dummy)
            {
                if(Employees == null)
                {
                    Employees = new List<Employee>();
                }
                Employees?.Clear();
                Employee emp;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, Phone, Address, RoleId from Employees"))
                {
                    while (reader.Read())
                    {
                        emp = new Employee();
                        emp.Id = (Int64)reader[0];
                        emp.Name = reader[1].ToString();
                        emp.Phone = reader[2].ToString();
                        emp.Address = reader[3].ToString();
                        emp.Role = new EmployeeRole { Id = (Int64)reader[4] };
                        Employees.Add(emp);
                    }
                    reader.Close();
                }
                foreach (var employee in Employees)
                {
                    employee.Role = RoleCache.GetInstance().GetRoleById(employee.Role.Id);
                }
            }
        }
        private static EmployeeCache instance;
        public static EmployeeCache GetInstance()
        {
            if (instance == null)
            {
                instance = new EmployeeCache();
            }
            return instance;
        }

        public Employee GetByName(string roleName)
        {
            return Employees.Find(delegate (Employee p) { return p.Name.Trim().ToLower() == roleName.Trim().ToLower(); });
        }

        public Employee GetById(Int64 id)
        {
            return Employees.Find(delegate (Employee p) { return p.Id == id; });
        }

        public void Insert(Employee employee)
        {
            if (GetByName(employee.Name) == null)
            {
                InsertToDb(employee);
                //then add it to cache with db ID
                Employees.Add(GetFromDbByName(employee.Name));
            }
        }

        private Employee GetFromDbByName(string name)
        {
            var emp = new Employee();
            emp.Name = name;
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,Phone,Address,RoleId From Employees Where (Name) = (?)");
            command.Parameters.AddWithValue("Name", emp.Name);
            using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command))
            {
                if (reader.Read())
                {
                    emp.Id = (Int64)reader[0];
                    emp.Name = reader[1].ToString();
                    emp.Phone = reader[2].ToString();
                    emp.Address = reader[3].ToString();
                    emp.Role = RoleCache.GetInstance().GetRoleById((Int64)reader[4]);
                }
                else
                {
                    emp = null;
                }
                reader.Close();
            }
            return emp;
        }
        private void InsertToDb(Employee emp)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Employees " +
                "(Name,Phone,Address,RoleId) Values(?,?,?,?)");
            command.Parameters.AddWithValue("Name", emp.Name);
            command.Parameters.AddWithValue("Phone", emp.Phone);
            command.Parameters.AddWithValue("Address", emp.Address);
            command.Parameters.AddWithValue("RoleId", emp.Role.Id);
            command.ExecuteNonQuery();
        }

        public void UpdateWithName(Employee emp)
        {
            var itemInCache = GetByName(emp.Name);
            //get the id
            emp.Id = itemInCache.Id;
            UpdateToDb(emp);
            //update item to cache
            Employees[Employees.IndexOf(itemInCache)] = emp;
        }

        private void UpdateToDb(Employee emp)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Employees " +
                "Set Name=?, Phone=?, Address=?,RoleId=? Where Id=?");
            command.Parameters.AddWithValue("Name", emp.Name);
            command.Parameters.AddWithValue("Phone", emp.Phone);
            command.Parameters.AddWithValue("Address", emp.Address);
            command.Parameters.AddWithValue("RoleId", emp.Role.Id);
            command.Parameters.AddWithValue("Id", emp.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Employees != null)
            {
                Employees.Clear();
            }
            instance = null;
        }

        public void Update(Employee emp)
        {
            var itemInCache = GetById(emp.Id);
            UpdateToDb(emp);
            //update item to cache
            Employees[Employees.IndexOf(itemInCache)] = emp;
        }

        public void Delete(Int64 selectedEmployeeId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Employees Where Id=?");
            command.Parameters.AddWithValue("Id", selectedEmployeeId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Employees.Remove(GetById(selectedEmployeeId));
        }
        public void Delete(Employee employeeToDelete)
        {
            Delete(employeeToDelete.Id);
        }
        internal List<Employee> UpdateAllQSizes()
        {
            foreach (var emp in Employees)
            {
                emp.QSize = ProcessCache.GetInstance().GetByEmployeeId(emp.Id).FindAll(p => p.Status != Process.ProcessStatus.Completed).Count;
            }
            return Employees;
        }
    }
}
