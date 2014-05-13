CREATE PROCEDURE [dbo].[spFMGetObject]
    @DirPath VARCHAR(MAX) ,
    @FileName VARCHAR(MAX) ,
    @IsDirectory BIT
AS 
    BEGIN
        DECLARE @Id BIGINT 
        
        SELECT  @Id = Id
        FROM    dbo.DirPath (NOLOCK)
        WHERE   FullName = @DirPath
        IF @IsDirectory IS NULL 
            BEGIN
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
        ELSE 
            BEGIN
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
                        @DirPath + '\' + Name AS FullName
                FROM    dbo.[FileSystem]
                WHERE   ParentId = @Id
                        AND IsDirectory = @IsDirectory
                        AND Name = @FileName

     	
            END

     
        

    END