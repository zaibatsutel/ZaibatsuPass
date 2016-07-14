using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass
{
    static class StringExtensions
    {

        public static char CalculateLuhnChecksum(this string self)
        {
            int sum = 00;
            // get the Luhn checksum
            for(int idx=0; idx < self.Length; idx++)
            {
                int tmp;
                bool result = int.TryParse("" + self[idx], out tmp);
                if (!result) throw new FormatException("String is not exclusively series of numeric values: " + self);

                if ((idx & 0x01) == 00)
                {
                    // Odd digit. Simply add the parsed value
                    sum += tmp;
                }
                else
                {
                    // even digit. Double it
                    int tmp2 = tmp * 2;
                    if (tmp2 > 9)
                    {
                        // get the two digits and add to the sum individually.
                        // we can cheat here because the maximum value we'll get out is 9*2 (18) which means we can do integer division
                        // and get the 10's place, subtract 10 and get the ones place. 
                        sum += ( tmp2 / 10 ) + ( tmp2 - 10 );

                    }
                    else
                        sum += tmp2;
                }
            }



            // now that we have the sum, we mod by 10 (

            int check = 10 - (sum % 10);

            return (check.ToString()[0]);
        }

    }
}
