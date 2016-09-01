using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListService.DAL
{
    public interface IUserTaskRepository
    {
        IEnumerable<Controllers.UserTask> GetForUser(string userId);
        void AddForUser(string userId, Controllers.UserTask userTask);
    }
}
