using System.Collections.Generic;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Repositories
{
    public interface IUsersRepository : IRepository<User>
    {
        IEnumerable<User> GetAll();
        void Create(IEnumerable<User> users);
        void SaveAll(IEnumerable<User> users);
    }
}
