using System;

namespace zapierfx.Models
{
    public class MetricsEntry
    {
        public DateTime Date { get; set; }
        public int AdBreaks { get; set; }
        public float AvgViewers { get; set; }
        public int ChatMessages { get; set; }
        public int Chatters { get; set; }
        public int ClipViews { get; set; }
        public int ClipsCreated { get; set; }
        public int Followers { get; set; }
        public int LiveViews { get; set; }
        public int UniqueViews { get; set; }
        public int MaxViewers { get; set; }
        public int MinsWatched { get; set; }
        public int MinsStreamed { get; set; }
        public string Channel { get; set; }
    }
}