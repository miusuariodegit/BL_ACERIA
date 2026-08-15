SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET XACT_ABORT ON;
GO

-- [GPX-DOC-v1] Script T-SQL idempotente que recrea el esquema de ASP.NET Core Identity y las
-- tablas propias de navegacion (AppProfiles/AppModules/AppProfileModules), y siembra los datos
-- minimos para el primer arranque de la aplicacion (perfil, modulo, rol, usuario y claim iniciales).
/*
Script base GPX
- Recrea toda la estructura necesaria para ASP.NET Core Identity + GPX
- Borra tablas existentes si ya estan creadas
- Crea AppProfiles, AppModules, AppProfileModules y tablas Identity
- Inserta datos minimos de perfil, modulo, rol, usuario y claim para arranque PRO
- Deja listo el menu dinamico padre/hijo desde AppModules
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.AspNetUserTokens', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserTokens;
    IF OBJECT_ID(N'dbo.AspNetUserLogins', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserLogins;
    IF OBJECT_ID(N'dbo.AspNetUserClaims', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserClaims;
    IF OBJECT_ID(N'dbo.AspNetUserRoles', N'U') IS NOT NULL DROP TABLE dbo.AspNetUserRoles;
    IF OBJECT_ID(N'dbo.AspNetRoleClaims', N'U') IS NOT NULL DROP TABLE dbo.AspNetRoleClaims;
    IF OBJECT_ID(N'dbo.AspNetUsers', N'U') IS NOT NULL DROP TABLE dbo.AspNetUsers;
    IF OBJECT_ID(N'dbo.AspNetRoles', N'U') IS NOT NULL DROP TABLE dbo.AspNetRoles;
    IF OBJECT_ID(N'dbo.AppProfileModules', N'U') IS NOT NULL DROP TABLE dbo.AppProfileModules;
    IF OBJECT_ID(N'dbo.AppModules', N'U') IS NOT NULL DROP TABLE dbo.AppModules;
    IF OBJECT_ID(N'dbo.AppProfiles', N'U') IS NOT NULL DROP TABLE dbo.AppProfiles;
    CREATE TABLE dbo.AppProfiles (
        Id INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        CONSTRAINT PK_AppProfiles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX IX_AppProfiles_Name
        ON dbo.AppProfiles(Name);

    CREATE TABLE dbo.AppModules (
        Id INT IDENTITY(1,1) NOT NULL,
        Code NVARCHAR(50) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Route NVARCHAR(150) NOT NULL,
        Description NVARCHAR(250) NOT NULL,
        IconCssClass NVARCHAR(150) NOT NULL,
        ParentCode NVARCHAR(50) NOT NULL,
        ParentName NVARCHAR(100) NOT NULL,
        ParentIconCssClass NVARCHAR(150) NOT NULL,
        ParentDisplayOrder INT NOT NULL,
        DisplayOrder INT NOT NULL,
        IsEnabled BIT NOT NULL,
        CONSTRAINT PK_AppModules PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX IX_AppModules_Code
        ON dbo.AppModules(Code);

    CREATE UNIQUE INDEX IX_AppModules_Route
        ON dbo.AppModules(Route);

    CREATE TABLE dbo.AspNetRoles (
        Id NVARCHAR(450) NOT NULL,
        Name NVARCHAR(256) NULL,
        NormalizedName NVARCHAR(256) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
    );

    CREATE UNIQUE INDEX RoleNameIndex
        ON dbo.AspNetRoles(NormalizedName)
        WHERE NormalizedName IS NOT NULL;

    CREATE TABLE dbo.AspNetUsers (
        Id NVARCHAR(450) NOT NULL,
        UserName NVARCHAR(256) NULL,
        NormalizedUserName NVARCHAR(256) NULL,
        Email NVARCHAR(256) NULL,
        NormalizedEmail NVARCHAR(256) NULL,
        EmailConfirmed BIT NOT NULL,
        PasswordHash NVARCHAR(MAX) NULL,
        SecurityStamp NVARCHAR(MAX) NULL,
        ConcurrencyStamp NVARCHAR(MAX) NULL,
        PhoneNumber NVARCHAR(MAX) NULL,
        PhoneNumberConfirmed BIT NOT NULL,
        TwoFactorEnabled BIT NOT NULL,
        LockoutEnd DATETIMEOFFSET(7) NULL,
        LockoutEnabled BIT NOT NULL,
        AccessFailedCount INT NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        ProfileId INT NULL,
        CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id),
        CONSTRAINT FK_AspNetUsers_AppProfiles_ProfileId
            FOREIGN KEY (ProfileId) REFERENCES dbo.AppProfiles(Id)
    );

    CREATE INDEX EmailIndex
        ON dbo.AspNetUsers(NormalizedEmail);

    CREATE UNIQUE INDEX UserNameIndex
        ON dbo.AspNetUsers(NormalizedUserName)
        WHERE NormalizedUserName IS NOT NULL;

    CREATE INDEX IX_AspNetUsers_ProfileId
        ON dbo.AspNetUsers(ProfileId);

    CREATE TABLE dbo.AppProfileModules (
        ProfileId INT NOT NULL,
        ModuleId INT NOT NULL,
        CONSTRAINT PK_AppProfileModules PRIMARY KEY (ProfileId, ModuleId),
        CONSTRAINT FK_AppProfileModules_AppProfiles_ProfileId
            FOREIGN KEY (ProfileId) REFERENCES dbo.AppProfiles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AppProfileModules_AppModules_ModuleId
            FOREIGN KEY (ModuleId) REFERENCES dbo.AppModules(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AppProfileModules_ModuleId
        ON dbo.AppProfileModules(ModuleId);

    CREATE TABLE dbo.AspNetRoleClaims (
        Id INT IDENTITY(1,1) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
        CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetRoleClaims_RoleId
        ON dbo.AspNetRoleClaims(RoleId);

    CREATE TABLE dbo.AspNetUserClaims (
        Id INT IDENTITY(1,1) NOT NULL,
        UserId NVARCHAR(450) NOT NULL,
        ClaimType NVARCHAR(MAX) NULL,
        ClaimValue NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
        CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserClaims_UserId
        ON dbo.AspNetUserClaims(UserId);

    CREATE TABLE dbo.AspNetUserLogins (
        LoginProvider NVARCHAR(128) NOT NULL,
        ProviderKey NVARCHAR(128) NOT NULL,
        ProviderDisplayName NVARCHAR(MAX) NULL,
        UserId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
        CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserLogins_UserId
        ON dbo.AspNetUserLogins(UserId);

    CREATE TABLE dbo.AspNetUserRoles (
        UserId NVARCHAR(450) NOT NULL,
        RoleId NVARCHAR(450) NOT NULL,
        CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId
            FOREIGN KEY (RoleId) REFERENCES dbo.AspNetRoles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_AspNetUserRoles_RoleId
        ON dbo.AspNetUserRoles(RoleId);

    CREATE TABLE dbo.AspNetUserTokens (
        UserId NVARCHAR(450) NOT NULL,
        LoginProvider NVARCHAR(128) NOT NULL,
        Name NVARCHAR(128) NOT NULL,
        Value NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
        CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId
            FOREIGN KEY (UserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE
    );

    INSERT INTO dbo.AppProfiles (Name, Description)
    VALUES
        (N'Administrador', N'Perfil principal con acceso completo al modulo inicial.');

    INSERT INTO dbo.AppModules (
        Code,
        Name,
        Route,
        Description,
        IconCssClass,
        ParentCode,
        ParentName,
        ParentIconCssClass,
        ParentDisplayOrder,
        DisplayOrder,
        IsEnabled
    )
    VALUES
        (N'PRODUCTION_HOME', N'Inicio', N'/modulos/produccion', N'Vista inicial del modulo de produccion.', N'icon medium-icon data-area-icon', N'PRODUCTION', N'Produccion', N'icon medium-icon data-area-icon', 1, 1, 1);

    INSERT INTO dbo.AppProfileModules (ProfileId, ModuleId)
    SELECT p.Id, m.Id
    FROM dbo.AppProfiles p
    CROSS JOIN dbo.AppModules m
    WHERE p.Name = N'Administrador';

    INSERT INTO dbo.AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES
        (N'bb1ce524-89e9-4f56-a1b8-1f0a4bd3402f', N'Administrador', N'ADMINISTRADOR', N'1b87f956-b443-4f5d-a8e9-48c47f5f3142');

    INSERT INTO dbo.AspNetUsers (
        Id,
        UserName,
        NormalizedUserName,
        Email,
        NormalizedEmail,
        EmailConfirmed,
        PasswordHash,
        SecurityStamp,
        ConcurrencyStamp,
        PhoneNumber,
        PhoneNumberConfirmed,
        TwoFactorEnabled,
        LockoutEnd,
        LockoutEnabled,
        AccessFailedCount,
        FullName,
        ProfileId
    )
    VALUES
        (
            N'8d4d30d3-a305-42cc-aa22-b6f0c764b327',
            N'vhernandez@consultoria-it.com',
            N'VHERNANDEZ@CONSULTORIA-IT.COM',
            N'vhernandez@consultoria-it.com',
            N'VHERNANDEZ@CONSULTORIA-IT.COM',
            1,
            N'AQAAAAIAAYagAAAAEGj/iLmm/bFL/19zfbiOYqGUZt0c4SPwdj/V9hF6av+O2LNgEaYBlSEavxDyASFpJg==',
            N'e74bcf66-1380-4d4d-9278-e9c64d016e86',
            N'82b44665-bdd0-406d-89d7-b015403e0867',
            NULL,
            0,
            0,
            NULL,
            1,
            0,
            N'Vicente Hernandez',
            (SELECT TOP 1 Id FROM dbo.AppProfiles WHERE Name = N'Administrador')
        );

    INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
    VALUES
        (N'8d4d30d3-a305-42cc-aa22-b6f0c764b327', N'bb1ce524-89e9-4f56-a1b8-1f0a4bd3402f');

    INSERT INTO dbo.AspNetRoleClaims (RoleId, ClaimType, ClaimValue)
    VALUES
        (N'bb1ce524-89e9-4f56-a1b8-1f0a4bd3402f', N'app:permission', N'production:access');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

PRINT 'Script base GPX recreado correctamente.';
PRINT 'Usuario inicial:';
PRINT '  vhernandez@consultoria-it.com / Inteldx486.@';
PRINT 'Rol inicial:';
PRINT '  Administrador';
PRINT 'Claim inicial:';
PRINT '  AspNetRoleClaims -> Administrador / app:permission / production:access';
