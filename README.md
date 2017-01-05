[![ahoycoders MyGet Build Status](https://www.myget.org/BuildSource/Badge/ahoycoders?identifier=d9e9494f-d9a8-4195-9904-81ce8e3d8a60)](https://www.myget.org/)
[![NuGet version](https://badge.fury.io/nu/blayer.data.png)](https://badge.fury.io/nu/blayer.data)
# blayer.data
Database abstraction layer using repository pattern

## Installation
Using NuGet `PM> Install-Package Blayer.Data`

## How to use it
### 1. Create a class library project
Usually `Your.Namespace.Domain`

### 2. Add Blayer.Data package to this library

### 3. Add the folders below

1. AdditionalSteps
2. ModelConfiguration
3. Notifications
4. Repositories
5. Validations

### 4. Create a file in the root of the project

A file in the root of the project named `AppConfiguration` (just an example) with the content:

```
namespace Your.Namespace.Domain
{
    public class AppConfiguration : RepositoryConfiguration
    {
    }
}
```
