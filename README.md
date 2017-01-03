# blayer.data
Database abstraction layer using repository pattern

## Installation
Using NuGet `PM> Install-Package Blayer.Data`

## How to use it
I'm planning on uploading the library to nuget and creating a project template for a smoother use, but for now this is how it should be done:

### 1. Import Blayer's code
### 2. Create a class library project with the following structure:

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
