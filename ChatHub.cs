using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LTMS
{
    public class ChatHub : Hub
    {
        // Send message to a specific group (Bid or Order context)
        public async Task SendMessage(string groupName, string senderId, string receiverId, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", senderId, receiverId, message);
        }

        // Join a group (Bid or Order context)
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // Leave a group (optional)
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
} 