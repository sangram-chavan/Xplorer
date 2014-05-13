CREATE PROCEDURE [dbo].[spFMGetFiles] @DirId BIGINT
AS 
    BEGIN
 
        DECLARE @FullName VARCHAR(MAX)
        
        SELECT  @FullName = FullName
        FROM    dbo.DirPath (NOLOCK)
        WHERE   Id = @DirId
        SELECT  Id ,
                Name ,
                IsDirectory ,
                NULL ,
                LastModifiedTime ,
                CreationTime ,
                Attributes ,
                Extension ,
                IsZipped ,
                IsEncrypted ,
                OriginalSize ,
                ParentId ,
                Permissions ,
                @FullName + '\' + Name AS FullName
        FROM    dbo.FileSystem (NOLOCK)
        WHERE   ParentId = @DirId
                AND IsDirectory = 0
        ORDER BY FullName
        	
  
    END