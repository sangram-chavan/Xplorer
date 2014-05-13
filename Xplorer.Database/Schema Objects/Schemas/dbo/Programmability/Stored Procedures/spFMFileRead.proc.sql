CREATE PROCEDURE [dbo].[spFMFileRead]
    @DirPath VARCHAR(MAX) ,
    @FileName VARCHAR(MAX)
AS 
    BEGIN
        DECLARE @Id BIGINT 
        
        SELECT  @Id = Id
        FROM    dbo.DirPath (NOLOCK)
        WHERE   FullName = @DirPath

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
                @DirPath + '\' + Name AS FullName
        FROM    dbo.[FileSystem]
        WHERE   ParentId = @Id
                AND Name = @FileName	
            


    END