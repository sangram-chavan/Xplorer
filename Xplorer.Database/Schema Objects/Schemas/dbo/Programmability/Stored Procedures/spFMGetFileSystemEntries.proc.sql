CREATE PROCEDURE [dbo].[spFMGetFileSystemEntries] @DirId BIGINT
AS 
    BEGIN
 
        DECLARE @FullName VARCHAR(MAX)
       
        SELECT  @FullName = FullName
        FROM    dbo.DirPath (NOLOCK)
        WHERE   Id = @DirId
        
        SELECT  Id ,
                Name ,
                IsDirectory ,
                Data ,
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
        ORDER BY IsDirectory DESC ,
                FullName 
	
       
    END