namespace Website_Documents.API.DTOs;

public class LiveSessionDTOs
{
    public class CreateSessionRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SessionType { get; set; } = "practice";
        public int? SubjectId { get; set; }
        public int? TopicId { get; set; }
        public int DifficultyLevel { get; set; } = 1;
        public int QuestionCount { get; set; } = 10;
        public int TimeLimitMinutes { get; set; } = 30;
        public long? SpaceId { get; set; }
    }

    public class SessionResponse
    {
        public long Id { get; set; }
        public long? SpaceId { get; set; }
        public string? SpaceName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public int? SubjectId { get; set; }
        public string? SubjectName { get; set; }
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public int DifficultyLevel { get; set; }
        public int QuestionCount { get; set; }
        public int TimeLimitMinutes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int CurrentParticipants { get; set; }
        public string? InviteCode { get; set; }
        public long HostId { get; set; }
        public string? HostName { get; set; }
        public string? HostAvatar { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class MemberResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool IsReady { get; set; }
        public int QuestionsAnswered { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalScore { get; set; }
        public int CurrentStreak { get; set; }
        public DateTime? JoinedAt { get; set; }
    }

    public class LeaderboardEntryResponse
    {
        public int Rank { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }
        public int TotalScore { get; set; }
        public int QuestionsCorrect { get; set; }
        public int TotalQuestions { get; set; }
        public int AverageTimeSeconds { get; set; }
        public int? FastestAnswerSeconds { get; set; }
        public int LongestStreak { get; set; }
    }

    public class ActivityResponse
    {
        public long Id { get; set; }
        public long? UserId { get; set; }
        public string? UserName { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Metadata { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class ChatMessageResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserAvatar { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public long? ReplyToId { get; set; }
        public bool IsPinned { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class WhiteboardItemResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string? UserName { get; set; }
        public string ElementType { get; set; } = string.Empty;
        public string? Content { get; set; }
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Color { get; set; }
        public int? FontSize { get; set; }
        public int LayerIndex { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class SubmitAnswerRequest
    {
        public long QuestionId { get; set; }
        public long? OptionId { get; set; }
        public char? Letter { get; set; }
        public int TimeSpentSeconds { get; set; }
    }

    public class SendChatRequest
    {
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "text";
        public long? ReplyToId { get; set; }
    }

    public class SetReadyRequest
    {
        public bool IsReady { get; set; }
    }

    public class SetCurrentQuestionRequest
    {
        public long QuestionId { get; set; }
    }

    public class AddWhiteboardTextRequest
    {
        public string Text { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string? Color { get; set; }
        public int? FontSize { get; set; }
    }

    public class AddWhiteboardDrawingRequest
    {
        public string DrawingData { get; set; } = string.Empty;
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; } = "#000000";
        public int? Width { get; set; }
        public int? Height { get; set; }
    }
}
