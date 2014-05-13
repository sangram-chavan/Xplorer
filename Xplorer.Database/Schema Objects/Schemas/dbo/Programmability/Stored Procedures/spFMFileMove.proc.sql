CREATE PROCEDURE [dbo].[spFMFileMove]
    @srcFileName VARCHAR(MAX) ,
    @srcDirFullName VARCHAR(MAX) ,
    @tgtFileName VARCHAR(MAX) ,
    @tgtExtension VARCHAR(10) ,
    @tgtDirFullName VARCHAR(MAX) ,
    @modifiedDateTime DATETIME
AS 
    BEGIN
    
        DECLARE @srcId BIGINT ,
            @tgtId BIGINT
            
        
        SELECT  @srcId = Id
        FROM    dbo.DirPath (NOLOCK)
        WHERE   FullName = @srcDirFullName

        SELECT  @tgtId = Id
        FROM    dbo.DirPath (NOLOCK)
        WHERE   FullName = @tgtDirFullName


        IF @srcId IS NULL
            OR @tgtId IS NULL 
            BEGIN
                RETURN 0
            END 
            
        UPDATE  dbo.FileSystem
        SET     Name = @tgtFileName ,
                Extension = @tgtExtension ,
                ParentId = @tgtId ,
                LastModifiedTime = @modifiedDateTime
        WHERE   ParentId = @srcId
                AND Name = @srcFileName
                AND IsDirectory = 0
             
       
    END