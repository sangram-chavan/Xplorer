CREATE PROCEDURE [dbo].[spFMGetDirectories] @DirId BIGINT
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
                ( CASE WHEN @FullName = Name THEN Name
                       ELSE @FullName + '\' + Name
                  END ) FullName
        FROM    dbo.FileSystem (NOLOCK)
        WHERE   ParentId = @DirId
                AND IsDirectory = 1
        ORDER BY FullName
            
    END