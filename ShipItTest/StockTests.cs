﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Repositories;
using ShipItTest.Builders;

namespace ShipItTest
{
    [TestClass]
    public class StockControllerTests : AbstractBaseTest
    {
        StockRepository stockRepository = new StockRepository();
        CompanyRepository companyRepository = new CompanyRepository();
        ProductRepository productRepository = new ProductRepository();

        private const string GTIN = "0000";

        public new void onSetUp()
        {
            base.onSetUp();
            companyRepository.AddCompanies(new List<Company>() { new CompanyBuilder().CreateCompany() });
            productRepository.AddProducts(new List<ProductDataModel>() { new ProductBuilder().setGtin(GTIN).CreateProductDatabaseModel() });
        }

        [TestMethod]
        public void TestAddNewStock()
        {
            onSetUp();
            var productId = productRepository.GetProductByGtin(GTIN).Id;

            stockRepository.AddStock(1, new List<StockAlteration>(){new StockAlteration(productId, 1)});

            var databaseStock = stockRepository.GetStockByWarehouseAndProductIds(1, new List<int>(){productId});
            Assert.AreEqual(databaseStock[productId].held, 1);
        }

        [TestMethod]
        public void TestUpdateExistingStock()
        {
            onSetUp();
            var productId = productRepository.GetProductByGtin(GTIN).Id;
            stockRepository.AddStock(1, new List<StockAlteration>() { new StockAlteration(productId, 2) });

            stockRepository.AddStock(1, new List<StockAlteration>() { new StockAlteration(productId, 5) });

            var databaseStock = stockRepository.GetStockByWarehouseAndProductIds(1, new List<int>() { productId });
            Assert.AreEqual(databaseStock[productId].held, 7);
        }
    }
}
