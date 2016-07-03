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
        public abstract String SerialNumber { get; }
        /// <summary>
        /// Balance as formatted in the locale which it comes from
        /// </summary>
        public abstract String Balance { get; }
        /// <summary>
        /// 
        /// </summary>
        public abstract List<TransitEvent> Events { get; }

        protected PhysicalCard.PhysicalCard mCard = null;

        /// <summary>
        /// This is to make sure that every TransitCard implementation has a constructor.
        /// What this gives is a member, mCard, which is available to children of the implementation so that they are not required to remember where they put it...
        /// </summary>
        /// <param name="c">a PhysicalCard snapshot.</param>
        protected TransitCard(PhysicalCard.PhysicalCard c) { mCard = c; }
    }
}
