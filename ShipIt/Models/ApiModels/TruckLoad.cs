using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Models.ApiModels
{
    public class TruckLoad
    {
        public float WeightInGrams => Items.Select(item => item.Weight).Sum();

        public List<Product> Items { get; set; } 
    }
}