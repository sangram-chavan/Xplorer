CREATE PROCEDURE [dbo].[spFMGetFilesRecursive] @DirId BIGINT
AS 
    BEGIN

        DECLARE @Tab TABLE
            (
              ROWNUM BIGINT IDENTITY(1, 1)
                            NOT NULL ,
              Id BIGINT
            ) 
        DECLARE @Files TABLE
            (
              Id BIGINT ,
              Name VARCHAR(255) ,
              IsDirectory BIT ,
              Data VARBINARY(MAX) ,
              LastModifiedTime DATETIME ,
              CreationTime DATETIME ,
              Attributes SMALLINT ,
              Extension VARCHAR(10) ,
              IsZipped BIT ,
              IsEncrypted BIT ,
              OriginalSize BIGINT ,
              ParentId BIGINT ,
              Permissions SMALLINT ,
              FullName VARCHAR(MAX)
            )
    

        INSERT  INTO @Tab
        VALUES  ( @DirId )

        DECLARE @MAXCNT BIGINT
        DECLARE @CNT BIGINT
        SET @MAXCNT = 1
        SET @CNT = 1
        DECLARE @FullName VARCHAR(MAX) 
        WHILE ( @CNT <= @MAXCNT ) 
            BEGIN
                SELECT  @DirId = Id
                FROM    @Tab
                WHERE   ROWNUM = @CNT
               
					
                SELECT  @FullName = FullName
                FROM    dbo.DirPath (NOLOCK)
                WHERE   Id = @DirId
                INSERT  INTO @Files
                        ( Id ,
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
                          FullName
                                
                        )
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
                                @FullName + '\' + Name
                        FROM    dbo.FileSystem (NOLOCK)
                        WHERE   ParentId = @DirId
                                AND IsDirectory = 0
                        
                INSERT  INTO @Tab
                        SELECT  Id
                        FROM    dbo.FileSystem (NOLOCK)
                        WHERE   ParentId = @DirId
                                AND IsDirectory = 1
                                AND Id != @DirId
                    
                SET @CNT = @CNT + 1 
                SELECT  @MAXCNT = MAX(ROWNUM)
                FROM    @Tab
            END
    
        SELECT  *
        FROM    @Files
        ORDER BY FullName
    END