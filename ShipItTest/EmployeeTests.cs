using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Controllers;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipIt.Services;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class EmployeeControllerTests : AbstractBaseTest
    {
        private EmployeeController employeeController;
        private EmployeeRepository employeeRepository;

        private const string NAME = "Gissell Sadeem";
        private const long ID = 1;
        private const int WAREHOUSE_ID = 1;

        [TestInitialize]
        public void TestInitialize()
        {
            employeeRepository = new EmployeeRepository();
            employeeController = new EmployeeController(new EmployeeService(employeeRepository), employeeRepository);
        }

        [TestMethod]
        public void TestRoundtripEmployeeRepository()
        {
            onSetUp();
            var employee = new EmployeeBuilder().CreateEmployee();
            employeeRepository.AddEmployees(new List<Employee>() {employee});
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).Name, employee.Name);
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).Ext, employee.ext);
            Assert.AreEqual(employeeRepository.GetEmployeeByName(employee.Name).WarehouseId, employee.WarehouseId);
        }

        [TestMethod]
        public void TestGetEmployeeByName()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() {employeeBuilder.CreateEmployee()});
            var result = employeeController.Get(NAME);

            var correctEmployee = employeeBuilder.CreateEmployee();
            Assert.IsTrue(EmployeesAreEqual(correctEmployee, result.Employees.First()));
            Assert.IsTrue(result.Success);
        }

        [TestMethod]
        public void TestGetEmployeesByWarehouseId()
        {
            onSetUp();
            var employeeBuilderA = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("A");
            var employeeBuilderB = new EmployeeBuilder().setWarehouseId(WAREHOUSE_ID).setName("B");
            employeeRepository.AddEmployees(new List<Employee>()
                {employeeBuilderA.CreateEmployee(), employeeBuilderB.CreateEmployee()});
            var result = employeeController.Get(WAREHOUSE_ID).Employees.ToList();

            var correctEmployeeA = employeeBuilderA.CreateEmployee();
            var correctEmployeeB = employeeBuilderB.CreateEmployee();

            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeA, result.First()));
            Assert.IsTrue(EmployeesAreEqual(correctEmployeeB, result.Last()));
        }

        [TestMethod]
        public void TestGetNonExistentEmployee()
        {
            onSetUp();
            try
            {
                employeeController.Get(NAME);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [TestMethod]
        public void TestGetEmployeeInNonexistentWarehouse()
        {
            onSetUp();
            try
            {
                var employees = employeeController.Get(WAREHOUSE_ID).Employees.ToList();
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(WAREHOUSE_ID.ToString()));
            }
        }

        [TestMethod]
        public void TestAddEmployees()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            var response = employeeController.Post(addEmployeesRequest);
            var databaseEmployee = employeeRepository.GetEmployeeByName(NAME);
            var correctDatabaseEmploye = employeeBuilder.CreateEmployee();

            Assert.IsTrue(response.Success);
            Assert.IsTrue(EmployeesAreEqual(new Employee(databaseEmployee), correctDatabaseEmploye));
        }

        [TestMethod]
        public void TestDeleteEmployees()
        {
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() {employeeBuilder.CreateEmployee()});

            var removeEmployeeRequest = new RemoveEmployeeRequest() {Name = NAME};
            employeeController.Delete(removeEmployeeRequest);

            try
            {
                employeeController.Get(NAME);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [TestMethod]
        public void TestDeleteNonexistentEmployee()
        {
            onSetUp();
            var removeEmployeeRequest = new RemoveEmployeeRequest() {Name = NAME};

            try
            {
                employeeController.Delete(removeEmployeeRequest);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(NAME));
            }
        }

        [TestMethod]
        public void EmployeesGivenDifferentIdsWhenNameIdentical()
        {
            // given
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee> {employeeBuilder.CreateEmployee()});
            var addEmployeesRequest = employeeBuilder.CreateAddEmployeesRequest();

            // when
            employeeController.Post(addEmployeesRequest);

            // then
            var employees = employeeRepository.GetAllEmployeesWithName(NAME).ToList();
            Assert.AreNotEqual(employees[0].Id, employees[1].Id);
        }

        [TestMethod]
        public void GettingAllEmployeesWithNameReturnsMultipleEmployees()
        {
            // given
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>
                {
                    employeeBuilder.CreateEmployee(), employeeBuilder.CreateEmployee(),
                }
            );

            // when
            var employeesWithGivenName = employeeController.GetAllWithName(NAME).Employees;

            // then
            Assert.AreEqual(2, employeesWithGivenName.Count());
        }

        [TestMethod]
        public void DeletesEmployeeByIdIfIdGiven()
        {
            // given
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME).SetId(1);
            employeeRepository.AddEmployee(employeeBuilder.CreateEmployee());

            // when
            employeeController.Delete(new RemoveEmployeeRequest {Id = ID});

            // then
            try
            {
                employeeRepository.Get(ID);
                Assert.Fail("NoSuchEntityException should have been thrown");
            }
            catch (NoSuchEntityException)
            {
            }
        }

        [TestMethod]
        public void DeletesAllEmployeesWithNameIfIdNotGiven()
        {
            // given
            onSetUp();
            var employeeBuilder = new EmployeeBuilder().setName(NAME);
            employeeRepository.AddEmployees(new List<Employee>() {employeeBuilder.CreateEmployee()});

            // when
            employeeController.Delete(new RemoveEmployeeRequest() {Name = NAME});

            // then
            try
            {
                var response = employeeRepository.GetAllEmployeesWithName(NAME).ToList();
                Assert.Fail("NoSuchEntityException should have been thrown");
            }
            catch (NoSuchEntityException)
            {
            }
        }

        private bool EmployeesAreEqual(Employee A, Employee B)
        {
            return A.WarehouseId == B.WarehouseId
                   && A.Name == B.Name
                   && A.role == B.role
                   && A.ext == B.ext;
        }
    }
}