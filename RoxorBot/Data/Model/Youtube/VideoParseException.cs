using System;

namespace RoxorBot.Data.Model.Youtube
{
    public class VideoParseException : Exception
    {
        public VideoParseException(string message)
            : base(message)
        {

        }
        private VideoParseException()
        { 
        
        }
    }
}
