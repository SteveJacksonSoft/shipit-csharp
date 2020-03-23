using System.Collections.Generic;
using System.Linq;

namespace ShipIt.Models.ApiModels
{
    public class OutboundOrderResponse
    {
        public OutboundOrderResponse(IEnumerable<TruckLoad> truckLoads)
        {
            TruckLoads = truckLoads.ToList();
        }
        
        public int NumberOfTrucks => TruckLoads.Count;

        public List<TruckLoad> TruckLoads { get; set; }
    }
}