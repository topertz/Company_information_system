PRAGMA foreign_keys = ON;

--Create User table
CREATE TABLE if not Exists `User` (
    `UserID` integer NOT NULL PRIMARY KEY, 
    `Username` text not NULL, 
    `PasswordHash` text not NULL,
    `PasswordSalt` text not NULL, 
    `Role` text not NULL
);

--Create Session table
CREATE TABLE if not Exists `Session` (
    `SessionCookie` text NOT NULL PRIMARY KEY, 
    `UserID` integer not NULL, 
    `ValidUntil` integer not NULL,
    `LoginTime` integer not NULL, 
    FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`)
);

--Create Product table
CREATE TABLE if not Exists `Product` (
    `ProductID` integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
    `ProductName` text not NULL, 
    `ProductPrice` integer not NULL
);

--Create Order table
CREATE TABLE if not Exists `Order` (
    `OrderID` integer NOT NULL PRIMARY KEY, 
    `ProductID` integer not null,
    `UserID` integer not null,
    `CustomerName` text not null, 
    `CustomerEmail` text not null,
    `CustomerAddress` text not null,
    `HouseNumber` integer not null,
	`PhoneNumber` text not null,
    `ProductName` text not null,
    `Price` integer not null,
    `Amount` integer not null, 
    `Status` integer not null, 
    `OrderDate` datetime not null,
    FOREIGN KEY(`ProductID`) REFERENCES `Product`(`ProductID`),
    FOREIGN KEY(`UserID`) REFERENCES `User`(`UserID`)
);

--Create Finance table
CREATE TABLE if not Exists `Finance` (
    `FinanceID` integer NOT NULL PRIMARY KEY, 
    `Month` text not null, 
    `Income` integer not null, 
    `Expense` integer not null
);

--Create Invoice table
CREATE TABLE Invoice (
    InvoiceID INTEGER NOT NULL PRIMARY KEY,
    CustomerName TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);

--Create InvoiceItem table
CREATE TABLE InvoiceItem (
    InvoiceItemID INTEGER NOT NULL PRIMARY KEY,
    InvoiceID INTEGER NOT NULL,
    ProductID INTEGER NOT NULL,
    Quantity INTEGER NOT NULL,
    UnitPrice REAL NOT NULL,
    FOREIGN KEY (InvoiceID) REFERENCES Invoice(InvoiceID),
    FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);