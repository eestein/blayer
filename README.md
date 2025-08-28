[![ahoycoders MyGet Build Status](https://www.myget.org/BuildSource/Badge/ahoycoders?identifier=d9e9494f-d9a8-4195-9904-81ce8e3d8a60)](https://www.myget.org/)
[![NuGet version](https://badge.fury.io/nu/blayer.data.png)](https://badge.fury.io/nu/blayer.data)

# ⚠️ Legacy Library Notice

**This library is based on 2015-era technology and is no longer recommended for new projects.**

# blayer.data
Database abstraction layer using repository pattern built on Entity Framework 6 and .NET Framework 4.6.1

## 📋 What This Library Does

Blayer.Data provides a structured Repository pattern implementation with:
- **Entity Management** with base classes and soft delete support
- **Validation Hooks** that run before save operations
- **Notification System** for post-save business logic
- **Additional Steps** for custom business rules
- **Configuration Pattern** for declarative repository setup

See [LIBRARY_ANALYSIS.md](./LIBRARY_ANALYSIS.md) for a comprehensive overview.

## 🚨 Current Status & Modernization

This library was built on technologies from 2015 and needs significant modernization:

- **Current Stack**: .NET Framework 4.6.1 + Entity Framework 6.1.3
- **Issues**: Windows-only, outdated dependencies, no async support, complex architecture
- **Recommendation**: Complete rewrite for modern .NET

📖 **Read our comprehensive guides:**
- [**Library Analysis**](./LIBRARY_ANALYSIS.md) - Understanding what the library does
- [**Modernization Guide**](./MODERNIZATION_GUIDE.md) - Complete modernization strategy  
- [**Code Examples**](./MODERNIZATION_EXAMPLES.md) - Before/after code comparisons

## Installation (Legacy)
Using NuGet `PM> Install-Package Blayer.Data`

⚠️ **Note**: This package targets .NET Framework 4.6.1 and will not work on modern .NET Core/.NET 5+ applications.

## How to use it
Please visit the [Wiki](https://github.com/eestein/blayer/wiki) you'll be able to find all the documentation there.

## Modern Alternatives

Instead of using this library, consider these modern approaches:

### Direct Entity Framework Core
```csharp
// Simple, direct approach
services.AddDbContext<MyDbContext>(options => 
    options.UseSqlServer(connectionString));
```

### CQRS with MediatR + EF Core
```csharp
// Modern command/query separation
public record CreateBlogPostCommand(string Title, string Content) : IRequest<int>;
// See MODERNIZATION_EXAMPLES.md for full implementation
```

### Repository Pattern with Modern .NET
```csharp
// If you prefer repositories, implement with modern patterns
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(T entity, CancellationToken cancellationToken = default);
}
```

## Migration Path

If you're currently using this library:

1. **Immediate**: Continue using for existing applications (maintenance mode)
2. **Planning**: Review our [modernization guide](./MODERNIZATION_GUIDE.md) 
3. **New Projects**: Use modern alternatives mentioned above
4. **Migration**: Follow our [step-by-step migration examples](./MODERNIZATION_EXAMPLES.md)

## Support

This library is in maintenance mode. For modern .NET applications, we recommend:
- Entity Framework Core with built-in features
- MediatR for CQRS patterns  
- FluentValidation for validation
- Domain events for business logic
