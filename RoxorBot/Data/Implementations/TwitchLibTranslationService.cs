using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Model;
using TwitchLib.Enums;

namespace RoxorBot.Data.Implementations
{
    public class TwitchLibTranslationService : ITwitchLibTranslationService
    {
        public Role TranslateUserType(UserType type)
        {
            switch (type)
            {
                case UserType.Viewer:
                    return Role.Viewers;
                case UserType.Moderator:
                    return Role.Moderators;
                case UserType.GlobalModerator:
                    return Role.GlobalMods;
                case UserType.Broadcaster:
                    return Role.Broadcaster;
                case UserType.Admin:
                    return Role.Admins;
                case UserType.Staff:
                    return Role.Saff;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
