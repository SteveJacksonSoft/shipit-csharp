using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Models.ApiModels
{
    public class OutboundOrderResponse
    {
        public OutboundOrderResponse(IEnumerable<Truck> truckLoads)
        {
            TruckLoads = truckLoads.ToList();
        }
        
        public int NumberOfTrucks => TruckLoads.Count;

        public List<Truck> TruckLoads { get; set; }
    }
}