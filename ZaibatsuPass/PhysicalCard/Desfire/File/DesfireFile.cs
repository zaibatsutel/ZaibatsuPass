using System;
using ZaibatsuPass.PhysicalCard.Desfire.File.Settings;

namespace ZaibatsuPass.PhysicalCard.Desfire.File
{
    public class DesfireFile
    {
        public byte ID { get; private set; }
        public DesfireFileSettings Settings { get; private set; }
        public byte[] Data { get; private set; }

        internal DesfireFile(byte i, Settings.DesfireFileSettings s, byte[] d )
        {
            ID = i;
            Settings = s;
            Data = d;
        }

        public static DesfireFile parse(byte i, DesfireFileSettings s, byte[] d)
        {
            System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Attempting to parse file {0:X} with settings type {0}", i, s.GetType().ToString());
            if (s is Desfire.File.Settings.ValueSettings)
                return new ValueFile(i, s as Settings.ValueSettings, d);
            else if (s is RecordSettings)
                return new RecordFile(i, s as Settings.RecordSettings, d);
            else if (s is StandardSettings)
                return new DesfireFile(i, s, d);
            else if (s is InvalidSettings || d == null) 
                return new InvalidFile(i);
            else
                throw new ArgumentException("Unknown settings kind???");
        }

    }

    public class InvalidFile : DesfireFile
    {
        public InvalidFile(byte i): base(i, null,new byte[] { })
        {

        }
    }

    public class ValueFile : DesfireFile
    {
        public int Value { get; private set; }
        public ValueFile(byte i, Settings.ValueSettings s, byte[] d) :base(i,s,d)
        {
            Value = d.MakeUint32();
        }
    }

    public class RecordFile : DesfireFile
    {
        private DesfireRecord[] files;

        public System.Collections.Generic.List<DesfireRecord> Records
        {
            get
            {
                return new System.Collections.Generic.List<DesfireRecord>(files);
            }
        }

        public DesfireRecord this[int idx]
        {
            get {
                if (files.Length < idx) return null;
                else return this.files[idx];
            }
        }

        public RecordFile(byte i, RecordSettings s, DesfireRecord[] records) : base(i,s,new byte[] { })
        {
            files = records;
        }

        public RecordFile(byte i, Settings.RecordSettings s, byte[] d) : base(i,s,d)
        {
            files = new DesfireRecord[s.CurRecords];
            // curRecords -1 because NXP engineers tried to be clever. 
            for(int idx = 0; idx < s.CurRecords; idx++)
            {
                byte[] tmp = new byte[s.RecordSize];
                Array.Copy(d, idx * s.RecordSize, tmp, 0, s.RecordSize);
                files[idx] = new DesfireRecord(tmp);
            }
        }
    }



}
