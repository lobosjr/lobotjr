using System.Collections.Generic;

namespace LobotJR.Client
{
#pragma warning disable IDE1006 // Naming Styles
    public class Data
    {
        public int chatter_count { get; set; }
        public Chatters chatters { get; set; }


        public class Chatters
        {
            public List<string> vips { get; set; }
            public List<string> moderators { get; set; }
            public List<string> staff { get; set; }
            public List<string> viewers { get; set; }
            public List<string> admins { get; set; }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
