using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using ShipIt.Models.ApiModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class Status
    {
        public List<Employee> Employees { get; set; }
        public int WarehouseCount { get; set; }
        public int EmployeeCount { get; set; }
        public int ItemsTracked { get; set; }
        public int StockHeld { get; set; }
        public int ProductCount { get; set; }
        public int CompanyCount { get; set; }
    }

    public class StatusController : ApiController
    {
        private IEmployeeRepository employeeRepository;
        private ICompanyRepository companyRepository;
        private IProductRepository productRepository;
        private IStockRepository stockRepository;

        public StatusController(IEmployeeRepository employeeRepository, ICompanyRepository companyRepository,
            IProductRepository productRepository, IStockRepository stockRepository)
        {
            this.employeeRepository = employeeRepository;
            this.stockRepository = stockRepository;
            this.companyRepository = companyRepository;
            this.productRepository = productRepository;
        }

        // GET api/status
        public Status Get()
        {
            return new Status
            {
                Employees = employeeRepository.GetEmployeesByWarehouseId(1)
                    .Select(em => new Employee(em))
                    .ToList(),
                EmployeeCount = employeeRepository.GetCount(),
                ItemsTracked = stockRepository.GetTrackedItemsCount(),
                CompanyCount = companyRepository.GetCount(),
                ProductCount = productRepository.GetCount(),
                StockHeld = stockRepository.GetStockHeldSum(),
                WarehouseCount = employeeRepository.GetWarehouseCount()
            };
        }
    }
}