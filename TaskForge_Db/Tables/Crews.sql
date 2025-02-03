CREATE TABLE [dbo].[Crews]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
	supervisor_id INT NULL,
	name NVARCHAR(128),
	foreign key (supervisor_id) references Users(id) on delete set null
)
