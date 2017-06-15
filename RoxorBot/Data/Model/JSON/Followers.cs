namespace RoxorBot.Data.Model.JSON
{
    public class Followers
    {
        public string created_at { get; set; }
        public Links _links { get; set; }
        public bool notifications { get; set; }
        public object channel { get; set; }
        public string error { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}
