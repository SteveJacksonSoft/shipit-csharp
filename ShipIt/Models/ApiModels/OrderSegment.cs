﻿using System.Collections.Generic;

namespace ShipIt.Models.ApiModels
{
    public class InboundOrderResponse
    {
        public Employee OperationsManager { get; set; }
        public int WarehouseId { get; set; }
        public List<OrderSegment> OrderSegments { get; set; }
    }

    public class OrderSegment
    {
        public List<InboundOrder> OrderLines { get; set; }
        public Company Company { get; set; } 
    }
}