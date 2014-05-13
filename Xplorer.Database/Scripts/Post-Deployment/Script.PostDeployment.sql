/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

USE [dFileManager];
GO
TRUNCATE TABLE dbo.FileSystem
TRUNCATE TABLE dbo.DirPath

BEGIN TRANSACTION;
INSERT INTO [dbo].[DirPath]([Id], [FullName])
SELECT 0, N'D:' UNION ALL
SELECT 1, N'D:\Uploads'
COMMIT;
GO

SET IDENTITY_INSERT [dbo].[FileSystem] ON;

BEGIN TRANSACTION;
INSERT INTO [dbo].[FileSystem]([Id], [Name], [IsDirectory], [Data], [LastModifiedTime], [CreationTime], [Attributes], [Extension], [IsZipped], [IsEncrypted], [OriginalSize], [ParentId], [Permissions])
SELECT 0, N'D:', 1, NULL, '20110401 11:35:35.480', '20110401 11:35:35.480', 144, N'', 0, 0, 0, 0, 3 UNION ALL
SELECT 1, N'Uploads', 1, NULL, '20110401 11:35:35.480', '20110401 11:35:35.480', 144, N'', 0, 0, 0, 0, 3
COMMIT;
GO

SET IDENTITY_INSERT [dbo].[FileSystem] OFF;