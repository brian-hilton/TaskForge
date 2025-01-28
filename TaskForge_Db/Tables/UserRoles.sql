CREATE TABLE [dbo].[UserRoles]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[user_id] INT NOT NULL,
	[role_id] INT NOT NULL, 
    CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([user_id]) REFERENCES [Users]([id]),
	CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([role_id]) REFERENCES [Roles]([id])
)

GO

CREATE INDEX [idx_UserRoles_user_id] ON [dbo].[UserRoles] ([user_id]);

GO

CREATE INDEX [idx_UserRoles_role_id] ON [dbo].[UserRoles] ([role_id]);
