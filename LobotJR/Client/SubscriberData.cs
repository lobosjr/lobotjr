using System.Collections.Generic;

namespace LobotJR.Client
{
#pragma warning disable IDE1006 // Naming Styles
    public class SubscriberData
    {
        public class Links
        {
            public string next { get; set; }
            public string self { get; set; }
        }

        public class Links2
        {
            public string self { get; set; }
        }

        public class User
        {
            public int _id { get; set; }
            public object logo { get; set; }
            public bool staff { get; set; }
            public string created_at { get; set; }
            public string name { get; set; }
            public string updated_at { get; set; }
            public string display_name { get; set; }
            public Links2 _links { get; set; }
        }

        public class Links3
        {
            public string self { get; set; }
        }

        public class Subscription
        {
            public string _id { get; set; }
            public User user { get; set; }
            public string created_at { get; set; }
            public Links3 _links { get; set; }
        }

        public class RootObject
        {
            public string _cursor { get; set; }
            public int _total { get; set; }
            public Links _links { get; set; }
            public List<Subscription> subscriptions { get; set; }
        }
    }
#pragma warning restore IDE1006 // Naming Styles
}
