using System;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IFilterProvider
    {
        Filter GetAutomatedMessage(Guid id);
        Filter CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
    }
}
