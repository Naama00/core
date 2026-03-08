using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Library.Hubs
{
    public class LibraryHub : Hub
    {
        // דיקשנרי להעתקה בין userId לרשיctionIds עבור חיבורים פעילים שלו
        private static ConcurrentDictionary<int, HashSet<string>> UserConnections = 
            new ConcurrentDictionary<int, HashSet<string>>();

        /// <summary>
        /// נקרא כאשר משתמש מתחבר ל-Hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    // הוסף את ה-connectionId לקבוצה של זה המשתמש
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

        /// <summary>
        /// נקרא כאשר משתמש מתנתק מה-Hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    // הסר את ה-connectionId מקבוצת זה המשתמש
                    if (UserConnections.TryGetValue(userId, out var connections))
                    {
                        connections.Remove(Context.ConnectionId);
                        
                        // אם אין יותר חיבורים למשתמש, הסר את המשתמש מהדיקשנרי
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

        /// <summary>
        /// שדר ספר שנוסף לכל החיבורים של המשתמש
        /// </summary>
        public async Task NotifyBookAdded(int userId, object book)
        {
            await BroadcastToUser(userId, "BookAdded", book);
        }

        /// <summary>
        /// שדר ספר שעודכן לכל החיבורים של המשתמש
        /// </summary>
        public async Task NotifyBookUpdated(int userId, object book)
        {
            await BroadcastToUser(userId, "BookUpdated", book);
        }

        /// <summary>
        /// שדר ספר שנמחק לכל החיבורים של המשתמש
        /// </summary>
        public async Task NotifyBookDeleted(int userId, object bookId)
        {
            await BroadcastToUser(userId, "BookDeleted", bookId);
        }

        /// <summary>
        /// שדר אירוע לכל החיבורים של משתמש מסוים
        /// </summary>
        private async Task BroadcastToUser(int userId, string methodName, object data)
        {
            if (UserConnections.TryGetValue(userId, out var connectionIds))
            {
                // שלח את ההודעה לכל ה-connectionIds של המשתמש
                await Clients.Clients(connectionIds.ToList()).SendAsync(methodName, data);
                Console.WriteLine($"Broadcasted {methodName} to {connectionIds.Count} connections of user {userId}");
            }
            else
            {
                Console.WriteLine($"No active connections found for user {userId}");
            }
        }

        /// <summary>
        /// קבל את כל הChats הפעילים כרגע (לצורך דיבוג)
        /// </summary>
        public Task<IEnumerable<int>> GetActiveUsers()
        {
            return Task.FromResult(UserConnections.Keys.AsEnumerable());
        }
    }
}
