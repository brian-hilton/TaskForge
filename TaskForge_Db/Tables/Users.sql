CREATE TABLE [dbo].[Users]
(
	[id] INT NOT NULL PRIMARY KEY IDENTITY,
	[name] nvarchar(128) NOT NULL,
	[password] nvarchar(2048) NOT NULL,
	[created_date] datetime2 NOT NULL,
	[updated_date] datetime2 NOT NULL,
	[email] nvarchar(128) NOT NULL,
);

GO

create index idx_users_name on [dbo].[Users] ([name]);
