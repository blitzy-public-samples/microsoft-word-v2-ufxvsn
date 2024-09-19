-- Create Versions table
CREATE TABLE Versions (
    Id NVARCHAR(450) PRIMARY KEY,
    DocumentId NVARCHAR(450) NOT NULL,
    VersionNumber INT NOT NULL,
    Content NVARCHAR(MAX),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CreatedById NVARCHAR(450) NOT NULL,
    Comment NVARCHAR(500),
    Size BIGINT NOT NULL,
    FOREIGN KEY (DocumentId) REFERENCES Documents(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES Users(Id),
    CONSTRAINT UQ_Versions_DocumentId_VersionNumber UNIQUE (DocumentId, VersionNumber)
);

-- Create index on DocumentId for faster lookups
CREATE INDEX IX_Versions_DocumentId ON Versions (DocumentId);

-- Create index on CreatedById for faster user-based queries
CREATE INDEX IX_Versions_CreatedById ON Versions (CreatedById);

-- Alter Documents table to add CurrentVersionId
ALTER TABLE Documents
ADD CurrentVersionId NVARCHAR(450);

-- Add foreign key constraint to link Documents with Versions
ALTER TABLE Documents
ADD CONSTRAINT FK_Documents_Versions_CurrentVersionId 
FOREIGN KEY (CurrentVersionId) REFERENCES Versions(Id);

-- Create index on CurrentVersionId for faster joins
CREATE INDEX IX_Documents_CurrentVersionId ON Documents (CurrentVersionId);