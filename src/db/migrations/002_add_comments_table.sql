-- Create Comments table
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
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (ParentCommentId) REFERENCES Comments(Id)
);

-- Create index on DocumentId for faster queries
CREATE INDEX IX_Comments_DocumentId ON Comments (DocumentId);

-- Create index on UserId for faster queries
CREATE INDEX IX_Comments_UserId ON Comments (UserId);

-- Create index on ParentCommentId for faster queries on nested comments
CREATE INDEX IX_Comments_ParentCommentId ON Comments (ParentCommentId);