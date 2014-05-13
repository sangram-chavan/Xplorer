CREATE PROCEDURE [dbo].[spFMFileCopy]
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
        INSERT  INTO dbo.FileSystem
                ( Name ,
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
                  Permissions
                        
                )
                SELECT  @tgtFileName ,
                        IsDirectory ,
                        Data ,
                        @modifiedDateTime ,
                        @modifiedDateTime ,
                        Attributes ,
                        @tgtExtension ,
                        IsZipped ,
                        IsEncrypted ,
                        OriginalSize ,
                        @tgtId ,
                        Permissions
                FROM    dbo.FileSystem (NOLOCK)
                WHERE   ParentId = @srcId
                        AND Name = @srcFileName
                        AND IsDirectory = 0
     
        
 
    END