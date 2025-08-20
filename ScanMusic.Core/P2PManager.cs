using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScanMusic.Core
{
    public class PeerInfo
    {
        public string Ip { get; set; }
        public int Port { get; set; }
    }

    public class P2PManager
    {

        public P2PManager()
        {
            KnownPeers.Add(new PeerInfo { Ip = "127.0.0.1", Port = 8000 });
        }

        {
            await Task.Delay(100);

            {
                results.Add("SampleSong.mp3 (127.0.0.1:8000)");
                results.Add("MusicDemo.wav (127.0.0.1:8000)");
            }
            return results;
        }
    }
}
