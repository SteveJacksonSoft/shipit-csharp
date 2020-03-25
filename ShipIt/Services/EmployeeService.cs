using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Repositories;

namespace ShipIt.Services
{
    public class EmployeeService
    {
        private IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            this._employeeRepository = employeeRepository;
        }

        public void Delete(long id)
        {
            try
            {
                _employeeRepository.RemoveEmployee(id);
            }
            catch (NoSuchEntityException)
            {
                throw new NoSuchEntityException("No employee exists with id: " + id); 
            }
        }

        public void Delete(string name)
        {
            try
            {
                _employeeRepository.RemoveEmployee(name);
            }
            catch (NoSuchEntityException)
            {
                throw new NoSuchEntityException("No employee exists with name: " + name);
            }
        }
    }
}