--EXEC dbo.sp_GetArtifactsForUser 1,'Widget'


CREATE FUNCTION [dbo].[fn_GetRoleArtifact]
(
  @RoleID BIGINT,
  @Artifact VARCHAR(50)
)
RETURNS @tempTable TABLE
(
  Artifact VARCHAR(50) NOT NULL,
  ArtifactId BIGINT NOT NULL,
  RoleRight BIT NOT NULL
)
AS 
BEGIN
           
    IF @Artifact IS NULL OR
       @Artifact = '' 
       BEGIN           
             INSERT INTO
                @tempTable
                SELECT
                    [Artifact],
                    [ArtifactId],
                    [Viewable] AS RoleRight
                FROM
                    [AccessRightsForRole]
                WHERE
                    [RoleID] = @RoleId
                ORDER BY
                    [Artifact],
                    [ArtifactId]
       END
    ELSE 
       BEGIN
             INSERT INTO
                @tempTable
                SELECT
                    [Artifact],
                    [ArtifactId],
                    [Viewable] AS RoleRight
                FROM
                    [AccessRightsForRole]
                WHERE
                    [RoleID] = @RoleId AND
                    [Artifact] = @Artifact 

			
       END
            
    RETURN 
END