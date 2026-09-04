using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service;

public class ChatService : Interfaces.IChatService
{
    /// <summary>
    /// Special id used by the floating "global" chat. There is always exactly one
    /// global space in the database (auto-created on first use) so that any
    /// authenticated user can chat without being a member.
    /// </summary>
    public const long GlobalSpaceId = 0;

    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ChatMessage> SendMessageAsync(long spaceId, long userId, string content, string messageType = "text")
    {
        // Validate content
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.", nameof(content));

        // The global chat does NOT require the user to be a member of any study
        // space. For the global chat we lazily ensure a corresponding row exists
        // in study_spaces so the foreign key on chat_messages is satisfied.
        if (spaceId == GlobalSpaceId)
        {
            await EnsureGlobalSpaceAsync();
        }
        else
        {
            // Validate that the study space exists and is active
            var space = await _unitOfWork.StudySpaces.GetByIdAsync(spaceId);
            if (space == null || space.IsActive != true)
                throw new InvalidOperationException($"Study space with id {spaceId} does not exist or is inactive.");

            // Validate that the user is a member of the space (active member)
            var isMember = await _unitOfWork.StudySpaceMembers.IsMemberAsync(spaceId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException($"User {userId} is not an active member of study space {spaceId}.");
        }

        var message = new ChatMessage
        {
            SpaceId = spaceId,
            UserId = userId,
            Content = content,
            MessageType = messageType,
            CreatedAt = DateTime.UtcNow
        };

        var createdMessage = await _unitOfWork.ChatMessages.CreateAsync(message);

        // Reload with User included for SignalR broadcast
        // Use explicit query to ensure User is loaded (EF Core may return cached entity without navigation properties)
        var result = await _unitOfWork.Context.Set<ChatMessage>()
            .Include(m => m.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == createdMessage.Id);
        
        return result ?? createdMessage;
    }

    /// <summary>
    /// Ensures that a study space with id = GlobalSpaceId exists and is active.
    /// If absent, a "Chat tổng" row is inserted via raw SQL because the EF Core
    /// Npgsql provider treats <c>bigserial</c> columns as identity columns and
    /// would otherwise overwrite any client-supplied id. Idempotent.
    /// </summary>
    private async Task EnsureGlobalSpaceAsync()
    {
        var existing = await _unitOfWork.StudySpaces.GetByIdAsync(GlobalSpaceId);
        if (existing != null && existing.IsActive == true)
            return;

        try
        {
            // Use raw SQL with ON CONFLICT so concurrent requests cannot race
            // against each other and so we can force the id = 0 to be used.
            // We use NULL (not 0) for created_by because users.id is a SERIAL
            // that starts at 1, so 0 would violate the FK on users(id).
            const string upsertSql = @"
                INSERT INTO study_spaces (id, name, description, space_type, invite_code, max_members, is_active, created_by, created_at)
                VALUES (0, 'Chat tổng', 'Phòng chat chung cho tất cả người dùng', 'global', 'GLOBAL', 2147483647, TRUE, NULL, NOW())
                ON CONFLICT (id) DO UPDATE
                  SET name = EXCLUDED.name,
                      description = EXCLUDED.description,
                      space_type = EXCLUDED.space_type,
                      is_active = TRUE;";

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(upsertSql);
        }
        catch (Exception)
        {
            var refetched = await _unitOfWork.StudySpaces.GetByIdAsync(GlobalSpaceId);
            if (refetched == null || refetched.IsActive != true)
                throw;
        }
    }

    public async Task<List<ChatMessage>> GetMessagesAsync(long spaceId, int page = 1, int pageSize = 50)
    {
        return await _unitOfWork.ChatMessages.GetMessagesAsync(spaceId, page, pageSize);
    }

    public async Task<int> GetUnreadCountAsync(long spaceId, long userId, DateTime? since = null)
    {
        return await _unitOfWork.ChatMessages.GetUnreadCountAsync(spaceId, userId, since);
    }
}
