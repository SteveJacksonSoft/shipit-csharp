using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Models.ApiModels
{
    public class Truck
    {
        public float ContentWeightInGrams => Items.Select(item => item.Weight).Sum();

        public readonly List<Product> Items = new List<Product>(); 
    }
}