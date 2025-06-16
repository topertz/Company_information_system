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
command.CommandText = "PRAGMA foreign_keys = ON;" +
    "CREATE TABLE if not Exists `Products` " +
    "(`ProductID` integer NOT NULL PRIMARY KEY, `ProductName` text not NULL, `ProductPrice` integer not NULL);" +
    "CREATE TABLE if not Exists `Orders`" +
    " (`OrderID` integer NOT NULL PRIMARY KEY, `CustomerName` text not null," +
    " `ProductID` integer not null, " +
    "`Amount` integer not null, `Status` integer not null, `OrderDate` datetime not null," +
    "FOREIGN KEY(`ProductID`) REFERENCES `Orders`(`ProductID`));" +
    "CREATE TABLE if not Exists `Finance` (`FinanceID` integer NOT NULL PRIMARY KEY, " +
    "`Month` text not null, `Income` integer not null, `Expense` integer not null);";
command.ExecuteNonQuery();
command.Dispose();

app.Run();