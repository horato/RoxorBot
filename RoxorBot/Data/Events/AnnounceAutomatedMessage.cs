using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Model;

namespace RoxorBot.Data.Events
{
    public class AnnounceAutomatedMessage : PubSubEvent<AutomatedMessageWrapper>
    {
    }
}
