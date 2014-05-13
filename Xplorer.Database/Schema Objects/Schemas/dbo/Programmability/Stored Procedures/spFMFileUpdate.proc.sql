CREATE PROCEDURE [dbo].[spFMFileUpdate]
    @DirPath VARCHAR(MAX) ,
    @FileName VARCHAR(MAX) ,
    @Data VARBINARY(MAX) ,
    @LastModifiedTime DATETIME ,
    @Attributes SMALLINT ,
    @Extension VARCHAR(10) ,
    @IsZipped BIT ,
    @IsEncrypted BIT ,
    @OriginalSize BIGINT ,
    @Permissions SMALLINT
AS 
    DECLARE @Id BIGINT 
        
    
    SELECT  @Id = Id
    FROM    dbo.DirPath (NOLOCK)
    WHERE   FullName = @DirPath
    IF NOT EXISTS ( SELECT  *
                    FROM    dbo.FileSystem (NOLOCK)
                    WHERE   ParentId = @Id
                            AND IsDirectory = 0
                            AND Name = @FileName ) 
        BEGIN
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
            VALUES  ( @FileName ,
                      0 ,
                      @Data ,
                      @LastModifiedTime ,
                      @LastModifiedTime ,
                      @Attributes ,
                      @Extension ,
                      @IsZipped ,
                      @IsEncrypted ,
                      @OriginalSize ,
                      @Id ,
                      @Permissions
		                    
                            
                    )
        END
    ELSE 
        BEGIN
    		
    	
            UPDATE  dbo.FileSystem
            SET     Data = @Data ,
                    Attributes = @Attributes ,
                    LastModifiedTime = @LastModifiedTime ,
                    Extension = @Extension ,
                    IsZipped = @IsZipped ,
                    IsEncrypted = @IsEncrypted ,
                    OriginalSize = @OriginalSize ,
                    Permissions = @Permissions
            WHERE   ParentId = @Id
                    AND IsDirectory = 0
                    AND Name = @FileName
        END
  