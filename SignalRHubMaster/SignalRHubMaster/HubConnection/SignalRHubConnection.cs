using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRHubMaster.HubConnection
{
    public class SignalRHubConnection : Hub
    {
        // Thread-safe collection to store connected users
        private static ConcurrentDictionary<string, string> ConnectedUsers = new ConcurrentDictionary<string, string>();

        // Method to handle user login and subscription
        public async Task Login(string username)
        {
            // Add the user to the connected users list with their connection ID
            ConnectedUsers[Context.ConnectionId] = username;

            // Notify all clients about the updated user list
            await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values);

            // Notify all clients about the user login
            await Clients.All.SendAsync("UserLoggedIn", username);
        }

        // Method to send a message to all clients
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", DateTime.Now.ToShortTimeString(), user, message);
        }

        // Handle user disconnects
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Remove the user from the connected users list
            if (ConnectedUsers.TryRemove(Context.ConnectionId, out var username))
            {
                // Notify all clients about the updated user list
                await Clients.All.SendAsync("UpdateUserList", ConnectedUsers.Values);

                // Notify all clients about the user logout
                await Clients.All.SendAsync("UserLoggedOut", username);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
