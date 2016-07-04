using System;

namespace ZaibatsuPass.TransitCard
{
    public class TransitEvent
    {
        DateTime eventTime { get; set; }
        public virtual string LocalCost { get; }
        public virtual string EventTitle { get; }
        public virtual string EventDetailsShort { get; }
        public virtual string EventDetailsLong { get; }
        public virtual TransitEventType EventType { get; }

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
