using ShopfloorUI.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace ShopfloorUI.ObjectCache
{
    public class ProcessCache
    {
        public List<Process> Processes { get; set; }
        object dummy = new object();

        private ProcessCache()
        {
            lock (dummy)
            {
                if (Processes == null)
                {
                    Processes = new List<Process>();
                }
                Processes?.Clear();
                Process process;
                DBFunctions.GetInstance().OpenConnection();
                using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader("Select Id, Name, Description, ResourceId, EmployeeId, Rate, StartDate, EndDate, DurationHours, Status, ProjectId, Type from Processes"))//, QNumber from Processes"))
                {
                    while (reader.Read())
                    {
                        process = new Process();
                        process.Id = (Int64)reader[0];
                        process.Name = reader[1].ToString();
                        process.Description = reader[2].ToString();
                        process.ExecutingResource = new Resource { Id = (Int64)reader[3] };
                        process.ExecutingEmployee = new Employee { Id = (Int64)reader[4] };
                        process.Rate = reader[5].ToString();
                        var startDate = reader[6].ToString();
                        if (string.IsNullOrEmpty(startDate))
                        {
                            process.StartDate = null;
                        }
                        else
                        {
                            process.StartDate = DateTimeOffset.Parse(startDate);
                        }
                        var endDate = reader[7].ToString();
                        if (string.IsNullOrEmpty(endDate))
                        {
                            process.EndDate = null;
                        }
                        else
                        {
                            process.EndDate = DateTimeOffset.Parse(endDate);
                        }
                        process.DurationHours = reader[8].ToString();
                        process.Status = Enum.Parse<Process.ProcessStatus>(reader[9].ToString());
                        process.ProjectId = (Int64)reader[10];
                        process.Type = Enum.Parse<Process.ProcessType>(reader[11].ToString());
                        //process.QNumber = int.Parse(reader[12].ToString());
                        Processes.Add(process);
                    }
                    reader.Close();
                }
                foreach (var p in Processes)
                {
                    p.ExecutingResource = ResourceCache.GetInstance().GetById(p.ExecutingResource.Id);
                    p.ExecutingEmployee = EmployeeCache.GetInstance().GetById(p.ExecutingEmployee.Id);
                }
            }
        }

        internal List<Process> GetByProjectId(long projectId)
        {
            return Processes.FindAll(p => { return p.ProjectId == projectId; });
        }

        private static ProcessCache instance;
        public static ProcessCache GetInstance()
        {
            if (instance == null)
            {
                instance = new ProcessCache();
            }
            return instance;
        }

        
        //public Process GetByObject(Process process)
        //{
        //    return Processes.Find(delegate (Process p) 
        //    { 
        //        return (p.Name.Trim().ToLower() == process.Name.Trim().ToLower())
        //            && (p.Description == process.Description)
        //            && (p.ProjectId == process.ProjectId)
        //            && (p.Type == process.Type); });
        //}

        public Process GetById(Int64 id)
        {
            return Processes.Find(delegate (Process p) { return p.Id == id; });
        }

        public List<Process> GetByResourceId(long id)
        {
            return Processes.FindAll(p => p.ExecutingResource.Id == id);
        }
        public List<Process> GetByEmployeeId(long id)
        {
            return Processes.FindAll(p => p.ExecutingEmployee.Id == id);
        }
        //public int GetNextQNumber(Resource res)
        //{
        //    var quedProcsOfResource = GetProcessesQueueByResourceId(res.Id)?.FindAll((p) => { return p.QNumber > 0; });
        //    return quedProcsOfResource.Count + 1;
        //}

        //public List<Process> GetProcessesQueueByResourceId(Int64 resId)
        //{
        //    return GetInstance().Processes.FindAll((p) => { return p.ExecutingResource?.Id == resId; }).OrderBy(p => p.QNumber).ToList<Process>();
        //}

        public Process Insert(Process proc)
        {
            //if (GetByObject(proc) == null)
            {
                InsertToDb(proc);
                proc.Id = GetLastInsertedId();
                Processes.Add(proc);
                return proc;
            }
        }
        private long GetLastInsertedId()
        {
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("select last_insert_rowid()");
            Int64 LastRowID64 = (Int64)command.ExecuteScalar();
            return LastRowID64;
        }
        //private Process GetFromDbByName(string name)
        //{
        //    var emp = new Employee();
        //    emp.Name = name;
        //    SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Select ID,Name,Phone,Address,RoleId From Employees Where (Name) = (?)");
        //    command.Parameters.AddWithValue("Name", emp.Name);
        //    using (SQLiteDataReader reader = DBFunctions.GetInstance().GetReader(command))
        //    {
        //        if (reader.Read())
        //        {
        //            emp.Id = (Int64)reader[0];
        //            emp.Name = reader[1].ToString();
        //            emp.Phone = reader[2].ToString();
        //            emp.Address = reader[3].ToString();
        //            emp.Role = RoleCache.GetInstance().GetRoleById((Int64)reader[4]);
        //        }
        //        else
        //        {
        //            emp = null;
        //        }
        //        reader.Close();
        //    }
        //    return emp;
        //}
        private void InsertToDb(Process proc)
        {
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Insert Into Processes " +
                "(Name, Description, ResourceId, EmployeeId, Rate, StartDate, EndDate, DurationHours, Status, ProjectId, Type) Values(?,?,?,?,?,?,?,?,?,?,?)");
            command.Parameters.AddWithValue("Name", proc.Name);
            command.Parameters.AddWithValue("Description", proc.Description);
            command.Parameters.AddWithValue("ResourceId", proc.ExecutingResource?.Id);
            command.Parameters.AddWithValue("EmployeeId", proc.ExecutingEmployee?.Id);
            command.Parameters.AddWithValue("Rate", proc.Rate);
            command.Parameters.AddWithValue("StartDate", proc.StartDate?.ToString());
            command.Parameters.AddWithValue("EndDate", proc.EndDate?.ToString());
            command.Parameters.AddWithValue("DurationHours", proc.DurationHours);
            command.Parameters.AddWithValue("Status", (int)proc.Status);
            command.Parameters.AddWithValue("ProjectId", proc.ProjectId);
            command.Parameters.AddWithValue("Type", (int)proc.Type);
            //command.Parameters.AddWithValue("QNumber", proc.QNumber);
            command.ExecuteNonQuery();
        }

        //public void UpdateWithName(Employee emp)
        //{
        //    var itemInCache = GetByObject(emp.Name);
        //    //get the id
        //    emp.Id = itemInCache.Id;
        //    UpdateToDb(emp);
        //    //update item to cache
        //    Processes[Processes.IndexOf(itemInCache)] = emp;
        //}

        private void UpdateToDb(Process proc)
        {
            //do not update stock here.. it is updated by stock cache
            DBFunctions.GetInstance().OpenConnection();
            SQLiteCommand command = DBFunctions.GetInstance().GetCommand("Update Processes " +
                "Set Name=?, Description=?, ResourceId=?, EmployeeId=?, Rate=?, StartDate=?, EndDate=?, DurationHours=?, Status=?, ProjectId=?, Type=? Where Id=?");
            command.Parameters.AddWithValue("Name", proc.Name);
            command.Parameters.AddWithValue("Description", proc.Description);
            command.Parameters.AddWithValue("ResourceId", proc.ExecutingResource?.Id ?? 0);
            command.Parameters.AddWithValue("EmployeeId", proc.ExecutingEmployee?.Id ?? 0);
            command.Parameters.AddWithValue("Rate", proc.Rate);
            command.Parameters.AddWithValue("StartDate", proc.StartDate?.ToString() ?? string.Empty);
            command.Parameters.AddWithValue("EndDate", proc.EndDate?.ToString() ?? string.Empty);
            command.Parameters.AddWithValue("DurationHours", proc.DurationHours);
            command.Parameters.AddWithValue("Status", (int)proc.Status);
            command.Parameters.AddWithValue("ProjectId", proc.ProjectId);
            command.Parameters.AddWithValue("Type", (int)proc.Type);
            //command.Parameters.AddWithValue("QNumber", proc.QNumber);
            command.Parameters.AddWithValue("Id", proc.Id);
            command.ExecuteNonQuery();
        }

        public void Clear()
        {
            if (Processes != null)
            {
                Processes.Clear();
            }
            instance = null;
        }

        public void Update(Process proc)
        {
            var itemInCache = GetById(proc.Id);
            UpdateToDb(proc);
            //update item to cache
            Processes[Processes.IndexOf(itemInCache)] = proc;
        }

        public void Delete(Int64 selectedProcessId)
        {
            //remove from db
            DBFunctions.GetInstance().OpenConnection();
            var command = DBFunctions.GetInstance().GetCommand("Delete From Processes Where Id=?");
            command.Parameters.AddWithValue("Id", selectedProcessId);
            command.ExecuteNonQuery();
            //remove from cache
            _ = Processes.Remove(GetById(selectedProcessId));
        }
        public void Delete(Process itemToDelete)
        {
            Delete(itemToDelete.Id);
        }

        //public void UpdateAllQNumbers()
        //{
        //    //if (instance == null)
        //    //    return; 
        //    //Global queue updation
        //    var resources = ResourceCache.GetInstance().Resources;
        //    foreach (var resource in resources)
        //    {
        //        //get all processes of each resource in their descending order;
        //        var processes = GetInstance().GetProcessesQueueByResourceId(resource.Id);
        //        //clear process from queue if completed (by giving -1)
        //        foreach (var process in processes)
        //        {
        //            if(process.Status == Process.ProcessStatus.Completed)
        //            {
        //                process.QNumber = -1;
        //            }
        //        }
        //        //get rest of the processes from the above ordered list. 
        //        var pendingProcs = processes
        //            .FindAll(p => p.QNumber > -1)
        //            .OrderBy(p=>p.QNumber).ToList<Process>(); 
        //        for (int i = 0; i < pendingProcs.Count; i++)
        //        {
        //            //give them new numbers starting from 1.
        //            pendingProcs[i].QNumber = i + 1;
        //        }
        //    }
        //    //Update all to db
        //    foreach (var process in Processes)
        //    {
        //        Update(process);
        //    }
        //}
    }
}
