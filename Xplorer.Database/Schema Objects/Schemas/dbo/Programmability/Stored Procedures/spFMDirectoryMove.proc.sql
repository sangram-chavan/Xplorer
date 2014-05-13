CREATE PROCEDURE [dbo].[spFMDirectoryMove]
    @source VARCHAR(MAX) ,
    @target VARCHAR(MAX) ,
    @targetParent VARCHAR(MAX) ,
    @trgDirName VARCHAR(MAX) ,
    @modifiedDateTime DATETIME
AS 
    BEGIN
        DECLARE @parentId BIGINT
        
       
        SELECT  @parentId = A.Id
        FROM    dbo.FileSystem (NOLOCK) AS A
                INNER JOIN dbo.DirPath (NOLOCK) AS B ON A.Id = B.Id
        WHERE   B.FullName = @targetParent 
              
        IF @parentId IS NULL 
            BEGIN
                RETURN
            END 
        UPDATE  dbo.FileSystem
        SET     Name = @trgDirName ,
                ParentId = @parentId ,
                CreationTime = @modifiedDateTime ,
                LastModifiedTime = @modifiedDateTime
        WHERE   Id IN (
                SELECT  A.Id
                FROM    dbo.FileSystem (NOLOCK) AS A
                        INNER JOIN dbo.DirPath (NOLOCK) AS B ON A.Id = B.Id
                WHERE   B.FullName = @source )
                
        UPDATE  dbo.DirPath
        SET     FullName = REPLACE(FullName, @source + '\', @target + '\')
        WHERE   FullName LIKE @source + '\%'

        UPDATE  dbo.DirPath
        SET     FullName = @target
        WHERE   FullName = @source
   
        


    END