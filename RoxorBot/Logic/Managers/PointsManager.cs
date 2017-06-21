using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Model;

namespace RoxorBot.Logic.Managers
{
    public class PointsManager : IPointsManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;

        public PointsManager(IEventAggregator aggregator, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _usersManager = usersManager;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing PointsManager...");
        }

        public void AddPoints(string user, int points)
        {
            SetPoints(user, GetPointsForUser(user) + points);
        }

        public void RemovePoints(string user, int points)
        {
            if (!UserExists(user))
                return;

            if (GetPointsForUser(user) < points)
                SetPoints(user, 0);
            else
                SetPoints(user, GetPointsForUser(user) - points);
        }

        public void SetPoints(string user, int points, bool dbUpdate = true)
        {
            var u = _usersManager.GetUser(user);
            if (u == null)
                u = _usersManager.AddOrGetUser(user, Role.Viewers);

            u.Points = points;
            if (dbUpdate)
                _usersManager.Save(u);
        }

        public bool UserExists(string name)
        {
            return (_usersManager.GetUser(name) != null);
        }

        public int GetPointsForUser(string name)
        {
            var user = _usersManager.GetUser(name);
            if (user == null)
                return 0;
            else
                return user.Points;
        }

        public int GetUsersCount()
        {
            return _usersManager.UsersCount;
        }
    }
}
