using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Repositories;

namespace Website_Documents.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly BookstoreDbContext _context;
    private IDbContextTransaction? _transaction;

    private IUserRepository? _users;
    private ISubjectRepository? _subjects;
    private IQuestionRepository? _questions;
    private IExamRepository? _exams;
    private IExamResultRepository? _examResults;
    private IStudySpaceRepository? _studySpaces;
    private IStudySpaceMemberRepository? _studySpaceMembers;
    private IChatMessageRepository? _chatMessages;
    private IFriendshipRepository? _friendships;
    private IPasswordResetTokenRepository? _passwordResetTokens;

    public UnitOfWork(BookstoreDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public ISubjectRepository Subjects =>
        _subjects ??= new SubjectRepository(_context);

    public IQuestionRepository Questions =>
        _questions ??= new QuestionRepository(_context);

    public IExamRepository Exams =>
        _exams ??= new ExamRepository(_context);

    public IExamResultRepository ExamResults =>
        _examResults ??= new ExamResultRepository(_context);

    public IStudySpaceRepository StudySpaces =>
        _studySpaces ??= new StudySpaceRepository(_context);

    public IStudySpaceMemberRepository StudySpaceMembers =>
        _studySpaceMembers ??= new StudySpaceMemberRepository(_context);

    public IChatMessageRepository ChatMessages =>
        _chatMessages ??= new ChatMessageRepository(_context);

    public IFriendshipRepository Friendships =>
        _friendships ??= new FriendshipRepository(_context);

    public IPasswordResetTokenRepository PasswordResetTokens =>
        _passwordResetTokens ??= new PasswordResetTokenRepository(_context);

    /// <inheritdoc />
    public DbContext Context => _context;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
