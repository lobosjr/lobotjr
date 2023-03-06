using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LobotJR.Shared.Utility
{
    public static class SerializerSettings
    {
        public static readonly JsonSerializerSettings Default = new JsonSerializerSettings()
        {
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy(true, false, true),
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }
}
