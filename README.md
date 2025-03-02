# SQLite Schema Repository

A lightweight schema-driven repository pattern for SQLite in .NET.

## Features

- schema-driven repository design 
- auto-generates SQL for CRUD operations 
- minimal runtime reflection 

## Usage

### 1. Define a Model

Create a simple class that represents your table:

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

### 2. Define a Schema

Set up a `ModelSchema` for your model:

```csharp
var key = new ModelProperty<User>("Id", PropertyType.Integer);
var properties = new List<NullableModelProperty<User>>
{
    new NullableModelProperty<User>("Name", PropertyType.String)
};
var schema = new ModelSchema<User>("Users", key, properties);
```

### 3. Create a Repository

Extend the `Repository<T>` class to manage your entity:

```csharp
public class UserRepository : Repository<User>
{
    public UserRepository(ModelSchema<User> schema, SqliteConnection connection) : base(schema, connection) { }
}
```

### 4. Perform CRUD Operations

```csharp
using var connection = new SqliteConnection("Data Source=mydatabase.db");
connection.Open();

var userRepo = new UserRepository(schema, connection);
userRepo.CreateTable();

// Insert a user
var user = new User { Id = 1, Name = "John Doe" };
userRepo.Insert(user);

// Retrieve user
var retrievedUser = userRepo.SelectByKey(1);
Console.WriteLine(retrievedUser.Name);

// Update user
user.Name = "Jane Doe";
userRepo.Update(user);

// Delete user
userRepo.DeleteByKey(1);
```
