using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Symphonia2
{
    public class ThreadingRPC
    {
        public void SetRPC(DiscordRPC drpc, string Details, string State, Constants constants)
        {
            Thread rpcThread = new Thread(new ThreadStart(() =>
            {
                drpc.client.SetPresence(new RichPresence()
                {
                    Details = Details,
                    State = State,
                    Assets = new Assets()
                    {
                        LargeImageKey = "symphony",
                        LargeImageText = "Symphonia",
                        SmallImageKey = "symphony",
                        SmallImageText = "Build " + constants.build
                    }
                });
            }));
            rpcThread.Start();
            
        }
    }
}
