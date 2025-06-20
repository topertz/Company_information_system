using System.Data.SQLite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseStaticFiles();

app.MapGet("/", () => Results.Redirect("/index.html"));
SQLiteConnection connection = DatabaseConnector.Db();
SQLiteCommand command = connection.CreateCommand();
command.CommandText = @"
    PRAGMA foreign_keys = ON;
    
    CREATE TABLE if not Exists `User` (
        `UserID` integer NOT NULL PRIMARY KEY, 
        `Username` text not NULL, 
        `PasswordHash` text not NULL,
        `PasswordSalt` text not NULL, 
        `Role` text not NULL
    );

    CREATE TABLE if not Exists `Session` (
        `SessionCookie` text NOT NULL PRIMARY KEY, 
        `UserID` integer not NULL, 
        `ValidUntil` integer not NULL,
        `LoginTime` integer not NULL, 
        FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`)
    );

    CREATE TABLE if not Exists `Product` (
        `ProductID` integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
        `ProductName` text not NULL, 
        `ProductPrice` integer not NULL
    );
    
    CREATE TABLE if not Exists `Order` (
        `OrderID` integer NOT NULL PRIMARY KEY, 
        `ProductID` integer not null,
        `UserID` integer not null,
        `CustomerName` text not null, 
        `CustomerEmail` text not null,
        `CustomerAddress` text not null,
        `HouseNumber` text not null,
        `ProductName` text not null,
        `Price` integer not null,
        `Amount` integer not null, 
        `Status` integer not null, 
        `OrderDate` datetime not null,
        FOREIGN KEY(`ProductID`) REFERENCES `Product`(`ProductID`),
        FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`)
    );
    
    CREATE TABLE if not Exists `Finance` (
        `FinanceID` integer NOT NULL PRIMARY KEY, 
        `Month` text not null, 
        `Income` integer not null, 
        `Expense` integer not null
    );
";
command.ExecuteNonQuery();
command.Dispose();

app.Run();