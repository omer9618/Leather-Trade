using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LTMS
{
    public class NotificationHub : Hub
    {
        // Send notification to a specific user
        public async Task SendNotification(string userId, string title, string message, string referenceId)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", title, message, referenceId);
        }
    }
} 