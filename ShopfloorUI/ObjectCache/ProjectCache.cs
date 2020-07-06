using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class ProjectCache
    {
        public List<Project> Projects { get; set; }
        object dummy = new object();

        

        private ProjectCache()
        {
            lock (dummy)
            {
                if (Projects == null)
                {
                    Projects = new List<Project>();
                }
                Projects?.Clear();
                Project project;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, CustomerId, OrderStatus, PoDate, PoNumber, QuoteNumber, StartDate, Quantity, Description, DeadlineDate from Projects")) 
                {
                    while (reader.Read())
                    {
                        project = new Project();
                        project.Id = (Int64)reader[0];
                        project.Name = reader[1].ToString();
                        project.Customer = new Customer { Id = (Int64?)reader[2] };
                        project.OrderStatus = Enum.Parse<Project.ProjectOrderStatus>(reader[3].ToString());
                        project.PoDate = DateTimeOffset.Parse(reader[4].ToString());
                        project.PoNumber = reader[5].ToString();
                        project.QuoteNumber = reader[6].ToString();
                        project.StartDate = DateTimeOffset.Parse(reader[7].ToString());
                        project.Quantity = (Decimal)reader[8];
                        project.Description = reader[9].ToString();
                        string deadlineDate = reader[10].ToString();
                        if (String.IsNullOrEmpty(deadlineDate))
                        {
                            project.Deadline = null;
                        }
                        else
                        {
                            project.Deadline = DateTimeOffset.Parse(deadlineDate);
                        }
                        Projects.Add(project);
                    }
                    reader.Close();
                }
                foreach (var p in Projects)
                {
                    p.Customer = CustomerCache.GetInstance().GetById(p.Customer.Id);
                    p.Processes = ProcessCache.GetInstance().GetByProjectId(p.Id);
                }
            }
        }
        private static ProjectCache instance;
        public static ProjectCache GetInstance()
        {
            if (instance == null)
            {
                instance = new ProjectCache();
            }
            return instance;
        }
        /// <summary>
        /// Matches project by name, po number, po date, quote number, and returns matching object from cache.
        /// Used for retrieving the ID of a newly inserted project.
        /// </summary>
        /// <param name="prj"></param>
        /// <returns></returns>
        public Project GetByProps(Project prj)
        {
            return Projects.Find(p =>
            {
                return p.Name == prj.Name
                    && p.PoNumber == prj.PoNumber
                    && p.QuoteNumber == prj.QuoteNumber
                    && p.PoDate == prj.PoDate;
            });
        }
        //public Project GetByName(string name) <- more than one project can have same name hence commenting this method
        //{
        //    return Projects.Find(delegate (Project p) { return p.Name.Trim().ToLower() == name.Trim().ToLower(); });
        //}

        public Project GetById(Int64 id)
        {
            return Projects.Find(delegate (Project p) { return p.Id == id; });
        }
        public List<Project> GetProjectsPending()
        {
            return Projects.FindAll(p => { return p.ProgressStatus != Project.ProjectProgressStatus.Completed; });
        }
        public List<Project> GetProjectsCompleted()
        {
            return Projects.FindAll(p => { return p.ProgressStatus == Project.ProjectProgressStatus.Completed; });
        }

        public Project Insert(Project prj)
        {
            //if (GetByName(cust.Name) == null)
            {
                InsertToDb(prj);
                //get id of inserted value
                prj.Id = GetLastInsertedId();
                Projects.Add(prj);
                return prj;
            }
        }

        private long GetLastInsertedId()
        {
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("select last_insert_rowid()");
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            return LastRowID64;
        }
        private void InsertToDb(Project prj)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Projects " +
                "(Name, CustomerId, OrderStatus, PoDate, PoNumber, QuoteNumber, StartDate, Quantity, Description, DeadlineDate) Values(?,?,?,?,?,?,?,?,?,?)");
            command.Parameters.AddWithValue("Name", prj.Name);
            command.Parameters.AddWithValue("CustomerId", prj.Customer?.Id);
            command.Parameters.AddWithValue("OrderStatus", (int)prj.OrderStatus);
            command.Parameters.AddWithValue("PoDate", prj.PoDate?.ToString());
            command.Parameters.AddWithValue("PoNumber", prj.PoNumber);
            command.Parameters.AddWithValue("QuoteNumber", prj.QuoteNumber);
            command.Parameters.AddWithValue("StartDate", prj.StartDate?.ToString());
            command.Parameters.AddWithValue("Quantity", prj.Quantity);
            command.Parameters.AddWithValue("Description", prj.Description);
            command.Parameters.AddWithValue("DeadlineDate", prj.Deadline?.ToString());
            command.ExecuteNonQuery();
        }

        //public void UpdateWithName(Customer customer)
        //{
        //    var itemInCache = GetByName(customer.Name);
        //    //get the id
        //    customer.Id = itemInCache.Id;
        //    UpdateToDb(customer);
        //    //update item to cache
        //    Projects[Projects.IndexOf(itemInCache)] = customer;
        //}

        private void UpdateToDb(Project prj)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Projects " +
                "Set Name=?, CustomerId=?, OrderStatus=?, PoDate=?, PoNumber=?, QuoteNumber=?, StartDate=?, Quantity=?, Description=?, DeadlineDate=? Where Id=?");
            command.Parameters.AddWithValue("Name", prj.Name);
            command.Parameters.AddWithValue("CustomerId", prj.Customer?.Id);
            command.Parameters.AddWithValue("OrderStatus", (int)prj.OrderStatus);
            command.Parameters.AddWithValue("PoDate", prj.PoDate?.ToString());
            command.Parameters.AddWithValue("PoNumber", prj.PoNumber);
            command.Parameters.AddWithValue("QuoteNumber", prj.QuoteNumber);
            command.Parameters.AddWithValue("StartDate", prj.StartDate?.ToString());
            command.Parameters.AddWithValue("Quantity", prj.Quantity);
            command.Parameters.AddWithValue("Description", prj.Description);
            command.Parameters.AddWithValue("DeadlineDate", prj.Deadline?.ToString());
            command.Parameters.AddWithValue("Id", prj.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Projects != null)
            {
                Projects.Clear();
            }
            instance = null;
        }

        public void Update(Project prj)
        {
            var itemInCache = GetById(prj.Id);
            UpdateToDb(prj);
            //update item to cache
            Projects[Projects.IndexOf(itemInCache)] = prj;
        }

        public void Delete(Int64 selectedItemId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Projects Where Id=?");
            command.Parameters.AddWithValue("Id", selectedItemId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Projects.Remove(GetById(selectedItemId));
        }
        public void Delete(Project itemToDelete)
        {
            Delete(itemToDelete.Id);
        }
    }
}
