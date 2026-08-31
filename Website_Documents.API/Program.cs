using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using Website_Documents.API.Hubs;
using Website_Documents.API.Middleware;
using Website_Documents.API.Services;
using Website_Documents.Repository;
using Website_Documents.Repository.DBContext;
using Website_Documents.Repository.Interfaces;
using Website_Documents.Repository.Repositories;
using Website_Documents.Service;
using Website_Documents.Service.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 0. Cấu hình User Secrets (chạy trước khi load appsettings.json)
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// 1. Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<BookstoreDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 3. Đăng ký Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IExamAttemptService, ExamAttemptService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStudySpaceService, StudySpaceService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();

// Learning & Study Services
builder.Services.AddScoped<IStudyService, StudyService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ILearningPlanService, LearningPlanService>();

// Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Collaborative Study Room Services
builder.Services.AddScoped<ILiveSessionService, LiveSessionService>();
builder.Services.AddScoped<IWhiteboardService, WhiteboardService>();

// Room Features Services (Music, Files, Settings)
builder.Services.AddScoped<IRoomMusicService, RoomMusicService>();
builder.Services.AddScoped<IRoomFileService, RoomFileService>();
builder.Services.AddScoped<IRoomSettingsService, RoomSettingsService>();

// Shared Documents Service
builder.Services.AddScoped<ISharedDocumentService, SharedDocumentService>();

// Local Storage Service
builder.Services.AddSingleton<IStorageService, LocalStorageService>();

// Forum Service
builder.Services.AddScoped<IForumService, ForumService>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();

// Call Service (Audio/Video)
builder.Services.AddScoped<ICallService, CallService>();

// Gemini AI Service
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Register Repositories
builder.Services.AddScoped<ILiveSessionRepository, LiveSessionRepository>();
builder.Services.AddScoped<ILiveSessionMemberRepository, LiveSessionMemberRepository>();
builder.Services.AddScoped<ISessionActivityRepository, SessionActivityRepository>();
builder.Services.AddScoped<ISessionChatRepository, SessionChatRepository>();
builder.Services.AddScoped<ISessionWhiteboardRepository, SessionWhiteboardRepository>();
builder.Services.AddScoped<ISessionSharedQuestionRepository, SessionSharedQuestionRepository>();
builder.Services.AddScoped<ISessionParticipantAnswerRepository, SessionParticipantAnswerRepository>();
builder.Services.AddScoped<ISessionLeaderboardRepository, SessionLeaderboardRepository>();

// SignalR
builder.Services.AddSignalR();

// 4. Cấu hình JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]!;
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 5. CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "https://localhost:7007",
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetIsOriginAllowed(host => true); // Cho phép tất cả origins trong development
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Xử lý vòng lặp navigation property (Subject -> SharedDocuments -> Subject)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// Configure request size limits for file uploads (100MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Website Documents API",
        Version = "v1",
        Description = "API for Practice High Edu Document"
    });
    
    // Cấu hình để xử lý nullable reference types
    c.UseInlineDefinitionsForEnums();
    c.IgnoreObsoleteProperties();
    
    // Thêm cấu hình SchemaGeneratorOptions
    c.SchemaGeneratorOptions = new Swashbuckle.AspNetCore.SwaggerGen.SchemaGeneratorOptions
    {
        SchemaIdSelector = type => type.FullName
    };
});

var app = builder.Build();

// 6. Middleware
app.UseExceptionMiddleware();

// Force HTTPS redirect in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Configure request size limits for file uploads
app.Use(async (context, next) =>
{
    context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize = 104857600; // 100MB
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Serve uploaded files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.MapHub<LiveSessionHub>("/hubs/live-session");
app.MapHub<CallHub>("/hubs/call");

app.Run();
