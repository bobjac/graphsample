using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListService.Controllers;

namespace TodoListService.DAL
{
    public class UserTaskRepository : IUserTaskRepository
    {
        private static ConcurrentDictionary<string, List<UserTask>> _tasks =
              new ConcurrentDictionary<string, List<UserTask>>();

        public void AddForUser(string userId, UserTask task)
        {
            bool successfullyAdded = true;
            if (!_tasks.ContainsKey(userId))
            {
                successfullyAdded = _tasks.TryAdd(userId, new List<UserTask>(CreateUserTasks(userId)));
            }

            if (successfullyAdded)
            { 
                List<UserTask> userTasks = _tasks[userId];

                if (userTasks == null)
                {
                    userTasks = new List<UserTask>();
                }

                userTasks.Add(task);

                _tasks[userId] = userTasks;
            }
        }

        public IEnumerable<UserTask> GetForUser(string userId)
        {
            if (!_tasks.ContainsKey(userId))
            {
                bool successfullyAdded = _tasks.TryAdd(userId, new List<UserTask>(CreateUserTasks(userId)));
            }

            List<UserTask> userTasks = _tasks[userId];

            if (userTasks == null)
            {
                userTasks = new List<UserTask>();
                _tasks[userId] = userTasks;
            }
            
            return userTasks;
        }

        private IEnumerable<UserTask> CreateUserTasks(string signedInUserID)
        {
            UserTask[] tasks = new UserTask[]
                {
                    new Controllers.UserTask { CurrentState = "New", DueDate = DateTime.Now.AddDays(5), TaskId = Guid.NewGuid(), TaskName = "Task1", UserId = signedInUserID },
                    new UserTask { CurrentState = "InProcess", DueDate = DateTime.Now.AddDays(7), TaskId = Guid.NewGuid(), TaskName = "Task1", UserId = signedInUserID }
                };
            return tasks;
        }
    }
}
