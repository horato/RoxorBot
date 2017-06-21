﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories
{
    public interface IAutomatedMessageWrapperFactory
    {
        AutomatedMessageWrapper CreateNew(AutomatedMessage model);
        AutomatedMessageWrapper CreateNew(string text, int interval, bool enabled);
    }
}
