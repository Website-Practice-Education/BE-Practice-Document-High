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
    IPasswordResetTokenRepository PasswordResetTokens { get; }

    /// <summary>
    /// Underlying EF Core <see cref="Microsoft.EntityFrameworkCore.DbContext"/>,
    /// exposed for advanced scenarios such as executing raw SQL that needs to
    /// bypass identity-column generation logic.
    /// </summary>
    Microsoft.EntityFrameworkCore.DbContext Context { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
