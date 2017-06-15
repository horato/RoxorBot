using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using TwitchLib.Enums;

namespace RoxorBot.Data.Interfaces
{
    public interface ITwitchLibTranslationService
    {
        Role TranslateUserType(UserType type);
    }
}
