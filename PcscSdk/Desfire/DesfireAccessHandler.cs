//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;

using Pcsc;

namespace Pcsc.Desfire
{
    /// <summary>
    /// Access handler class for Desfire based ICC. It provides wrappers for different Desfire 
    /// commands
    /// </summary>
    public class AccessHandler
    {
        /// <summary>
        /// connection object to smart card
        /// </summary>
        private SmartCardConnection connectionObject { set; get; }

        /// <summary>
        /// Desfire command APDU
        /// </summary>
        private DesfireCommand desfireCommand { set; get; }
        
        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="ScConnection">
        /// connection object to a Desfire ICC
        /// </param>
        public AccessHandler(SmartCardConnection ScConnection)
        {
            connectionObject = ScConnection;

            desfireCommand = new DesfireCommand();
        }
        /// <summary>
        /// Read card details commands
        /// </summary>
        /// <returns>
        /// returns Desfire CardDetails object
        /// </returns>
        public async Task<CardDetails> ReadCardDetailsAsync()
        {
            desfireCommand.Command = (byte) DesfireCommand.CommandType.GetVersion;
            desfireCommand.Data = null;

            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;

            if (!desfireRes.SubsequentFrame || desfireRes.ResponseData.Length != 7)
            {
                return null;
            }

            CardDetails card = new CardDetails();

            card.HardwareVendorID = desfireRes.ResponseData[0];
            card.HardwareType = desfireRes.ResponseData[1];
            card.HardwareSubType = desfireRes.ResponseData[2];
            card.HardwareMajorVersion = desfireRes.ResponseData[3];
            card.HardwareMinorVersion = desfireRes.ResponseData[4];
            card.HardwareStorageSize = desfireRes.ResponseData[5];
            card.HardwareProtocolType = desfireRes.ResponseData[6];

            desfireCommand.Command = (byte)DesfireCommand.CommandType.GetAdditionalFrame;
            desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;

            if (!desfireRes.SubsequentFrame || desfireRes.ResponseData.Length != 7)
            {
                // Not expected
                return null;
            }
            card.SoftwareVendorID = desfireRes.ResponseData[0];
            card.SoftwareType = desfireRes.ResponseData[1];
            card.SoftwareSubType = desfireRes.ResponseData[2];
            card.SoftwareMajorVersion = desfireRes.ResponseData[3];
            card.SoftwareMinorVersion = desfireRes.ResponseData[4];
            card.SoftwareStorageSize = desfireRes.ResponseData[5];
            card.SoftwareProtocolType = desfireRes.ResponseData[6];

            desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;

            if (!desfireRes.Succeeded || desfireRes.ResponseData.Length != 14)
            {
                // Not expected
                return null;
            }

            card.UID = new byte[7];
            System.Buffer.BlockCopy(desfireRes.ResponseData, 0, card.UID, 0, 7);

            card.ProductionBatchNumber = new byte[5];
            System.Buffer.BlockCopy(desfireRes.ResponseData, 7, card.ProductionBatchNumber, 0, 5);

            card.WeekOfProduction = desfireRes.ResponseData[12];
            card.YearOfProduction = desfireRes.ResponseData[13];

            return card;
        }

        /// <summary>
        /// Request the list of ICC application IDs from the card.
        /// 
        /// nb: this selects ICC application ID <code>0x00 0x00 0x00</code>
        /// </summary>
        /// <returns>Array of application IDs as unparsed byte IDs</returns>
        public async Task<byte[][]> GetApplicationIds()
        {

            // The first thing we must do is select application id 0x00000

            System.Diagnostics.Debug.WriteLine("PCSCSDK: Attempt to swap to AID 0x000000");
            await SelectApplicationAsync(new byte[] { 0x00, 0x00, 0x00 });

            System.Diagnostics.Debug.WriteLine("PCSCSDK: Swap to AID 0x000000 OK");


            // then, we can request the index of application IDs
            desfireCommand.Command = (byte)DesfireCommand.CommandType.GetApplicationDirectory;
            desfireCommand.Data = null;

            // Transceive and get the response

            System.Diagnostics.Debug.WriteLine("PCSCSDK: Attempt to fetch application directory");

            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
            if(desfireRes.BoundaryError) { throw new Exception("Couldn't get application IDs from desfire card"); }
            System.Diagnostics.Debug.WriteLine("PCSCSDK: Got application directory.");
            // buffer that holds our application IDs.
            byte[] buffer = new byte[desfireRes.ResponseData.Length];
            // copy the recieved information into our buffer. We won't work on them *quite* yet.
            Array.Copy(desfireRes.ResponseData, buffer, buffer.Length);

            // If there are more than 19 AIDs, there's going to be another frame.
            // Theoretically, there's a maximum of 28 AIDs that can be registered to a card, however
            // it's not entirely clear.
            if(desfireRes.SubsequentFrame)
            {
                System.Diagnostics.Debug.WriteLine("PCSCSDK: Need additional frame for AID list");
                desfireCommand.Command = (byte)DesfireCommand.CommandType.GetAdditionalFrame;
                desfireCommand.Data = null;
                desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
                if (desfireRes.SubsequentFrame)
                {
                    System.Diagnostics.Debug.WriteLine("PCSCSDK: Too many frames for AID list.");
                    // Unexpected: There should be no more than 3 frames.
                    return null;
                }
                // resize the array to be the current size of the array + the size of the responded data.
                Array.Resize(ref buffer, buffer.Length + desfireRes.ResponseData.Length);
                // Now, copy the data from the latest response, offset forward against the current buffer length
                int offset = buffer.Length;
                Array.Copy(desfireRes.ResponseData, 0, buffer, offset, desfireRes.ResponseData.Length);
            }

            // Each AID is 3 bytes
            int count = buffer.Length / 3;
            // There should not be > 28 application IDs.
            if(count > 28) { return null; }
            // a token holder. 
            int aIdx = 0;

            // A horrible type, forged in the bowels of hell.
            byte[][] ids = new byte[count][];

            // Walk the wrapper. 
            while (aIdx < count)
            {
                int idx = aIdx * 3;
                byte[] atmp = new byte[3];
                Array.Copy(buffer, idx, atmp, 0, 3);
                System.Diagnostics.Debug.WriteLine("Got AID " + BitConverter.ToString(atmp));
                ids[aIdx] = atmp;
                aIdx++;
            }
            System.Diagnostics.Debug.WriteLine("Got " + count + " AIDs");
            return ids;

        }
        
        /// <summary>
        /// Select ICC application by AID
        /// </summary>
        /// <param name="aid">
        /// application id
        /// </param>
        public async Task SelectApplicationAsync(byte[] aid)
        {
            // AIDs must be 3 bytes.
            if (aid.Length != 3)
            {
                throw new NotSupportedException();
            }

            desfireCommand.Command = (byte)DesfireCommand.CommandType.SelectApplication;
            desfireCommand.Data = aid;
            DesfireResponse desfireRes = null;
            if (connectionObject == null)
            {
                throw new Exception("??? no connection object ???");
            }
            try
            {
                desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("FUCK:"+e.ToString());
            }
            if(desfireRes == null)
            {
                throw new Exception("??? DesFire response Isn't a thing???");
            }

            // There is no real data to return, but if it didn't succeed, then there's something wrong.
            if (!desfireRes.Succeeded)
            {
                throw new Exception("Failure selecting application, SW=" + desfireRes.SW + " (" + desfireRes.SWTranslation + ")");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aid"></param>
        /// <returns></returns>
        public async Task<byte[]> getFiles(byte[] aid)
        {
            System.Diagnostics.Debug.WriteLine("PCSCSDK: Get files for AID " + BitConverter.ToString(aid));
            await SelectApplicationAsync(aid);
            
            desfireCommand.Command = (byte)(DesfireCommand.CommandType.GetFiles);
            desfireCommand.CommandData = null;
            System.Diagnostics.Debug.WriteLine("PCSCSDK: Fetch file index!");
            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
            if(!desfireRes.Succeeded)
            {
                System.Diagnostics.Debug.WriteLine("PCSCSDK: !!! FAILED TO GET FILE INDEX SW=" + desfireRes.SW + " ( " + desfireRes.SWTranslation + " ) ");
                throw new Exception("Failure listing files SW=" + desfireRes.SW);
            }

            return desfireRes.ResponseData;
            
        }

        public async Task<byte[]> GetFileSettingsAsync(byte fileNumber)
        {
            desfireCommand.Command = (byte)DesfireCommand.CommandType.GetFileSettings;
            desfireCommand.CommandData = new byte[] { fileNumber };

            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
            if(!desfireRes.Succeeded)
            {
                throw new Exception("Failure getting file settings SW=" + desfireRes.SW);
            }
            return desfireRes.ResponseData;
        }

        /// <summary>
        /// Read data command
        /// </summary>
        /// <param name="fileNumber">
        /// </param>
        /// <param name="offset">
        /// </param>
        /// <param name="length">
        /// </param>
        /// <returns>
        /// byte array of read data
        /// </returns>
        public async Task<byte[]> ReadDataAsync(byte fileNumber, ulong offset, ulong length)
        {
            var args = new byte[7];
            args[0] = fileNumber;
            args[1] = (byte)(offset & 0xFF);
            args[2] = (byte)((offset >> 8) & 0xFF);
            args[3] = (byte)((offset >> 16) & 0xFF);
            args[4] = (byte)(length & 0xFF);
            args[5] = (byte)((length >> 8) & 0xFF);
            args[6] = (byte)((length >> 16) & 0xFF);

            desfireCommand.Command = (byte)DesfireCommand.CommandType.ReadData;
            desfireCommand.Data = args;

            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
            
            

            if (!desfireRes.Succeeded)
            {
                throw new Exception("Failure reading data, SW=" + desfireRes.SW + " (" + desfireRes.SWTranslation + ")");
            }

            return desfireRes.ResponseData;
        }
        /// <summary>
        /// Read record comand
        /// </summary>
        /// <param name="fileNumber"></param>
        /// <param name="offset"></param>
        /// <param name="numberOfRecords"></param>
        /// <returns>
        /// byte array of read data
        /// </returns>
        public async Task<byte[]> ReadRecordAsync(byte fileNumber, ulong offset, ulong numberOfRecords)
        {
            var args = new byte[7];
            args[0] = fileNumber;
            args[1] = (byte)(offset & 0xFF);
            args[2] = (byte)((offset >> 8) & 0xFF);
            args[3] = (byte)((offset >> 16) & 0xFF);
            args[4] = (byte)(numberOfRecords & 0xFF);
            args[5] = (byte)((numberOfRecords >> 8) & 0xFF);
            args[6] = (byte)((numberOfRecords >> 16) & 0xFF);

            desfireCommand.Command = (byte)DesfireCommand.CommandType.ReadRecord;
            desfireCommand.Data = args;

            DesfireResponse desfireRes = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;

            if (desfireRes.BoundaryError)
            {
                // Boundary error, the record doesn't exist
                return null;
            }
            if (!desfireRes.Succeeded)
            {
                throw new Exception("Failure reading record, SW=" + desfireRes.SW + " (" + desfireRes.SWTranslation + ")");
            }

            return desfireRes.ResponseData;
        }

        public async Task<byte[]> ReadDataAsync(byte filenumber)
        {
            desfireCommand.Command = 0xBD;
            desfireCommand.CommandData = new byte[] { filenumber, 0, 0, 0, 0, 0, 0 };
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            while (true)
            {
                DesfireResponse response = await connectionObject.TransceiveAsync(desfireCommand) as DesfireResponse;
                if (response.Succeeded || response.SubsequentFrame)
                {
                    ms.Write(response.ResponseData, 0, response.ResponseData.Length);
                }
                if (response.SubsequentFrame)
                {
                    desfireCommand.Command = 0xAF;
                    desfireCommand.CommandData = null;
                    continue;
                }
                else if(response.AccessDenied)
                {
                    System.Diagnostics.Debug.WriteLine("PCSCSDK: Access denied when reading file {0} SW={1:X}", filenumber, response.SW);
                    return null;
                }
                else if (response.Succeeded)
                {
                    return ms.ToArray();
                }
                else
                {
                    throw new Exception("Failed to get file; SW="+response.SW);
                }
            }
        }
    }
    /// <summary>
    /// Class hodls Desfire card details information
    /// </summary>
    public class CardDetails
    {
        public byte HardwareVendorID { get; set; }
        public byte HardwareType { get; set; }
        public byte HardwareSubType { get; set; }
        public byte HardwareMajorVersion { get; set; }
        public byte HardwareMinorVersion { get; set; }
        public byte HardwareStorageSize { get; set; }
        public byte HardwareProtocolType { get; set; }
        public byte SoftwareVendorID { get; set; }
        public byte SoftwareType { get; set; }
        public byte SoftwareSubType { get; set; }
        public byte SoftwareMajorVersion { get; set; }
        public byte SoftwareMinorVersion { get; set; }
        public byte SoftwareStorageSize { get; set; }
        public byte SoftwareProtocolType { get; set; }
        // 7 bytes
        public byte[] UID { get; set; }
        // 5 bytes
        public byte[] ProductionBatchNumber { get; set; }
        public byte WeekOfProduction { get; set; }
        public byte YearOfProduction { get; set; }
        public override string ToString()
        {
            return
                "HardwareVendorID = " + HardwareVendorID.ToString() + Environment.NewLine +
                "HardwareType = " + HardwareType.ToString() + Environment.NewLine +
                "HardwareSubType = " + HardwareSubType.ToString() + Environment.NewLine +
                "HardwareMajorVersion = " + HardwareMajorVersion.ToString() + Environment.NewLine +
                "HardwareMinorVersion = " + HardwareMinorVersion.ToString() + Environment.NewLine +
                "HardwareStorageSize = " + HardwareStorageSize.ToString() + Environment.NewLine +
                "HardwareProtocolType = " + HardwareProtocolType.ToString() + Environment.NewLine +
                "SoftwareVendorID = " + SoftwareVendorID.ToString() + Environment.NewLine +
                "SoftwareType = " + SoftwareType.ToString() + Environment.NewLine +
                "SoftwareSubType = " + SoftwareSubType.ToString() + Environment.NewLine +
                "SoftwareMajorVersion = " + SoftwareMajorVersion.ToString() + Environment.NewLine +
                "SoftwareMinorVersion = " + SoftwareMinorVersion.ToString() + Environment.NewLine +
                "SoftwareStorageSize = " + SoftwareStorageSize.ToString() + Environment.NewLine +
                "SoftwareProtocolType = " + SoftwareProtocolType.ToString() + Environment.NewLine +
                "UID = " + BitConverter.ToString(UID) + Environment.NewLine +
                "ProductionBatchNumber = " + BitConverter.ToString(ProductionBatchNumber) + Environment.NewLine +
                "WeekOfProduction = " + WeekOfProduction.ToString() + Environment.NewLine +
                "YearOfProduction = " + YearOfProduction.ToString() + Environment.NewLine;
        }
    }
}
