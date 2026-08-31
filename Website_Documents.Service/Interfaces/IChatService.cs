using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface IChatService
{
    Task<ChatMessage> SendMessageAsync(long spaceId, long userId, string content, string messageType = "text");
    Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page = 1, int pageSize = 50);
    Task<int> GetUnreadCountAsync(long spaceId, long userId, DateTime? since = null);
}
