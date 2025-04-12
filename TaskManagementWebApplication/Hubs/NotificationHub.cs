using Microsoft.AspNetCore.SignalR;
using TaskManagement.Models;

namespace TaskManagement.Hubs
{
    public class NotificationHub : Hub
    {
        public async Task SendTaskUpdate(List<TaskItem> tasks)
        {
            await Clients.All.SendAsync("ReceiveTaskUpdate", tasks);
        }
    }
}