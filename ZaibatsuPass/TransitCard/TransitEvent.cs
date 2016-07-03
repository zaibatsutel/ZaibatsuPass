using System;

namespace ZaibatsuPass.TransitCard
{
    public class TransitEvent
    {
        DateTime eventTime { get; set; }
        public virtual string LocalCost { get; set; }
        public virtual string EventTitle { get; set; }
        public virtual string EventDetails { get; set; }
        public virtual TransitEventType EventType { get; set; }

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
