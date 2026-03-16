using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Library.Hubs
{
    public class LibraryHub : Hub
    {
        private static ConcurrentDictionary<int, HashSet<string>> UserConnections = 
            new ConcurrentDictionary<int, HashSet<string>>();

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    UserConnections.AddOrUpdate(userId,
                        new HashSet<string> { Context.ConnectionId },
                        (key, existingSet) =>
                        {
                            existingSet.Add(Context.ConnectionId);
                            return existingSet;
                        });

                    Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnConnectedAsync: {ex.Message}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    if (UserConnections.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(Context.ConnectionId);

                        if (connections.Count == 0)
                        {
                            UserConnections.TryRemove(userId, out _);
                            Console.WriteLine($"User {userId} fully disconnected");
                        }
                    }

                    Console.WriteLine($"User {userId} disconnected from connection ID: {Context.ConnectionId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnDisconnectedAsync: {ex.Message}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task NotifyBookAdded(int userId, object book)
        {
            await BroadcastToUser(userId, "BookAdded", book);
        }

        public async Task NotifyBookUpdated(int userId, object book)
        {
            await BroadcastToUser(userId, "BookUpdated", book);
        }

        public async Task NotifyBookDeleted(int userId, object bookId)
        {
            await BroadcastToUser(userId, "BookDeleted", bookId);
        }

            private async Task BroadcastToUser(int userId, string methodName, object data)
        {
            if (UserConnections.TryGetValue(userId, out var connectionIds))
            {
                await Clients.Clients(connectionIds.ToList()).SendAsync(methodName, data);
                Console.WriteLine($"Broadcasted {methodName} to {connectionIds.Count} connections of user {userId}");
            }
            else
            {
                Console.WriteLine($"No active connections found for user {userId}");
            }
        }

        public Task<IEnumerable<int>> GetActiveUsers()
        {
            return Task.FromResult(UserConnections.Keys.AsEnumerable());
        }
    }
}
