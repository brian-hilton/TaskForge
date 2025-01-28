CREATE TABLE [dbo].[Jobs]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[name] nvarchar(128) NOT NULL,
	[status] nvarchar(128) NOT NULL,
	[location] nvarchar(128) NOT NULL,
	[user_id] int,
	[created_date] datetime2 NOT NULL,
	[updated_date] datetime2 NOT NULL,
	[due_date] datetime2,
	CONSTRAINT [FK_Jobs_Users] FOREIGN KEY ([user_id]) REFERENCES [Users]([id])
);

GO

CREATE INDEX [idx_Jobs_name] ON [dbo].[Jobs] ([name]);

GO

CREATE INDEX [idx_Jobs_status] ON [dbo].[Jobs] ([status]);

GO

CREATE INDEX [idx_Jobs_location] ON [dbo].[Jobs] ([location]);

GO

CREATE INDEX [idx_Jobs_user_id] ON [dbo].[Jobs] ([user_id]);

GO

CREATE INDEX [idx_Jobs_created_date] ON [dbo].[Jobs] ([created_date]);

GO

CREATE INDEX [idx_Jobs_updated_date] ON [dbo].[Jobs] ([updated_date]);

GO

CREATE INDEX [idx_Jobs_due_date] ON [dbo].[Jobs] ([due_date]);
