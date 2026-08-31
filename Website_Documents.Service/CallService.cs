using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Models;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class CallService : ICallService
{
    private readonly BookstoreDbContext _context;

    public CallService(BookstoreDbContext context)
    {
        _context = context;
    }

    public async Task<CallSessionDto> CreateCallSessionAsync(long spaceId, long userId, string callType)
    {
        // Check if there's already an active call in this space
        var existingCall = await GetActiveCallForSpaceAsync(spaceId);
        if (existingCall != null)
        {
            throw new InvalidOperationException("There's already an active call in this space");
        }

        var roomId = await GenerateRoomIdAsync();
        
        var session = new CallSession
        {
            SpaceId = spaceId,
            InitiatorId = userId,
            CallType = callType,
            RoomId = roomId,
            Status = "active",
            StartedAt = DateTime.UtcNow,
            MaxParticipants = 10,
            CreatedAt = DateTime.UtcNow
        };

        _context.CallSessions.Add(session);
        await _context.SaveChangesAsync();

        // Auto-join the initiator to the call
        var participant = new CallParticipant
        {
            CallSessionId = session.Id,
            UserId = userId,
            JoinTime = DateTime.UtcNow,
            ConnectionStatus = "connected",
            PeerId = GeneratePeerId()
        };

        _context.CallParticipants.Add(participant);
        await _context.SaveChangesAsync();

        return await GetCallSessionAsync(session.Id) ?? throw new InvalidOperationException("Failed to create call session");
    }

    public async Task<CallSessionDto?> GetCallSessionAsync(long sessionId)
    {
        var session = await _context.CallSessions
            .Include(s => s.Initiator)
            .Include(s => s.Participants)
                .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return null;

        return MapToDto(session);
    }

    public async Task<CallSessionDto?> GetActiveCallForSpaceAsync(long spaceId)
    {
        var session = await _context.CallSessions
            .Include(s => s.Initiator)
            .Include(s => s.Participants)
                .ThenInclude(p => p.User)
            .Where(s => s.SpaceId == spaceId && s.Status == "active")
            .OrderByDescending(s => s.StartedAt)
            .FirstOrDefaultAsync();

        if (session == null)
            return null;

        return MapToDto(session);
    }

    public async Task<bool> EndCallSessionAsync(long sessionId, long userId)
    {
        var session = await _context.CallSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            return false;

        // Only the initiator can end the call
        if (session.InitiatorId != userId)
            return false;

        session.Status = "ended";
        session.EndedAt = DateTime.UtcNow;

        // Mark all participants as left
        var participants = await _context.CallParticipants
            .Where(p => p.CallSessionId == sessionId && p.LeaveTime == null)
            .ToListAsync();

        foreach (var participant in participants)
        {
            participant.LeaveTime = DateTime.UtcNow;
            participant.ConnectionStatus = "disconnected";
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<CallParticipantDto?> JoinCallAsync(long sessionId, long userId)
    {
        var session = await _context.CallSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.Status == "active");

        if (session == null)
            return null;

        // Check if user is already in the call
        var existingParticipant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (existingParticipant != null)
        {
            // Reconnect existing participant
            existingParticipant.LeaveTime = null;
            existingParticipant.JoinTime = DateTime.UtcNow;
            existingParticipant.ConnectionStatus = "connected";
            existingParticipant.PeerId = GeneratePeerId();
            await _context.SaveChangesAsync();
            
            var user = await _context.Users.FindAsync(userId);
            return new CallParticipantDto
            {
                Id = existingParticipant.Id,
                UserId = existingParticipant.UserId,
                UserName = user?.FullName ?? "Unknown",
                UserAvatar = user?.AvatarUrl,
                JoinTime = existingParticipant.JoinTime,
                IsMuted = existingParticipant.IsMuted,
                IsVideoOff = existingParticipant.IsVideoOff,
                IsScreenSharing = existingParticipant.IsScreenSharing,
                ConnectionStatus = existingParticipant.ConnectionStatus,
                PeerId = existingParticipant.PeerId
            };
        }

        // Check max participants
        var currentCount = await _context.CallParticipants
            .CountAsync(p => p.CallSessionId == sessionId && p.LeaveTime == null);

        if (currentCount >= session.MaxParticipants)
            throw new InvalidOperationException("Call is full");

        var participant = new CallParticipant
        {
            CallSessionId = sessionId,
            UserId = userId,
            JoinTime = DateTime.UtcNow,
            ConnectionStatus = "connected",
            PeerId = GeneratePeerId()
        };

        _context.CallParticipants.Add(participant);
        await _context.SaveChangesAsync();

        var joinedUser = await _context.Users.FindAsync(userId);
        return new CallParticipantDto
        {
            Id = participant.Id,
            UserId = participant.UserId,
            UserName = joinedUser?.FullName ?? "Unknown",
            UserAvatar = joinedUser?.AvatarUrl,
            JoinTime = participant.JoinTime,
            IsMuted = participant.IsMuted,
            IsVideoOff = participant.IsVideoOff,
            IsScreenSharing = participant.IsScreenSharing,
            ConnectionStatus = participant.ConnectionStatus,
            PeerId = participant.PeerId
        };
    }

    public async Task<bool> LeaveCallAsync(long sessionId, long userId)
    {
        var participant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (participant == null)
            return false;

        participant.LeaveTime = DateTime.UtcNow;
        participant.ConnectionStatus = "disconnected";
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CallParticipantDto>> GetCallParticipantsAsync(long sessionId)
    {
        var participants = await _context.CallParticipants
            .Include(p => p.User)
            .Where(p => p.CallSessionId == sessionId && p.LeaveTime == null)
            .OrderBy(p => p.JoinTime)
            .ToListAsync();

        return participants.Select(p => new CallParticipantDto
        {
            Id = p.Id,
            UserId = p.UserId,
            UserName = p.User?.FullName ?? "Unknown",
            UserAvatar = p.User?.AvatarUrl,
            JoinTime = p.JoinTime,
            LeaveTime = p.LeaveTime,
            IsMuted = p.IsMuted,
            IsVideoOff = p.IsVideoOff,
            IsScreenSharing = p.IsScreenSharing,
            ConnectionStatus = p.ConnectionStatus,
            PeerId = p.PeerId
        }).ToList();
    }

    public async Task<bool> ToggleMuteAsync(long sessionId, long userId)
    {
        var participant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (participant == null)
            return false;

        participant.IsMuted = !participant.IsMuted;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleVideoAsync(long sessionId, long userId)
    {
        var participant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (participant == null)
            return false;

        participant.IsVideoOff = !participant.IsVideoOff;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleScreenShareAsync(long sessionId, long userId)
    {
        var participant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (participant == null)
            return false;

        participant.IsScreenSharing = !participant.IsScreenSharing;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateConnectionStatusAsync(long sessionId, long userId, string status)
    {
        var participant = await _context.CallParticipants
            .FirstOrDefaultAsync(p => p.CallSessionId == sessionId && p.UserId == userId && p.LeaveTime == null);

        if (participant == null)
            return false;

        participant.ConnectionStatus = status;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> GenerateRoomIdAsync()
    {
        return $"call_{Guid.NewGuid():N}"[..20].ToUpper();
    }

    public async Task<bool> IsUserInCallAsync(long spaceId, long userId)
    {
        var activeSession = await _context.CallSessions
            .Where(s => s.SpaceId == spaceId && s.Status == "active")
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

        if (activeSession == 0)
            return false;

        return await _context.CallParticipants
            .AnyAsync(p => p.CallSessionId == activeSession && p.UserId == userId && p.LeaveTime == null);
    }

    private string GeneratePeerId()
    {
        return $"peer_{Guid.NewGuid():N}"[..16].ToUpper();
    }

    private CallSessionDto MapToDto(CallSession session)
    {
        return new CallSessionDto
        {
            Id = session.Id,
            SpaceId = session.SpaceId,
            InitiatorId = session.InitiatorId,
            InitiatorName = session.Initiator?.FullName ?? "Unknown",
            InitiatorAvatar = session.Initiator?.AvatarUrl,
            CallType = session.CallType,
            RoomId = session.RoomId,
            Status = session.Status,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt,
            MaxParticipants = session.MaxParticipants,
            ParticipantCount = session.Participants.Count(p => p.LeaveTime == null),
            Participants = session.Participants
                .Where(p => p.LeaveTime == null)
                .Select(p => new CallParticipantDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = p.User?.FullName ?? "Unknown",
                    UserAvatar = p.User?.AvatarUrl,
                    JoinTime = p.JoinTime,
                    LeaveTime = p.LeaveTime,
                    IsMuted = p.IsMuted,
                    IsVideoOff = p.IsVideoOff,
                    IsScreenSharing = p.IsScreenSharing,
                    ConnectionStatus = p.ConnectionStatus,
                    PeerId = p.PeerId
                }).ToList()
        };
    }
}
