# Quick Start Modernization Example

This document provides concrete code examples showing how to modernize the Blayer.Data library from its current 2015-era implementation to modern .NET 8 standards.

## Example Entity Comparison

### Before (Current Implementation)
```csharp
// Entity
using Blayer.Data;
using System.ComponentModel.DataAnnotations.Schema;

[Table("BlogPosts")]
public class BlogPost : EntityBase
{
    public int BlogPostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedDate { get; set; }
    public string AuthorName { get; set; }
}

// Repository
public class BlogPostRepository : Repository<BlogPost>
{
    public override IValidate GetValidate()
    {
        return new BlogPostValidation();
    }
    
    public override INotify GetNotify()
    {
        return new BlogPostNotification();
    }
}

// Validation
public class BlogPostValidation : IValidate
{
    public void Validate(EntityState state, object entity, object originalEntity)
    {
        var blogPost = (BlogPost)entity;
        
        if (string.IsNullOrEmpty(blogPost.Title))
            throw new BusinessException("Title is required", BusinessExceptionType.Validation);
            
        if (blogPost.Title.Length > 200)
            throw new BusinessException("Title must be under 200 characters", BusinessExceptionType.Validation);
    }
}

// Usage
using (var context = new BlayerContext(new MyRepositoryConfiguration()))
{
    var blogRepo = context.GetRepository<BlogPost>();
    
    var post = new BlogPost
    {
        Title = "Hello World",
        Content = "This is my first post",
        CreatedDate = DateTime.Now,
        AuthorName = "John Doe"
    };
    
    blogRepo.Create(post);
    context.Save(); // Synchronous save with validation and notifications
}
```

### After (Modern Implementation)

```csharp
// Entity with nullable reference types
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("BlogPosts")]
public class BlogPost
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(100)]
    public string AuthorName { get; set; } = string.Empty;
    
    // Domain events (optional)
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Modern validation with FluentValidation
public class BlogPostValidator : AbstractValidator<BlogPost>
{
    public BlogPostValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must be under 200 characters");
            
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required");
            
        RuleFor(x => x.AuthorName)
            .NotEmpty()
            .WithMessage("Author name is required")
            .MaximumLength(100);
    }
}

// CQRS Commands and Queries instead of repositories
public record CreateBlogPostCommand(
    string Title, 
    string Content, 
    string AuthorName) : IRequest<int>;

public class CreateBlogPostHandler : IRequestHandler<CreateBlogPostCommand, int>
{
    private readonly AppDbContext _context;
    private readonly IValidator<CreateBlogPostCommand> _validator;
    private readonly IMediator _mediator;
    private readonly ILogger<CreateBlogPostHandler> _logger;
    
    public CreateBlogPostHandler(
        AppDbContext context,
        IValidator<CreateBlogPostCommand> validator,
        IMediator mediator,
        ILogger<CreateBlogPostHandler> logger)
    {
        _context = context;
        _validator = validator;
        _mediator = mediator;
        _logger = logger;
    }
    
    public async Task<int> Handle(CreateBlogPostCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating blog post with title: {Title}", request.Title);
        
        // Validate
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        // Create entity
        var blogPost = new BlogPost
        {
            Title = request.Title,
            Content = request.Content,
            AuthorName = request.AuthorName
        };
        
        // Add domain event
        blogPost.AddDomainEvent(new BlogPostCreatedEvent(blogPost.Title, blogPost.AuthorName));
        
        // Save
        _context.BlogPosts.Add(blogPost);
        await _context.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Created blog post with ID: {BlogPostId}", blogPost.Id);
        
        return blogPost.Id;
    }
}

// Query example
public record GetBlogPostQuery(int Id) : IRequest<BlogPost?>;

public class GetBlogPostHandler : IRequestHandler<GetBlogPostQuery, BlogPost?>
{
    private readonly AppDbContext _context;
    
    public GetBlogPostHandler(AppDbContext context) => _context = context;
    
    public async Task<BlogPost?> Handle(GetBlogPostQuery request, CancellationToken cancellationToken)
    {
        return await _context.BlogPosts
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
    }
}

// Domain event and handler (replaces INotify)
public record BlogPostCreatedEvent(string Title, string AuthorName) : IDomainEvent;

public class BlogPostCreatedHandler : INotificationHandler<BlogPostCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<BlogPostCreatedHandler> _logger;
    
    public BlogPostCreatedHandler(IEmailService emailService, ILogger<BlogPostCreatedHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task Handle(BlogPostCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Blog post created: {Title} by {Author}", 
            notification.Title, notification.AuthorName);
            
        // Send notification email, update search index, etc.
        await _emailService.SendBlogPostCreatedNotification(notification.Title, notification.AuthorName);
    }
}

// Modern usage with dependency injection
public class BlogController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public BlogController(IMediator mediator) => _mediator = mediator;
    
    [HttpPost]
    public async Task<ActionResult<int>> CreateBlogPost(CreateBlogPostCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetBlogPost), new { id }, id);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
    {
        var blogPost = await _mediator.Send(new GetBlogPostQuery(id));
        return blogPost == null ? NotFound() : Ok(blogPost);
    }
}
```

## Configuration & DI Setup

### Before (Manual Configuration)
```csharp
public class MyRepositoryConfiguration : RepositoryConfiguration
{
    public string ConnectionString = "Server=.;Database=MyApp;Integrated Security=true";
    
    public BlogPostRepository BlogPost { get; set; }
    public UserRepository User { get; set; }
}

// Usage
var context = new BlayerContext(new MyRepositoryConfiguration());
```

### After (Modern DI with Options)
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=MyApp;Integrated Security=true"
  },
  "BlayerData": {
    "EnableAuditing": true,
    "CommandTimeout": "00:00:30"
  }
}

// Program.cs (or Startup.cs)
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure services
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            
        builder.Services.Configure<BlayerDataOptions>(
            builder.Configuration.GetSection("BlayerData"));
            
        // Add MediatR
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        
        // Add FluentValidation
        builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        
        // Add custom services
        builder.Services.AddScoped<IEmailService, EmailService>();
        
        var app = builder.Build();
        app.Run();
    }
}

// DbContext
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.AuthorName).HasMaxLength(100);
        });
        
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events
        await DispatchDomainEventsAsync();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private async Task DispatchDomainEventsAsync()
    {
        var entities = ChangeTracker.Entries<BlogPost>()
            .Where(e => e.Entity.DomainEvents.Any())
            .ToList();
            
        var domainEvents = entities
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
            
        entities.ForEach(e => e.Entity.ClearDomainEvents());
        
        var mediator = this.GetService<IMediator>();
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
```

## Testing Comparison

### Before (MSTest)
```csharp
[TestClass]
public class BlogPostTests
{
    [TestMethod]
    public void CreateBlogPost_ValidData_Success()
    {
        // Arrange
        var config = new TestRepositoryConfiguration();
        var context = new BlayerContext(config);
        var repository = context.GetRepository<BlogPost>();
        
        var blogPost = new BlogPost
        {
            Title = "Test Post",
            Content = "Test Content",
            AuthorName = "Test Author"
        };
        
        // Act
        repository.Create(blogPost);
        context.Save();
        
        // Assert
        Assert.IsTrue(blogPost.BlogPostId > 0);
    }
}
```

### After (xUnit with Testcontainers)
```csharp
public class CreateBlogPostHandlerTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    
    public CreateBlogPostHandlerTests(DatabaseFixture fixture) => _fixture = fixture;
    
    [Fact]
    public async Task Handle_ValidCommand_ReturnsBlogPostId()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new CreateBlogPostCommand(
            Title: "Test Post",
            Content: "Test Content", 
            AuthorName: "Test Author");
        
        // Act
        var result = await mediator.Send(command);
        
        // Assert
        Assert.True(result > 0);
        
        // Verify in database
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var blogPost = await context.BlogPosts.FindAsync(result);
        
        Assert.NotNull(blogPost);
        Assert.Equal("Test Post", blogPost.Title);
    }
    
    [Fact]
    public async Task Handle_InvalidTitle_ThrowsValidationException()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        
        var command = new CreateBlogPostCommand(
            Title: "", // Invalid empty title
            Content: "Test Content",
            AuthorName: "Test Author");
        
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => mediator.Send(command));
    }
}

// Database fixture with Testcontainers
public class DatabaseFixture : IDisposable
{
    private readonly MsSqlContainer _container;
    
    public IServiceProvider ServiceProvider { get; private set; }
    
    public DatabaseFixture()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .Build();
            
        _container.StartAsync().Wait();
        
        // Setup DI container
        var services = new ServiceCollection();
        
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(_container.GetConnectionString()));
            
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateBlogPostHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(CreateBlogPostCommand).Assembly);
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddLogging();
        
        ServiceProvider = services.BuildServiceProvider();
        
        // Create database schema
        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
    }
    
    public void Dispose()
    {
        ServiceProvider?.Dispose();
        _container?.DisposeAsync().AsTask().Wait();
    }
}
```

## Key Differences Summary

| Aspect | Before | After |
|--------|--------|-------|
| **Async Support** | None | async/await throughout |
| **Validation** | Custom interfaces + exceptions | FluentValidation with structured errors |
| **Business Logic** | INotify interface | Domain events with MediatR |
| **Dependency Injection** | Manual instantiation | Built-in DI container |
| **Testing** | MSTest with manual setup | xUnit with Testcontainers |
| **Error Handling** | Custom exceptions | Structured validation errors |
| **Configuration** | Hardcoded classes | Options pattern + appsettings.json |
| **Logging** | None built-in | Microsoft.Extensions.Logging |
| **Performance** | Synchronous + multiple abstractions | Async + optimized queries |
| **Maintainability** | Tightly coupled | Loosely coupled with clear separation |

This modernization transforms the library from a 2015-era solution to a contemporary .NET 8 application following current best practices and patterns.