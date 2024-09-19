-- Create Users table
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME2,
    IsActive BIT NOT NULL DEFAULT 1
);

-- Create Documents table
CREATE TABLE Documents (
    Id NVARCHAR(450) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Content NVARCHAR(MAX),
    OwnerId NVARCHAR(450) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastModifiedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    IsPublic BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (OwnerId) REFERENCES Users(Id)
);

-- Create UserDocuments table for managing document access
CREATE TABLE UserDocuments (
    UserId NVARCHAR(450) NOT NULL,
    DocumentId NVARCHAR(450) NOT NULL,
    AccessLevel INT NOT NULL DEFAULT 0,
    PRIMARY KEY (UserId, DocumentId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id)
);

-- Create Versions table for document versioning
CREATE TABLE Versions (
    Id NVARCHAR(450) PRIMARY KEY,
    DocumentId NVARCHAR(450) NOT NULL,
    VersionNumber INT NOT NULL,
    Content NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById NVARCHAR(450) NOT NULL,
    Comment NVARCHAR(500),
    Size BIGINT NOT NULL,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
    FOREIGN KEY (CreatedById) REFERENCES Users(Id)
);

-- Create Comments table for document commenting
CREATE TABLE Comments (
    Id NVARCHAR(450) PRIMARY KEY,
    DocumentId NVARCHAR(450) NOT NULL,
    UserId NVARCHAR(450) NOT NULL,
    Content NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Position INT NOT NULL,
    IsResolved BIT NOT NULL DEFAULT 0,
    ParentCommentId NVARCHAR(450),
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ParentCommentId) REFERENCES Comments(Id)
);

-- Create UserSettings table for storing user preferences
CREATE TABLE UserSettings (
    UserId NVARCHAR(450) PRIMARY KEY,
    Theme NVARCHAR(50) NOT NULL DEFAULT 'Default',
    FontSize INT NOT NULL DEFAULT 12,
    Language NVARCHAR(10) NOT NULL DEFAULT 'en-US',
    AutoSaveInterval INT NOT NULL DEFAULT 300,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Create Tags table for document tagging
CREATE TABLE Tags (
    Id NVARCHAR(450) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE
);

-- Create DocumentTags table for many-to-many relationship between Documents and Tags
CREATE TABLE DocumentTags (
    DocumentId NVARCHAR(450) NOT NULL,
    TagId NVARCHAR(450) NOT NULL,
    PRIMARY KEY (DocumentId, TagId),
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id),
    FOREIGN KEY (TagId) REFERENCES Tags(Id)
);

-- Create indexes for improved query performance
CREATE INDEX IX_Documents_OwnerId ON Documents (OwnerId);
CREATE INDEX IX_UserDocuments_DocumentId ON UserDocuments (DocumentId);
CREATE INDEX IX_Versions_DocumentId ON Versions (DocumentId);
CREATE INDEX IX_Comments_DocumentId ON Comments (DocumentId);
CREATE INDEX IX_Comments_UserId ON Comments (UserId);