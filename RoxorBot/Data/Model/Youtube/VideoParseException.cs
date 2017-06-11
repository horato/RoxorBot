using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model.Youtube
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
