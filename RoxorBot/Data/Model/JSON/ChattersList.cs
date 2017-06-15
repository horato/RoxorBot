namespace RoxorBot.Data.Model.JSON
{
    public class ChattersList
    {
        public string[] moderators { get; set; }
        public string[] staff { get; set; }
        public string[] admins { get; set; }
        public string[] global_mods { get; set; }
        public string[] viewers { get; set; }
    }
}
