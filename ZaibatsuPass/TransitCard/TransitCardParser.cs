using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.TransitCard
{
    class TransitCardParser
    {

        internal sealed class TransitCardFilter
        {
            public Func<PhysicalCard.PhysicalCard, bool> filterFunc;
            public Type InstanciationType;
            public TransitCardFilter(Type t,Func<PhysicalCard.PhysicalCard,bool> f)
            {
                InstanciationType = t;
                filterFunc = f;
            }
        }

        static List<TransitCardFilter> filters = new List<TransitCardFilter>()
        {
            new TransitCardFilter(typeof(ORCA.ORCACard), (PhysicalCard.PhysicalCard f) => {return (f is PhysicalCard.Desfire.DesfireCard) && ((PhysicalCard.Desfire.DesfireCard)f).getApplication(ORCA.ORCACard.AID_ORCA)!=null; } ),

        };

        public static TransitCard parseTransitCard(PhysicalCard.PhysicalCard card)
        {
            foreach(TransitCardFilter f in filters)
            {
                if (f.filterFunc(card)) return (TransitCard)(Activator.CreateInstance(f.InstanciationType, card));
            }

            return null;
        }

    }
}
