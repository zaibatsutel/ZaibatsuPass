using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.PhysicalCard.Desfire.File
{
    public enum DesfireFileType : Byte
    {
        StandardData= 0x00,
        Backup = 0x01,
        Value = 0x02,
        LinearRecord = 0x03,
        CyclicRecord = 0x04 
    }
}
