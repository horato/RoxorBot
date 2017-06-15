namespace RoxorBot.Data.Model
{
    public class FilterItem
    {
        public int id { get; set; }
        public string word { get; set; }
        //TODO: change to timespan
        public int duration { get; set; }
        public string addedBy { get; set; }
        public bool isRegex { get; set; }
        public bool isWhitelist { get; set; }
    }
}
