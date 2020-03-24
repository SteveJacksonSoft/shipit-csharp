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

        public static List<Truck> PackOrdersIntoTrucks(IEnumerable<ProductOrder> products)
        {
            var ordersByDecreasingWeight = SortInDecreasingWeightOrder(products);

            return PackIntoTrucks(ordersByDecreasingWeight);
        }

        private static IEnumerable<ProductOrder> SortInDecreasingWeightOrder(IEnumerable<ProductOrder> productOrders)
        {
            return productOrders.OrderBy(order => order.Quantity * order.Product.Weight).Reverse();
        }

        private static List<Truck> PackIntoTrucks(IEnumerable<ProductOrder> sortedOrders)
        {
            var trucks = new List<Truck> {new Truck()};

            foreach (var item in sortedOrders)
            {
                trucks = FitProductOrderInEmptiestOpenTruck(item, trucks);
            }

            return trucks;
        }

        private static List<Truck> FitProductOrderInEmptiestOpenTruck(ProductOrder order, List<Truck> trucks)
        {
            var trucksByAscendingWeight = trucks.OrderBy(truck => truck.ContentWeightInGrams);

            var emptiestTruck = trucksByAscendingWeight.First();
            if (emptiestTruck.ContentWeightInGrams + order.TotalWeight <= truckCapacity)
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

        private static Truck FillEmptyTruckWithProduct(Product product)
        {
            var truck = new Truck();

            while (truck.ContentWeightInGrams + product.Weight < truckCapacity)
            {
                truck.Items.Add(product);
            }

            return truck;
        }
    }
}