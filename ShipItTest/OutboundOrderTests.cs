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
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class OutboundOrderControllerTests : AbstractBaseTest
    {
        OutboundOrderController outboundOrderController = new OutboundOrderController(
            new StockRepository(),
            new ProductRepository()
        );
        StockRepository stockRepository = new StockRepository();
        CompanyRepository companyRepository = new CompanyRepository();
        ProductRepository productRepository = new ProductRepository();
        EmployeeRepository employeeRepository = new EmployeeRepository();

        private static Employee EMPLOYEE = new EmployeeBuilder().CreateEmployee();
        private static Company COMPANY = new CompanyBuilder().CreateCompany();
        private static readonly int WAREHOUSE_ID = EMPLOYEE.WarehouseId;

        private Product product1;
        private int productId1;
        private const string Gtin1 = "0000";
        
        private Product product2;
        private int productId2;
        private const string Gtin2 = "0001";

        public new void onSetUp()
        {
            base.onSetUp();
            employeeRepository.AddEmployees(new List<Employee>() { EMPLOYEE });
            companyRepository.AddCompanies(new List<Company>() { COMPANY });
            var productDataModel1 = new ProductBuilder().setGtin(Gtin1).CreateProductDatabaseModel();
            productRepository.AddProducts(new List<ProductDataModel>() { productDataModel1 });
            product1 = new Product(productRepository.GetProductByGtin(Gtin1));
            productId1 = product1.Id;
            var productDataModel2 = new ProductBuilder().setGtin(Gtin2).CreateProductDatabaseModel();
            productRepository.AddProducts(new List<ProductDataModel>() { productDataModel2 });
            product2 = new Product(productRepository.GetProductByGtin(Gtin2));
            productId2 = product2.Id;
        }

        [TestMethod]
        public void OutboundOrderRemovesCorrectNumberOfStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId1, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 3
                    }
                }
            };

            outboundOrderController.Post(outboundOrder);

            var stock = stockRepository.GetStockByWarehouseAndProductIds(WAREHOUSE_ID, new List<int> { productId1 })[productId1];
            Assert.AreEqual(stock.held, 7);
        }

        [TestMethod]
        public void OutboundOrderOnlyRemovesStockWithGivenGtin()
        {
            // given
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId1, 10) });
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId2, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 3
                    }
                }
            };

            // when
            outboundOrderController.Post(outboundOrder);

            // then
            var stock = stockRepository.GetStockByWarehouseId(WAREHOUSE_ID).ToList();
            var numberOfProduct1Held = stock.Where(s => s.ProductId == productId1).Sum(s => s.held);
            var numberOfProduct2Held = stock.Where(s => s.ProductId == productId2).Sum(s => s.held);
            Assert.AreEqual(numberOfProduct1Held, 7);
            Assert.AreEqual(numberOfProduct2Held, 10);
        }

        [TestMethod]
        public void TestOutboundOrderInsufficientStock()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId1, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 11
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin1));
            }
        }

        [TestMethod]
        public void TestOutboundOrderStockNotHeld()
        {
            onSetUp();
            var noStockGtin = Gtin1 + "XYZ";
            productRepository.AddProducts(new List<ProductDataModel>() { new ProductBuilder().setGtin(noStockGtin).CreateProductDatabaseModel() });
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId1, 10) });

            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 8
                    },
                    new GtinOrder()
                    {
                        gtin = noStockGtin,
                        quantity = 1000
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (InsufficientStockException e)
            {
                Assert.IsTrue(e.Message.Contains(noStockGtin));
                Assert.IsTrue(e.Message.Contains("no stock held"));
            }
        }

        [TestMethod]
        public void TestOutboundOrderBadGtin()
        {
            onSetUp();
            var badGtin = Gtin1 + "XYZ";

            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 1
                    },
                    new GtinOrder()
                    {
                        gtin = badGtin,
                        quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (NoSuchEntityException e)
            {
                Assert.IsTrue(e.Message.Contains(badGtin));
            }
        }

        [TestMethod]
        public void TestOutboundOrderDuplicateGtins()
        {
            onSetUp();
            stockRepository.AddStock(WAREHOUSE_ID, new List<StockAlteration>() { new StockAlteration(productId1, 10) });
            var outboundOrder = new OutboundOrderRequestModel()
            {
                WarehouseId = WAREHOUSE_ID,
                GtinOrders = new List<GtinOrder>()
                {
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 1
                    },
                    new GtinOrder()
                    {
                        gtin = Gtin1,
                        quantity = 1
                    }
                }
            };

            try
            {
                outboundOrderController.Post(outboundOrder);
                Assert.Fail("Expected exception to be thrown.");
            }
            catch (ValidationException e)
            {
                Assert.IsTrue(e.Message.Contains(Gtin1));
            }
        }
    }
}
