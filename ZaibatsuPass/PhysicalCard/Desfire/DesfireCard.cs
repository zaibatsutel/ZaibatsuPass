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
                    System.Diagnostics.Debug.WriteLine("ZaibatsuPass: Get content of file " + __fileID);
                    byte[] __data = await handler.ReadDataAsync(__fileID);
                    if (__data != null)
                    {
                        Desfire.File.DesfireFile __tFile = Desfire.File.DesfireFile.parse(__fileID, p_settings, __data);
                        t_files[fileIdx] = __tFile;
                    }
                    else
                    {
                        t_files[fileIdx] = new File.InvalidFile(__fileID);
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
