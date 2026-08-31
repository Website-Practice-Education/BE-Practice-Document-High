using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Website_Documents.Repository.Models;

namespace Website_Documents.Service.Interfaces;

public interface ICallService
{
    // Session management
    Task<CallSessionDto> CreateCallSessionAsync(long spaceId, long userId, string callType);
    Task<CallSessionDto?> GetCallSessionAsync(long sessionId);
    Task<CallSessionDto?> GetActiveCallForSpaceAsync(long spaceId);
    Task<bool> EndCallSessionAsync(long sessionId, long userId);
    
    // Participant management
    Task<CallParticipantDto?> JoinCallAsync(long sessionId, long userId);
    Task<bool> LeaveCallAsync(long sessionId, long userId);
    Task<List<CallParticipantDto>> GetCallParticipantsAsync(long sessionId);
    
    // Call controls
    Task<bool> ToggleMuteAsync(long sessionId, long userId);
    Task<bool> ToggleVideoAsync(long sessionId, long userId);
    Task<bool> ToggleScreenShareAsync(long sessionId, long userId);
    Task<bool> UpdateConnectionStatusAsync(long sessionId, long userId, string status);
    
    // Utilities
    Task<string> GenerateRoomIdAsync();
    Task<bool> IsUserInCallAsync(long spaceId, long userId);
}

public class CallSessionDto
{
    public long Id { get; set; }
    public long SpaceId { get; set; }
    public long InitiatorId { get; set; }
    public string InitiatorName { get; set; } = string.Empty;
    public string? InitiatorAvatar { get; set; }
    public string CallType { get; set; } = string.Empty;
    public string RoomId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int MaxParticipants { get; set; }
    public int ParticipantCount { get; set; }
    public List<CallParticipantDto> Participants { get; set; } = new();
}

public class CallParticipantDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public DateTime JoinTime { get; set; }
    public DateTime? LeaveTime { get; set; }
    public bool IsMuted { get; set; }
    public bool IsVideoOff { get; set; }
    public bool IsScreenSharing { get; set; }
    public string ConnectionStatus { get; set; } = string.Empty;
    public string? PeerId { get; set; }
}
