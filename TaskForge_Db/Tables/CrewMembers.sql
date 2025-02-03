CREATE TABLE [dbo].[CrewMembers]
(
	crew_id INT NOT NULL,
	user_id INT NOT NULL,
	role NVARCHAR(32) not null check (role in ('worker', 'supervisor')),
	PRIMARY KEY (crew_id, user_id),
	foreign key (crew_id) references Crews(id) on delete cascade,
	foreign key (user_id) references Users(id) 
);

GO

CREATE INDEX IX_CrewMembers_UserId ON CrewMembers(user_id);

GO 

CREATE INDEX IX_CrewMembers_CrewId ON CrewMembers(crew_id);