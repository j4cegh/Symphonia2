using DiscordRPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Symphonia2
{
    public class DiscordRPC
    {
        public DiscordRpcClient client;
        public void Init()
        {
            Thread rpcThreadBase = new Thread(new ThreadStart(() =>
            {
                Constants constants = new Constants();
                constants.initVersion();
                client = new DiscordRpcClient("897430409355350057");



                //Subscribe to events
                client.OnReady += (sender, e) =>
                {
                };

                client.OnPresenceUpdate += (sender, e) =>
                {
                };

                //Connect to the RPC
                client.Initialize();
                client.SetPresence(new RichPresence()
                {
                    Details = "Just launched!",
                    State = "About to play some tunes?",
                    Assets = new Assets()
                    {
                        LargeImageKey = "symphony",
                        LargeImageText = "Symphonia",
                        SmallImageKey = "symphony",
                        SmallImageText = "Build " + constants.build
                    }
                });
            }));
            rpcThreadBase.Start();
            


        }
    }
}
