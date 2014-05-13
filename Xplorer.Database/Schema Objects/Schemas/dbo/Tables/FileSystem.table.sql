CREATE TABLE [dbo].[FileSystem] (
    [Id]               BIGINT          IDENTITY (1, 1) NOT NULL,
    [Name]             VARCHAR (255)   NOT NULL,
    [IsDirectory]      BIT             NULL,
    [Data]             VARBINARY (MAX) NULL,
    [LastModifiedTime] DATETIME        NULL,
    [CreationTime]     DATETIME        NULL,
    [Attributes]       SMALLINT        NULL,
    [Extension]        NCHAR (10)      NULL,
    [IsZipped]         BIT             NULL,
    [IsEncrypted]      BIT             NULL,
    [OriginalSize]     BIGINT          NULL,
    [ParentId]         BIGINT          NOT NULL,
    [Permissions]      SMALLINT        NOT NULL
);

