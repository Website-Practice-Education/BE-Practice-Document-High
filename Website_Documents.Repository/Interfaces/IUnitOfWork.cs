namespace Website_Documents.Repository.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ISubjectRepository Subjects { get; }
    IQuestionRepository Questions { get; }
    IExamRepository Exams { get; }
    IExamResultRepository ExamResults { get; }
    IStudySpaceRepository StudySpaces { get; }
    IStudySpaceMemberRepository StudySpaceMembers { get; }
    IChatMessageRepository ChatMessages { get; }
    IFriendshipRepository Friendships { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
