using System.ComponentModel;

namespace scp714
{
    public class Config
    {
        public bool IsEnabled { get; set; } = true; 
        
        [Description("SCP-714 works from inventory")]
        public bool InInventory { get; set; }  = true;

        [Description("SCP-714 spawns on the map")]
        public bool CanSpawn { get; set; } = true;

        [Description("How many seconds SCP-714 can be worn before death")]
        public int TimeWear { get; set; } = 120;

    }
}
