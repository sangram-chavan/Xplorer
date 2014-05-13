CREATE PROCEDURE [dbo].[spFMDirectoryCreate]
    @Name VARCHAR(255) ,
    @Data VARBINARY(MAX) ,
    @CreationTime DATETIME ,
    @Attributes SMALLINT ,
    @OriginalSize BIGINT ,
    @ParentId BIGINT ,
    @Permissions SMALLINT ,
    @FullName VARCHAR(MAX)
AS 
    BEGIN
        DECLARE @Id BIGINT
        
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
        VALUES  ( @Name ,
                  1 ,
                  NULL ,
                  @CreationTime ,
                  @CreationTime ,
                  @Attributes ,
                  '' ,
                  0 ,
                  0 ,
                  0 ,
                  @ParentId ,
                  @Permissions
                        
                )
        SET @Id = SCOPE_IDENTITY()
        
        INSERT  INTO dbo.DirPath
                ( Id, FullName )
        VALUES  ( @Id, @FullName )

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
                @FullName AS FullName
        FROM    dbo.[FileSystem]
        WHERE   Id = @Id
           

    END