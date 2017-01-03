[![ahoycoders MyGet Build Status](https://www.myget.org/BuildSource/Badge/ahoycoders?identifier=d9e9494f-d9a8-4195-9904-81ce8e3d8a60)](https://www.myget.org/)
[![NuGet version](https://badge.fury.io/nu/blayer.data.png)](https://badge.fury.io/nu/blayer.data)
# blayer.data
Database abstraction layer using repository pattern

## How to use it
### 1. Install Blayer.Data
Using NuGet `PM> Install-Package Blayer.Data`

### 2. Create a class library project with the following structure (Usually `Your.Namespace.Domain`):

Folders:

1. AdditionalSteps
2. ModelConfiguration
3. Notifications
4. Repositories
5. Validations

A file in the root of the project named `AppConfiguration` with the content:

```
namespace UserNamespace
{
    public class AppConfiguration : RepositoryConfiguration
    {
    }
}
```

This project should reference the imported `Blayer`'s library on step #1.
