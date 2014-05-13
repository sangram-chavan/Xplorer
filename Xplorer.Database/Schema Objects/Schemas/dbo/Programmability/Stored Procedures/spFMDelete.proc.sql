CREATE PROCEDURE [dbo].[spFMDelete] @ObjectId BIGINT
AS 
    BEGIN
        DECLARE @Tab TABLE
            (
              ROWNUM BIGINT IDENTITY(1, 1)
                            NOT NULL ,
              Id BIGINT
            ) 
        DECLARE @Files TABLE ( Id BIGINT )
    

        INSERT  INTO @Tab
        VALUES  ( @ObjectId )

        DECLARE @MAXCNT BIGINT
        DECLARE @CNT BIGINT
        SET @MAXCNT = 1
        SET @CNT = 1
        WHILE ( @CNT <= @MAXCNT ) 
            BEGIN
                SELECT  @ObjectId = Id
                FROM    @Tab
                WHERE   ROWNUM = @CNT

                
					
				
                INSERT  INTO @Files
                        SELECT  Id
                        FROM    dbo.FileSystem (NOLOCK)
                        WHERE   Id = @ObjectId
                                OR ( ParentId = @ObjectId
                                     AND IsDirectory = 0
                                   )
                        
                INSERT  INTO @Tab
                        SELECT  Id
                        FROM    dbo.FileSystem (NOLOCK)
                        WHERE   ParentId = @ObjectId
                                AND IsDirectory = 1
                    
                SET @CNT = @CNT + 1 
                SELECT  @MAXCNT = MAX(ROWNUM)
                FROM    @Tab
            END
    
        
        DELETE  FROM dbo.FileSystem
        WHERE   id IN ( SELECT  ID
                        FROM    @Files ) 

    END