﻿namespace RoxorBot.Data.Interfaces
{
    public interface IPointsManager
    {
        void AddPoints(string user, int points);
        void RemovePoints(string user, int points);
        void SetPoints(string user, int points, bool dbUpdate = true);
        bool UserExists(string name);
        int GetPointsForUser(string name);
        int GetUsersCount();
    }
}
