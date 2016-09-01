using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListWebApp.Models
{
    public class Task
    {
        public Guid TaskId { get; set; }
        public string UserId { get; set; }
        public string TaskName { get; set; }
        public string CurrentState { get; set; }
        public DateTime DueDate { get; set; }
    }
}
