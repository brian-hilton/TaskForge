CREATE TABLE [dbo].[Crews]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	supervisor_id INT NULL,
	name NVARCHAR(128),
	created_date DATETIME DEFAULT GETDATE(),
    updated_date DATETIME NULL,
	foreign key (supervisor_id) references Users(id) on delete set null
);

GO

CREATE INDEX IX_Crews_SupervisorId ON Crews(supervisor_id);
