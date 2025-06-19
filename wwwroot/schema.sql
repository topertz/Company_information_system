PRAGMA foreign_keys = ON;

-- Create Products Table
CREATE TABLE if not Exists `Products` (
  `ProductID` integer NOT NULL PRIMARY KEY, 
  `ProductName` text not NULL, 
  `ProductPrice` integer not NULL
);

-- Create Orders Table
CREATE TABLE if not Exists `Orders` (
  `OrderID` integer NOT NULL PRIMARY KEY, 
  `CustomerName` text not null,
  `ProductID` integer not null, 
  `Amount` integer not null, 
  `Status` integer not null, 
  `OrderDate` datetime not null,
   FOREIGN KEY("ProductID") REFERENCES "Orders"("ProductID")
);

-- Create Finance Table
CREATE TABLE if not Exists `Finance` (
  `FinanceID` integer NOT NULL PRIMARY KEY, 
  `Month` text not null, 
  `Income` integer not null, 
  `Expense` integer not null
);

-- Create Users Table
CREATE TABLE Users (
    UserID INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT NOT NULL UNIQUE,
    PasswordHash TEXT NOT NULL
);