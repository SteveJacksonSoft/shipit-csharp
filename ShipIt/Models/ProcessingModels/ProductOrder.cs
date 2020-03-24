using ShipIt.Models.ApiModels;

namespace ShipIt.Models.ProcessingModels
{
    public class ProductOrder
    {
        public float TotalWeight => Quantity * Product.Weight;
        
        public int Quantity { get; set; }
        
        public Product Product { get; set; }
    }
}