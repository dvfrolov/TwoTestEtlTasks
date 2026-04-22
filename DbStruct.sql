/*
GO
CREATE SCHEMA stage;
GO

CREATE TABLE stage.SourceHierarchy (
    Id INT PRIMARY KEY,
    ParentId INT NULL,
    Name NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.TargetHierarchy (
    Id INT PRIMARY KEY,
    ParentId INT NULL,
    Name NVARCHAR(100) NOT NULL,
    FullPath NVARCHAR(1000) NOT NULL
);

CREATE TABLE stage.SourceUsers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NOT NULL
);

CREATE TABLE dbo.TargetUsers (
    Id INT PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL,
    UserName NVARCHAR(100) NOT NULL,
    ModifiedDate DATETIME NOT NULL
);

INSERT INTO stage.SourceHierarchy (Id, ParentId, Name) VALUES
(1, NULL, 'Root'),
(2, 1, 'Dept_IT'),
(3, 1, 'Dept_HR'),
(4, 1, 'Dept_Sales'),
(5, 2, 'Team_Dev'),
(6, 2, 'Team_QA'),
(7, 3, 'Team_Recruitment'),
(8, 4, 'Team_Retail'),
(9, 5, 'Group_Backend'),
(10, 5, 'Group_Frontend'),
(11, 6, 'Group_Auto'),
(12, 6, 'Group_Manual'),
(13, 9, 'Emp_001'),
(14, 9, 'Emp_002'),
(15, 10, 'Emp_003'),
(16, 11, 'Emp_004'),
(17, 12, 'Emp_005'),
(18, 7, 'Emp_006'),
(19, 8, 'Emp_007'),
(20, 8, 'Emp_008'),
(21, 4, 'Team_Wholesale'),
(22, 21, 'Group_Logistics'),
(23, 22, 'Emp_009'),
(24, 3, 'Team_Training'),
(25, 24, 'Emp_010');

INSERT INTO stage.SourceUsers (Email, UserName, ModifiedDate) VALUES
('john@example.com', 'John Doe', '2023-01-01 10:00:00'),
('john@example.com', 'John D.', '2023-01-02 12:00:00'),
('john@example.com', 'J. Doe', '2023-01-03 15:00:00'),
('alice@example.com', 'Alice Smith', '2023-02-01 09:00:00'),
('alice@example.com', 'Alice S.', '2023-02-01 09:00:00'),
('bob@example.com', 'Bob Brown', '2023-03-01 11:00:00'),
('charlie@example.com', 'Charlie White', '2023-04-01 14:00:00'),
('charlie@example.com', 'C. White', '2023-04-02 16:00:00'),
('dave@example.com', 'Dave Black', '2023-05-01 10:00:00'),
('eve@example.com', 'Eve Green', '2023-06-01 10:00:00');

*/