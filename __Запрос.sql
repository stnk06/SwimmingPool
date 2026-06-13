CREATE DATABASE SwimmingManagmentKlimovo;
GO

CREATE TABLE dbo.MembershipTypes (
    MembershipTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Price DECIMAL(10, 2) NOT NULL,
    DurationInDays INT NOT NULL 
);

CREATE TABLE dbo.ActivityTypes (
    ActivityTypeId INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500)
);



CREATE TABLE dbo.Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NOT NULL,
    PassportNumber NVARCHAR(50) UNIQUE NOT NULL,
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255)
);

CREATE TABLE dbo.Trainers (
    TrainerId INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Specialization NVARCHAR(255), -- Специализация тренера
    ExperienceYears INT,
    ContactInfo NVARCHAR(255)
);

CREATE TABLE dbo.Pools (
    PoolId INT IDENTITY(1,1) PRIMARY KEY,
    PoolName NVARCHAR(100) NOT NULL UNIQUE,
    DepthMeters DECIMAL(4,2),
    WorkingHours NVARCHAR(255)
);


CREATE TABLE dbo.Memberships (
    MembershipId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    MembershipTypeId INT NOT NULL,
    PurchaseDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    CONSTRAINT FK_Memberships_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId) ON DELETE CASCADE,
    CONSTRAINT FK_Memberships_MembershipTypes FOREIGN KEY (MembershipTypeId) REFERENCES dbo.MembershipTypes(MembershipTypeId) ON DELETE CASCADE
);

CREATE TABLE dbo.MedicalCheckups (
    MedicalCheckupId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    CheckupDate DATE NOT NULL,
    ExpiryDate DATE NOT NULL,
    IsCleared BIT NOT NULL, 
    Notes NVARCHAR(MAX),
    CONSTRAINT FK_MedicalCheckups_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId) ON DELETE CASCADE
);

CREATE TABLE dbo.Classes (
    ClassId INT IDENTITY(1,1) PRIMARY KEY,
    ActivityTypeId INT NOT NULL,
    TrainerId INT NOT NULL,
    PoolId INT NOT NULL,
    StartTime DATETIME NOT NULL,
    EndTime DATETIME NOT NULL,
    MaxParticipants INT,
    CONSTRAINT FK_Classes_ActivityTypes FOREIGN KEY (ActivityTypeId) REFERENCES dbo.ActivityTypes(ActivityTypeId),
    CONSTRAINT FK_Classes_Trainers FOREIGN KEY (TrainerId) REFERENCES dbo.Trainers(TrainerId),
    CONSTRAINT FK_Classes_Pools FOREIGN KEY (PoolId) REFERENCES dbo.Pools(PoolId)
);

CREATE TABLE dbo.ClassRegistrations (
    RegistrationId INT IDENTITY(1,1) PRIMARY KEY,
    ClientId INT NOT NULL,
    ClassId INT NOT NULL,
    RegistrationDate DATETIME NOT NULL DEFAULT GETDATE(),
    AttendanceStatus NVARCHAR(50) DEFAULT 'Записан', 
    CONSTRAINT FK_ClassRegistrations_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId) ON DELETE CASCADE,
    CONSTRAINT FK_ClassRegistrations_Classes FOREIGN KEY (ClassId) REFERENCES dbo.Classes(ClassId) ON DELETE CASCADE,
    CONSTRAINT UQ_Client_Class UNIQUE (ClientId, ClassId)
);
GO