using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emotiv;

namespace EmotyperDataUtility
{
   
    class EmotivHeadsetDataProvider
    {
        private static EmoEngine engine; // Access to the EDK is viaa the EmoEngine 
        private static int userID = -1; // userID is used to uniquely identify a user's headset
        private static void initEngine()
        {
            engine = EmoEngine.Instance;
            engine.UserAdded += new EmoEngine.UserAddedEventHandler(engine_UserAdded_Event);
            // connect to Emoengine.            
            engine.Connect();
        }

        static void engine_UserAdded_Event(object sender, EmoEngineEventArgs e)
        {
            Console.WriteLine("User Added Event has occured");
            // record the user 
            userID = (int)e.userId;
            // enable data aquisition for this user.
            engine.DataAcquisitionEnable((uint)userID, true);
            // ask for up to 1 second of buffered data
            engine.EE_DataSetBufferSizeInSec(1);
        }
        static  Dictionary<EdkDll.EE_DataChannel_t, double[]>  GetData()
        {
            if (engine == null)
            {
                initEngine();
            }
            // Handle any waiting events
            engine.ProcessEvents();
            // If the user has not yet connected, do not proceed
            if ((int)userID == -1)
                return null;
            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = engine.GetData((uint)userID);
            return data;
        }
    }
}
