using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ZaibatsuPass.PhysicalCard.Desfire.File.Settings
{
    public class DesfireFileSettings
    {
        public DesfireFileType FileType { get; private set; }
        public Byte CommSetting;
        public Byte[] AccessRights;
        protected DesfireFileSettings(System.IO.BinaryReader reader )
        {
            this.FileType = (DesfireFileType)reader.ReadByte();
            this.CommSetting = reader.ReadByte();
            this.AccessRights = reader.ReadBytes(2);
        }
        
        public static DesfireFileSettings Parse(byte[] buffer)
        {
            System.IO.BinaryReader reader = new System.IO.BinaryReader(new System.IO.MemoryStream(buffer));
            

            DesfireFileType ft = (DesfireFileType) (buffer[0]);
            switch(ft)
            {
                case DesfireFileType.StandardData:
                case DesfireFileType.Backup:
                    // It's a standard file...
                    return new StandardSettings(reader);
                case DesfireFileType.LinearRecord:
                case DesfireFileType.CyclicRecord:
                    return new RecordSettings(reader);
                case DesfireFileType.Value:
                    return new ValueSettings(reader);
                default:
                    throw new ArgumentException("Unkonwn type of record.");
            }
        }

    }


    public class StandardSettings : DesfireFileSettings
    {
        public int Filesize { get; private set; }
        public StandardSettings(System.IO.BinaryReader reader) : base(reader)
        {
            byte[] sz = reader.ReadBytes(3);
            this.Filesize = sz.MakeUint24();
        }
    }

    public class RecordSettings:DesfireFileSettings
    {
        public int RecordSize { get; private set; }
        public int MaxRecords { get; private set; }
        public int CurRecords { get; private set; }
        
        public RecordSettings(System.IO.BinaryReader reader) : base(reader)
        {
            byte[] bRecordSize = reader.ReadBytes(3).Reverse().ToArray();
            byte[] bMaxRecords = reader.ReadBytes(3).Reverse().ToArray();
            byte[] bCurRecords = reader.ReadBytes(3).Reverse().ToArray() ;
            RecordSize = bRecordSize.MakeUint24();
            MaxRecords = bMaxRecords.MakeUint24();
            CurRecords = bCurRecords.MakeUint24();
            if (CurRecords > MaxRecords) throw new ArgumentException("Current No of records > Max number of records");
        }
    }

    public class ValueSettings:DesfireFileSettings
    {
        public int LowerLimit { get; private set; }
        public int UpperLimit { get; private set; }
        public int LimitedCreditValue { get; private set; }
        public Boolean LimitedCreditEnabled { get; private set; }

        public ValueSettings(System.IO.BinaryReader reader) : base(reader)
        {
            LowerLimit = reader.ReadBytes(4).MakeUint32();
            UpperLimit = reader.ReadBytes(4).MakeUint32();
            LimitedCreditValue = reader.ReadBytes(4).MakeUint32();
            LimitedCreditEnabled = reader.ReadByte() != 0x00;
        }

    }

    public class InvalidSettings:DesfireFileSettings
    {
        public InvalidSettings(System.IO.BinaryReader reader):base(reader) { }
    }


}
