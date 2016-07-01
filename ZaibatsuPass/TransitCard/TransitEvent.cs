using System;

namespace ZaibatsuPass.TransitCard
{
    public class TransitEvent
    {
        DateTime eventTime { get; set; }
        string LocalCost { get; set; }
        
        public string Route { get; set; }
        public string Station { get; set; }
        public string Agency { get; set; }
        public TransitEventType EventType { get; set; }

    }

    public enum TransitEventType
    {
        Bus,
        Train,
        Trolley,
        Metro,
        Ferry,
        TicketMachine,
        VendingMachine,
        POS,
        Other,
        Ban,
        Tram
    }
}
