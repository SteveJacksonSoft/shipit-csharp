using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using Npgsql;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;

namespace ShipIt.Repositories
{
    public interface IEmployeeRepository
    {
        int GetCount();
        int GetWarehouseCount();
        IEnumerable<EmployeeDataModel> GetAllEmployeesWithName(string name);
        EmployeeDataModel GetEmployeeByName(string name);
        IEnumerable<EmployeeDataModel> GetEmployeesByWarehouseId(int warehouseId);
        EmployeeDataModel GetOperationsManager(int warehouseId);
        void AddEmployees(IEnumerable<Employee> employees);
        void RemoveEmployee(string name);
        void RemoveEmployee(long id);
    }

    public class EmployeeRepository : RepositoryBase, IEmployeeRepository
    {
        public static IDbConnection CreateSqlConnection()
        {
            return new NpgsqlConnection(ConnectionHelper.GetConnectionString());
        }

        public int GetCount()
        {

            using (IDbConnection connection = CreateSqlConnection())
            {
                var command = connection.CreateCommand();
                string EmployeeCountSQL = "SELECT COUNT(*) FROM em";
                command.CommandText = EmployeeCountSQL;
                connection.Open();
                var reader = command.ExecuteReader();

                try
                {
                    reader.Read();
                    return (int) reader.GetInt64(0);
                }
                finally
                {
                    reader.Close();
                }
            };
        }

        public int GetWarehouseCount()
        {
            using (IDbConnection connection = CreateSqlConnection())
            {
                var command = connection.CreateCommand();
                string EmployeeCountSQL = "SELECT COUNT(DISTINCT w_id) FROM em";
                command.CommandText = EmployeeCountSQL;
                connection.Open();
                var reader = command.ExecuteReader();

                try
                {
                    reader.Read();
                    return (int)reader.GetInt64(0);
                }
                finally
                {
                    reader.Close();
                }
            };
        }

        public EmployeeDataModel Get(long id)
        {
            string sql = "SELECT id, name, w_id, role, ext FROM em WHERE id = @id";
            var parameter = new NpgsqlParameter("@id", id);
            string noProductWithIdErrorMessage = string.Format("No employees found with id: {0}", id);
            return base.RunSingleGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public IEnumerable<EmployeeDataModel> GetAllEmployeesWithName(string name)
        {
            string sql = "SELECT id, name, w_id, role, ext FROM em WHERE name = @name";
            var parameter = new NpgsqlParameter("@name", name);
            string noProductWithIdErrorMessage = string.Format("No employees found with name: {0}", name);
            return base.RunGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public EmployeeDataModel GetEmployeeByName(string name)
        {
            string sql = "SELECT id, name, w_id, role, ext FROM em WHERE name = @name";
            var parameter = new NpgsqlParameter("@name", name);
            string noProductWithIdErrorMessage = string.Format("No employees found with name: {0}", name);
            return base.RunSingleGetQuery(sql, reader => new EmployeeDataModel(reader),noProductWithIdErrorMessage, parameter);
        }

        public IEnumerable<EmployeeDataModel> GetEmployeesByWarehouseId(int warehouseId)
        {

            string sql = "SELECT id, name, w_id, role, ext FROM em WHERE w_id = @w_id";
            var parameter = new NpgsqlParameter("@w_id", warehouseId);
            string noProductWithIdErrorMessage =
                string.Format("No employees found with Warehouse Id: {0}", warehouseId);
            return base.RunGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage, parameter);
        }

        public EmployeeDataModel GetOperationsManager(int warehouseId)
        {

            string sql = "SELECT id, name, w_id, role, ext FROM em WHERE w_id = @w_id AND role = @role";
            var parameters = new []
            {
                new NpgsqlParameter("@w_id", warehouseId),
                new NpgsqlParameter("@role", DataBaseRoles.OperationsManager)
            };

            string noProductWithIdErrorMessage =
                string.Format("No employees found with Warehouse Id: {0}", warehouseId);
            return base.RunSingleGetQuery(sql, reader => new EmployeeDataModel(reader), noProductWithIdErrorMessage, parameters);
        }

        public void AddEmployee(Employee employee)
        {
            var sql = employee.Id == null
                ? "INSERT INTO em (name, w_id, role, ext) VALUES(@name, @w_id, @role, @ext)"
                : "INSERT INTO em (id, name, w_id, role, ext) VALUES(@id, @name, @w_id, @role, @ext)";

            var parameters = new EmployeeDataModel(employee).GetNpgsqlParameters().ToArray();

            base.RunQuery(sql, parameters);
        }

        public void AddEmployees(IEnumerable<Employee> employees)
        {
            const string sql = "INSERT INTO em (name, w_id, role, ext) VALUES(@name, @w_id, @role, @ext)";
            
            var parametersList = employees.Select(employee => new EmployeeDataModel(employee))
                .Select(employeeDataModel => employeeDataModel.GetNpgsqlParameters().ToArray())
                .ToList();

            base.RunTransaction(sql, parametersList);
        }

        public void RemoveEmployee(string name)
        {
            string sql = "DELETE FROM em WHERE name = @name";
            var parameter = new NpgsqlParameter("@name", name);
            var rowsDeleted = RunSingleQueryAndReturnRecordsAffected(sql, parameter);
            if (rowsDeleted == 0)
            {
                throw new NoSuchEntityException("Incorrect result size: expected 1, actual 0");
            }
            else if (rowsDeleted > 1)
            {
                throw new InvalidStateException("Unexpectedly deleted " + rowsDeleted + " rows, but expected a single update");
            }
        }

        public void RemoveEmployee(long id)
        {
            string sql = "DELETE FROM em WHERE id = @id";
            var parameter = new NpgsqlParameter("@id", id);
            var rowsDeleted = RunSingleQueryAndReturnRecordsAffected(sql, parameter);
            if (rowsDeleted == 0)
            {
                throw new NoSuchEntityException("Incorrect result size: expected 1, actual 0");
            }
        }
    }
}