--EXEC dbo.sp_GetArtifactsForUser 1,'Widget'


CREATE FUNCTION [dbo].[fn_GetUserArtifact]
    (
      @UserID BIGINT,
      @Artifact VARCHAR(50)
    )
RETURNS @tempTable TABLE
    (
      Artifact VARCHAR(50) NOT NULL,
      ArtifactId BIGINT NOT NULL,
      UserRight BIT,
      InheritedRight BIT NOT NULL
    )
AS 
BEGIN
	
    DECLARE @RoleId BIGINT
    SELECT  @RoleId = RoleId
    FROM    users
    WHERE   [Id] = @UserID
    IF @Artifact IS NULL OR
        @Artifact = '' 
        BEGIN
            INSERT  INTO @tempTable
                    SELECT  Artifact,
                            ArtifactId,
                            UserRight,
                            InheritedRight
                    FROM    (
                              SELECT    CASE WHEN A.Artifact IS NULL
                                             THEN B.Artifact
                                             ELSE A.Artifact
                                        END AS "Artifact",
                                        CASE WHEN A.ArtifactId IS NULL
                                             THEN B.ArtifactId
                                             ELSE A.ArtifactId
                                        END AS "ArtifactId",
                                        A.[Viewable] AS "UserRight",
                                        ISNULL(B.[Viewable], 'FALSE') AS "InheritedRight"
                              FROM      (
                                          SELECT    [Id],
                                                    [UserId],
                                                    [Artifact],
                                                    [ArtifactId],
                                                    [Viewable],
                                                    [ModifiedDate],
                                                    [GivenByUser]
                                          FROM      [AccessRightsForUser]
                                          WHERE     [UserId] = @UserID
                                        ) AS A FULL OUTER JOIN
                                        (
                                          SELECT    [Id],
                                                    [RoleID],
                                                    [Artifact],
                                                    [ArtifactId],
                                                    [Viewable],
                                                    [ModifiedDate],
                                                    [GivenByUser]
                                          FROM      [AccessRightsForRole] AS Tab1
                                          WHERE     [RoleID] = @RoleId
                                        ) AS B
                                        ON A.[Artifact] = B.[Artifact] AND
                                           A.[ArtifactId] = B.[ArtifactId]
                            ) AS TempTab
                    ORDER BY [Artifact],
                            ArtifactId
                          
							
                               
        END
    ELSE 
        BEGIN
            INSERT  INTO @tempTable
                    SELECT  @Artifact AS "Artifact",
                            CASE WHEN A.ArtifactId IS NULL THEN B.ArtifactId
                                 ELSE A.ArtifactId
                            END AS "ArtifactId",
                            A.[Viewable] AS "UserRight",
                            ISNULL(B.[Viewable], 'FALSE') AS "InheritedRight"
                    FROM    (
                              SELECT    [Id],
                                        [UserId],
                                        [Artifact],
                                        [ArtifactId],
                                        [Viewable],
                                        [ModifiedDate],
                                        [GivenByUser]
                              FROM      [AccessRightsForUser]
                              WHERE     [UserId] = @UserID
                            ) AS A FULL OUTER JOIN
                            (
                              SELECT    [Id],
                                        [RoleID],
                                        [Artifact],
                                        [ArtifactId],
                                        [Viewable],
                                        [ModifiedDate],
                                        [GivenByUser]
                              FROM      [AccessRightsForRole] AS Tab1
                              WHERE     [RoleID] = @RoleId
                            ) AS B
                            ON A.[Artifact] = B.[Artifact] AND
                               A.[ArtifactId] = B.[ArtifactId]
                    WHERE   A.Artifact = @Artifact OR
                            B.Artifact = @Artifact
        END
            
    RETURN 
END