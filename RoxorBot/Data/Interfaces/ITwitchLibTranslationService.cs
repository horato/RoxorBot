using RoxorBot.Data.Enums;
using TwitchLib.Enums;

namespace RoxorBot.Data.Interfaces
{
    public interface ITwitchLibTranslationService
    {
        Role TranslateUserType(UserType type);
    }
}
