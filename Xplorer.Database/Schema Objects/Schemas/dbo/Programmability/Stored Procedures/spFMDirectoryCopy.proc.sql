CREATE PROCEDURE [dbo].[spFMDirectoryCopy]
    @SourceId BIGINT ,
    @TargeId BIGINT ,
    @TargetPath VARCHAR(MAX) ,
    @LastModifiedTime DATETIME ,
    @CreationTime DATETIME 
AS 
    BEGIN
        DECLARE @TempTable TABLE
            (
              ROWNUM BIGINT ,
              Id BIGINT ,
              Name VARCHAR(255) ,
              IsDirectory BIT
            )
       
        INSERT  INTO @TempTable
                SELECT  ROW_NUMBER() OVER ( ORDER BY ID ASC ) AS ROWNUM ,
                        Id ,
                        Name ,
                        IsDirectory
                FROM    dbo.FileSystem (NOLOCK)
                WHERE   ParentId = @SourceId
            
        DECLARE @ObjectId BIGINT ,
            @Name VARCHAR(255) ,
            @IsDirectory BIT ,
            @ParentId BIGINT ,
            @MAXCNT BIGINT ,
            @CNT BIGINT 

        SELECT  @MAXCNT = MAX(ROWNUM)
        FROM    @TempTable
        
        SET @CNT = 1
        WHILE ( @CNT <= @MAXCNT ) 
            BEGIN
                BEGIN
                    SELECT  @ObjectId = Id ,
                            @IsDirectory = IsDirectory ,
                            @Name = Name
                    FROM    @TempTable
                    WHERE   ROWNUM = @CNT
                    
            	
            
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
                            SELECT  Name ,
                                    IsDirectory ,
                                    Data ,
                                    @LastModifiedTime ,
                                    @CreationTime ,
                                    Attributes ,
                                    Extension ,
                                    IsZipped ,
                                    IsEncrypted ,
                                    OriginalSize ,
                                    @TargeId ,
                                    Permissions
                            FROM    dbo.FileSystem (NOLOCK)
                            WHERE   Id = @ObjectId 
                    SET @ParentId = SCOPE_IDENTITY()
				
                    IF @IsDirectory = 1 
                        BEGIN
                            
                            INSERT  INTO dbo.DirPath
                                    ( Id ,
                                      FullName 
                                            
                                    )
                            VALUES  ( @ObjectId ,
                                      @TargetPath + '\' + @Name 
                                            
                                    )
                            DECLARE @tempPath VARCHAR(MAX)
                            SET @tempPath = @TargetPath + '\' + @Name 
                            EXEC dbo.[spFMDirectoryCopy] @ObjectId, @ParentId,
                                @tempPath, @LastModifiedTime, @CreationTime
                                
                        END
                    
                END 
	
                SET @CNT = @CNT + 1 

            END
    END