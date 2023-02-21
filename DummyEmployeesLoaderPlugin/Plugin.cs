using Newtonsoft.Json;
using PhoneApp.Domain.Attributes;
using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DummyEmployeesLoaderPlugin
{
    [Author(Name = "Daniil Melnikov")]
    public class Plugin : IPluggable
    {
        public class DummyRootObject
        {
            [JsonProperty("users")]
            public List<DummyUserDto> Users { get; set; }
        }
        public class DummyUserDto
        {
            [JsonProperty("firstName")]
            public string FirstName { get; set; }
            [JsonProperty("lastName")]
            public string LastName { get; set; }
            [JsonProperty("phone")]
            public string Phone { get; set; }
        }
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Starting Dummy Employees loader");

            try
            {
                var employeesList = new List<EmployeesDTO>();
                logger.Info($"Loaded {employeesList.Count()} employees");
                using (var client = new WebClient())
                {
                    var res = client.DownloadString(@"https://dummyjson.com/users");
                    var dummyUsers = JsonConvert.DeserializeObject<DummyRootObject>(res);

                    foreach (var dummyUser in dummyUsers.Users)
                    { 
                        var employee = new EmployeesDTO { Name = $"{dummyUser.FirstName} {dummyUser.LastName}" };
                        employee.AddPhone(dummyUser.Phone);
                        employeesList.Add(employee);
                    }
                }
                if (args is IEnumerable<EmployeesDTO> existEmployees && existEmployees != null)
                {
                    return existEmployees.Concat(employeesList);
                }
                return employeesList;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
                logger.Trace(ex.StackTrace);
            }

            return new List<DataTransferObject>();
        }
    }
}
