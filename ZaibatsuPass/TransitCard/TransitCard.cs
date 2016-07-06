using System;
using System.Collections.Generic;
using ZaibatsuPass.TransitCard.ORCA;

namespace ZaibatsuPass.TransitCard
{
    abstract class TransitCard
    {

        /// <summary>
        /// Name of the transit card (Clipper, ORCA, etc)
        /// </summary>
        public abstract String Name { get; }
        /// <summary>
        /// Serial number on the card
        /// </summary>
        public virtual String SerialNumber { get { return "Unknown"; } }
        /// <summary>
        /// Balance as formatted in the locale which it comes from
        /// </summary>
        public virtual String Balance { get { return "Unknown"; } }
        /// <summary>
        /// The list of transit events.
        /// 
        /// The default implementation returns an empty list. You should return events that make sense.
        /// </summary>
        public virtual List<TransitEvent> Events { get { return new List<TransitEvent>(); } }

        protected PhysicalCard.PhysicalCard mCard = null;

        public virtual Boolean isStub { get { return true; } }
        public virtual Boolean hasEvents { get { return false; } }
        public virtual Boolean hasExtras { get { return false; } }

        /// <summary>
        /// This is to make sure that every TransitCard implementation has a constructor.
        /// What this gives is a member, mCard, which is available to children of the implementation so that they are not required to remember where they put it...
        /// </summary>
        /// <param name="c">a PhysicalCard snapshot.</param>
        protected TransitCard(PhysicalCard.PhysicalCard c) { mCard = c; }
    }
}
