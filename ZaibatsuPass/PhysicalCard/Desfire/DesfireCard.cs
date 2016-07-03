using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZaibatsuPass.PhysicalCard.Desfire
{
    public class DesfireCard : ZaibatsuPass.PhysicalCard.PhysicalCard
    {

        public Pcsc.Desfire.CardDetails cardDetails { get; protected set; }
        

        private DesfireApplication[] mApplications;

        public DesfireApplication getApplication(byte[] aid)
        {
            foreach(DesfireApplication app in mApplications)
            {
                if (app.ApplicationID.SequenceEqual(aid))
                    return app;
            }
            return null;
        }

        private DesfireCard(DesfireApplication[] apps)
        {
            mApplications = apps;
        }

        public static async Task<DesfireCard> SnapshotAsync(Pcsc.Desfire.AccessHandler handler) {

            Pcsc.Desfire.CardDetails cd = await handler.ReadCardDetailsAsync();

            // get the list of applications
            byte[][] __appIDs = await handler.GetApplicationIds();

            // Holder for the apps
            DesfireApplication[] t_apps = new DesfireApplication[__appIDs.Length];

            int appIdx = 0;

            foreach(byte[] __appID in __appIDs)
            {
                System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Getting details on AID " + BitConverter.ToString(__appID));
                await handler.SelectApplicationAsync(__appID);
                System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Getting Files for AID " + BitConverter.ToString(__appID));
                byte[] __fileIDs = await handler.getFiles(__appID);

                Desfire.File.DesfireFile[] t_files = new File.DesfireFile[__fileIDs.Length];
                int fileIdx = 0;
                foreach(byte __fileID in __fileIDs)
                {
                    System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Get settings for file" + __fileID);
                    byte[] __settings = await handler.GetFileSettingsAsync(__fileID);
                    Desfire.File.Settings.DesfireFileSettings p_settings = Desfire.File.Settings.DesfireFileSettings.Parse(__settings);
                    System.Diagnostics.Debug.WriteLine("Got file settings "+ p_settings.GetType().ToString(),"ZaibatsuPass");

                    if (p_settings is File.Settings.RecordSettings)
                    {
                        // Records are a little different. They're special in the land of DESfire cards and need to be read differently.
                        // We'll be reading a bunch of records in bulk, actually.

                        File.Settings.RecordSettings recSettings = p_settings as File.Settings.RecordSettings;
                        DesfireRecord[] records = new DesfireRecord[recSettings.CurRecords];
                        for (int __fileRecordIdx = 0; __fileRecordIdx < recSettings.CurRecords; __fileRecordIdx++)
                        {
                            System.Diagnostics.Debug.WriteLine("Getting {0:X} record {1}", __fileID, __fileRecordIdx);
                            byte[] tRec = await handler.ReadRecordAsync(__fileID, (ulong)__fileRecordIdx, 1);
                            if(tRec == null) { break; } // Whoops.

                            records[__fileRecordIdx] = new DesfireRecord(tRec);

                            System.Diagnostics.Debug.WriteLine("Got {0} bytes of data for {1}", tRec.Length, __fileRecordIdx);
                        }
                        t_files[fileIdx] = new File.RecordFile(__fileID, recSettings, records);

                    }
                    else
                    {
                        // ... Business as usual ...
                        System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Get content of file " + __fileID);
                        byte[] __data = await handler.ReadDataAsync(__fileID);
                        if (__data != null)
                        {
                            Desfire.File.DesfireFile __tFile = Desfire.File.DesfireFile.parse(__fileID, p_settings, __data);
                            t_files[fileIdx] = __tFile;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("ZaibatsuPass: ??? DATA WAS NULL ???");
                            t_files[fileIdx] = new File.InvalidFile(__fileID);
                        }
                    }
                    fileIdx++;
                }
                DesfireApplication __tApp = new DesfireApplication(__appID, t_files);
                t_apps[appIdx] = __tApp;
                appIdx++;
            }

            DesfireCard __tCard = new DesfireCard(t_apps);
            __tCard.cardDetails = cd;
            return __tCard;
            
        }

    }
}
