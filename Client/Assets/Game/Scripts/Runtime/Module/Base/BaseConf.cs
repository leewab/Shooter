using System.Collections.Generic;
using Newtonsoft.Json;

namespace GameConfig
{
    public enum ColorType
    {
        Red    = 0,   
        Green  = 1,   
        Blue   = 2,   
        Yellow = 3,
        Purple = 4,
        Orange = 5,
        None
    }
    
    public class BaseConf
    {
        [JsonProperty("Id")]
        public int Id { get; set; } = 0;

        public static T GetConf<T>(int id) where T : BaseConf
        {
            string confName = typeof(T).Name;
            return ConfManager.Instance.GetConfig<T>(confName, id);
        }

        public static Dictionary<int, BaseConf> GetAllConf<T>() where T : BaseConf
        {
            string confName = typeof(T).Name;
            return ConfManager.Instance.GetAllConf(confName);
        }
        
    }
}