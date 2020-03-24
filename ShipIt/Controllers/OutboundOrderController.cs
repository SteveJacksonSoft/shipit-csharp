using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using log4net;
using ShipIt.Algorithms;
using ShipIt.Exceptions;
using ShipIt.Models.ApiModels;
using ShipIt.Models.DataModels;
using ShipIt.Models.ProcessingModels;
using ShipIt.Repositories;

namespace ShipIt.Controllers
{
    public class OutboundOrderController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IStockRepository stockRepository;
        private readonly IProductRepository productRepository;

        public OutboundOrderController(IStockRepository stockRepository, IProductRepository productRepository)
        {
            this.stockRepository = stockRepository;
            this.productRepository = productRepository;
        }

        public OutboundOrderResponse Post([FromBody] OutboundOrderRequestModel request)
        {
            log.Info(String.Format("Processing outbound order: {0}", request));

            var productsByGtin = productRepository
                .GetProductsByGtin(ExtractGtinsOrThrowIfDuplicates(request))
                .ToDictionary(p => p.Gtin, p => new Product(p));

            var lineItems = ExtractStockAlterationsOrThrowIfUnknownGtin(request, productsByGtin);

            var products = GetProductsAsOrdered(request.GtinOrders, productsByGtin).ToList();
            var stock = stockRepository.GetStockByWarehouseAndProductIds
            (
                request.WarehouseId,
                products.Select(product => product.Id).ToList()
            );

            EnsureSufficientStockHeld(request, lineItems, stock);

            stockRepository.RemoveStock(request.WarehouseId, lineItems);

            var productOrders = request.GtinOrders.Select(order =>
                new ProductOrder
                {
                    Product = productsByGtin[order.gtin],
                    Quantity = order.quantity
                }
            );

            return new OutboundOrderResponse(TruckLoading.PackOrdersIntoTrucks(productOrders));
        }

        private static void EnsureSufficientStockHeld
            (OutboundOrderRequestModel request, List<StockAlteration> lineItems, Dictionary<int, StockDataModel> stock)
        {
            var orderLines = request.GtinOrders.ToList();
            var errors = new List<string>();

            for (int i = 0; i < lineItems.Count; i++)
            {
                var lineItem = lineItems[i];
                var orderLine = orderLines[i];

                if (!stock.ContainsKey(lineItem.ProductId))
                {
                    errors.Add(string.Format("Product: {0}, no stock held", orderLine.gtin));
                    continue;
                }

                var item = stock[lineItem.ProductId];
                if (lineItem.Quantity > item.held)
                {
                    errors.Add(
                        string.Format("Product: {0}, stock held: {1}, stock to remove: {2}", orderLine.gtin, item.held,
                            lineItem.Quantity));
                }
            }

            if (errors.Count > 0)
            {
                throw new InsufficientStockException(string.Join("; ", errors));
            }
        }

        private static List<StockAlteration> ExtractStockAlterationsOrThrowIfUnknownGtin
            (OutboundOrderRequestModel request, Dictionary<string, Product> products)
        {
            var lineItems = new List<StockAlteration>();
            var errors = new List<string>();

            foreach (var orderLine in request.GtinOrders)
            {
                if (!products.ContainsKey(orderLine.gtin))
                {
                    errors.Add(string.Format("Unknown product gtin: {0}", orderLine.gtin));
                }
                else
                {
                    var product = products[orderLine.gtin];
                    lineItems.Add(new StockAlteration(product.Id, orderLine.quantity));
                }
            }

            if (errors.Count > 0)
            {
                throw new NoSuchEntityException(string.Join("; ", errors));
            }

            return lineItems;
        }

        private static IEnumerable<Product> GetProductsAsOrdered
            (IEnumerable<GtinOrder> orders, Dictionary<string, Product> products)
        {
            return products
                .Where(productKeyValuePair => orders.Any(line => line.gtin == productKeyValuePair.Key))
                .Select(pair => pair.Value);
        }

        private static List<string> ExtractGtinsOrThrowIfDuplicates(OutboundOrderRequestModel request)
        {
            var gtins = new List<String>();
            foreach (var orderLine in request.GtinOrders)
            {
                if (gtins.Contains(orderLine.gtin))
                {
                    throw new ValidationException(String.Format(
                        "Outbound order request contains duplicate product gtin: {0}",
                        orderLine.gtin
                    ));
                }

                gtins.Add(orderLine.gtin);
            }

            return gtins;
        }
    }
}