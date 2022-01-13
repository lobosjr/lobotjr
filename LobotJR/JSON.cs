using System.Collections.Generic;


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

public class SubscriberData
{
    public class Data
    {
        public string broadcaster_id { get; set; }
        public string broadcaster_login { get; set; }
        public string broadcaster_name { get; set; }
        public string gifter_id { get; set; }
        public string gifter_login { get; set; }
        public string gifter_name { get; set; }
        public bool is_gift { get; set; }
        public string tier { get; set; }
        public string plan_name { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_login { get; set; }
    }

    public class Pagination
    {
        public string cursor { get; set; }
    }

    public class RootObject
    {
        public List<Data> data { get; set; }
        public Pagination pagination { get; set; }
        public int total { get; set; }
        public int points { get; set; }
    }
}
