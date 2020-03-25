using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;
using ShipIt.Services;

namespace ShipIt.Controllers
{

    public class EmployeeController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IEmployeeRepository employeeRepository;

        private EmployeeService _employeeService;

        public EmployeeController(EmployeeService employeeService, IEmployeeRepository employeeRepository)
        {
            _employeeService = employeeService;
            this.employeeRepository = employeeRepository;
        }

        public EmployeeResponse GetAllWithName(string name)
        {
            var employees = employeeRepository.GetAllEmployeesWithName(name)
                .Select(e => new Employee(e));
            
            return new EmployeeResponse(employees);
        }
        
        public EmployeeResponse Get(string name)
        {
            log.Info(String.Format("Looking up employee by name: {0}", name));

            var employee = new Employee(employeeRepository.GetEmployeeByName(name));

            log.Info("Found employee: " + employee);
            return new EmployeeResponse(employee);
        }

        public EmployeeResponse Get(int warehouseId)
        {
            log.Info(String.Format("Looking up employee by id: {0}", warehouseId));

            var employees = employeeRepository
                .GetEmployeesByWarehouseId(warehouseId)
                .Select(e => new Employee(e));

            log.Info(String.Format("Found employees: {0}", employees));
            
            return new EmployeeResponse(employees);
        }

        public Response Post([FromBody]AddEmployeesRequest requestModel)
        {
            List<Employee> employeesToAdd = requestModel.Employees;

            if (employeesToAdd.Count == 0)
            {
                throw new MalformedRequestException("Expected at least one <employee> tag");
            }

            log.Info("Adding employees: " + employeesToAdd);

            employeeRepository.AddEmployees(employeesToAdd);

            log.Debug("Employees added successfully");

            return new Response() { Success = true };
        }

        public void Delete([FromBody]RemoveEmployeeRequest requestModel)
        {
            var name = requestModel.Name;
            if (name == null)
            {
                var id = requestModel.Id;
                if (id == 0)
                {
                    throw new MalformedRequestException("Unable to parse name from request parameters");
                }

                _employeeService.Delete(id);
                return;
            }

            _employeeService.Delete(name);
        }
    }
}
