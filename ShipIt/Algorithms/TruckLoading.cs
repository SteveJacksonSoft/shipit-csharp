using System;
using System.Collections.Generic;
using System.Linq;
using ShipIt.Models.ApiModels;
using ShipIt.Models.ProcessingModels;

namespace ShipIt.Algorithms
{
    public class TruckLoading
    {
        private const float truckCapacity = 2_000_000;

        public static List<TruckLoad> PackOrdersIntoTrucks(IEnumerable<ProductOrder> products)
        {
            var sortedOrders = SortInDecreasingWeightOrder(products);

            return PackIntoTrucks(sortedOrders);
        }

        private static IEnumerable<ProductOrder> SortInDecreasingWeightOrder(IEnumerable<ProductOrder> productOrders)
        {
            return productOrders.OrderBy(order => order.Quantity * order.Product.Weight).Reverse();
        }

        private static List<TruckLoad> PackIntoTrucks(IEnumerable<ProductOrder> sortedOrders)
        {
            var trucks = new List<TruckLoad> {new TruckLoad()};

            foreach (var item in sortedOrders)
            {
                trucks = FitProductOrderInEmptiestOpenTruck(item, trucks);
            }

            return trucks;
        }

        private static List<TruckLoad> FitProductOrderInEmptiestOpenTruck(ProductOrder order, List<TruckLoad> trucks)
        {
            var trucksByAscendingWeight = trucks.OrderBy(truck => truck.WeightInGrams);

            var emptiestTruck = trucksByAscendingWeight.First();
            if (emptiestTruck.WeightInGrams + order.TotalWeight <= truckCapacity)
            {
                for (var i = 0; i < order.Quantity; i++)
                {
                    emptiestTruck.Items.Add(order.Product);
                }

                return trucks;
            }

            var newTruck = FillEmptyTruckWithProduct(order.Product);

            trucks.Add(newTruck);

            return FitProductOrderInEmptiestOpenTruck (
                new ProductOrder {Product = order.Product, Quantity = order.Quantity - newTruck.Items.Count},
                trucks
            );
        }

        private static TruckLoad FillEmptyTruckWithProduct(Product product)
        {
            var truck = new TruckLoad();

            while (truck.WeightInGrams + product.Weight < truckCapacity)
            {
                truck.Items.Add(product);
            }

            return truck;
        }
    }
}