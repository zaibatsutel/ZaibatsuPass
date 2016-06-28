using System;
using ZaibatsuPass.PhysicalCard.Desfire.File;

namespace ZaibatsuPass.PhysicalCard.Desfire
{
    public class DesfireApplication
    {
        public byte[] ApplicationID { get; set; }
        private DesfireFile[] mFiles;

        public DesfireApplication(byte[] aid, DesfireFile[] files)
        {
            ApplicationID = aid;
            mFiles = files;
        }

        public DesfireFile getFile(byte id)
        {
            foreach(DesfireFile file in mFiles)
            {
                if (file.ID == id) return file;
            }
            throw new ArgumentException("Invalid file ID");
        }


    }
}
