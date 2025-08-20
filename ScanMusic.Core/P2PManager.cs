using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScanMusic.Core
{
    public class PeerInfo
    {
        public string Ip { get; set; }
        public int Port { get; set; }
        public List<string> SharedFiles { get; set; } = new List<string>();
    }

    public class P2PManager
    {
        public List<PeerInfo> KnownPeers { get; private set; } = new List<PeerInfo>();

        public P2PManager()
        {
            KnownPeers.Add(new PeerInfo { Ip = "127.0.0.1", Port = 8000 });
        }

        public async Task<List<string>> SearchAsync(string query)
        {
            var results = new List<string>();
            await Task.Delay(100);

            if (!string.IsNullOrEmpty(query) && query.Length > 2)
            {
                results.Add("SampleSong.mp3 (127.0.0.1:8000)");
                results.Add("MusicDemo.wav (127.0.0.1:8000)");
            }

            return results;
        }
    }
}