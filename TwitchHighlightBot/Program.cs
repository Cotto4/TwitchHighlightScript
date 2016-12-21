using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchCSharp.Clients;
using TwitchCSharp.Models;
using dConsole = System.Diagnostics.Debug;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TwitchHighlightBot
{
    class Program
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetActiveWindow(IntPtr hWnd);

        static void Main(string[] args)
        {

            string videoID = "v107506782";
            string channelName = "esl_lol";


            var tClient = new TwitchAuthenticatedClient("occaehrea1cvfkhlqt56zxwrgsjw0k", "1th49vlc8vwrff20q510icedpb23ds");
            //TwitchList<Video> v = tClient.GetChannelVideos("imaqtpie", true, false);
            
            
            Video tVideo = tClient.GetVideo(videoID);
            double startTime = tVideo.RecordedAt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            
            List<double> interestingMoments = new List<double>();
            List<long> allMessages = new List<long>();
            int numChunks = (int)tVideo.Length / 30; // Get 30 second chunks for chat query
            for (int i = 0; i < numChunks; i++)
            {
                var client = new WebClient();
                //https://discuss.dev.twitch.tv/t/getting-chat-replay-transcript/5295
                var stream = client.OpenRead("https://rechat.twitch.tv/rechat-messages?start=" + startTime + "&video_id=" + tVideo.Id);
                var reader = new StreamReader(stream);
                TwitchChatReplay oChatReplay = Newtonsoft.Json.JsonConvert.DeserializeObject<TwitchChatReplay>(reader.ReadLine());
                foreach (var k in oChatReplay.data)
                {
                    allMessages.Add(k.attributes.timestamp);
                }
                Console.WriteLine((((float)i / numChunks) * 100).ToString() + "%");
                
                startTime += 30;
            }
            int increment = 5000; // 5 second granularity?
            /***************************/
            //Don't look at this, scary
            int p = 0;
            List<Tuple<int, int>> chunks = new List<Tuple<int, int>>();
            double time = tVideo.RecordedAt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            while(time < allMessages.Max())
            {
                chunks.Add(new Tuple<int, int>(p, allMessages.FindAll(x => x > time && x < (time + increment)).Count));
                time += increment;
                p += increment;
            }
            float perc = (float)(0.99) * (1 + chunks.Count); //99th percentile
            List<Tuple<int,int>> SortedList = chunks.OrderBy(o => o.Item2).ToList();
            int percVal = SortedList.ElementAt((int)perc).Item2;
            SortedList.RemoveRange(0, (SortedList.Count - percVal - 1)); // list is now only items we want
            SortedList = SortedList.OrderBy(o => o.Item1).ToList();
            List<int> timestamps = new List<int>();
            foreach (var tuple in SortedList)
            {
                timestamps.Add(tuple.Item1);
            }
            timestamps = timestamps.OrderBy(o => o).ToList();
            foreach (var c in timestamps.ToList())
            {
                int t = c;
                List<int> toRemove = new List<int>();
                int timeOfClip = 0;
                while (timestamps.Contains((t))) {
                    timeOfClip += increment;
                    toRemove.Add(t);
                    t += increment;
                }
                if (timeOfClip >= (2*increment))
                {
                    //big enough to warrant a highlight yo
                    TimeSpan t2 = TimeSpan.FromMilliseconds(c - (2 * increment));
                    dConsole.WriteLine("http://www.twitch.tv/" + channelName + "/v/" + tVideo.Id.Substring(1) + "?t=0" + t2.Hours + "h" + t2.Minutes + "m" + t2.Seconds + "s - Lasting " + (timeOfClip + (2 * increment))/1000 + "s" );
                    Process proc = System.Diagnostics.Process.Start("http://www.twitch.tv/" + channelName + "/v/" + tVideo.Id.Substring(1) + "?t=0" + t2.Hours + "h" + t2.Minutes + "m" + t2.Seconds + "s");
                    IntPtr ptr = proc.MainWindowHandle;
                    //SetActiveWindow(ptr);
                    //System.Threading.Thread.Sleep(10000);
                    //System.Windows.Forms.SendKeys.SendWait("%{X}");
                    
                }
                timestamps.RemoveAll(x => toRemove.Contains(x));
            }

            
            //foreach (Video a in v.List)
            //{
            //    //QUERY CHAT USING https://rechat.twitch.tv/rechat-messages?start=TIMESTAMP&video_id=VOD_ID
            // ONLY GETTING 10 VIDEOS PER CHANNEL, NEED TO FIND WAY TO CHANGE DEFAULT
            //    dConsole.WriteLine(a.Url);
            //    dConsole.WriteLine("");
            //    dConsole.WriteLine(a.RecordedAt.ToString());
            //    dConsole.WriteLine(a.RecordedAt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds);
            //}
        }

        public static DateTime convertTimestamp (long t)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(t));
        }

        public static DateTime convertTimestamp(double t)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(t));
        }
    }
}
