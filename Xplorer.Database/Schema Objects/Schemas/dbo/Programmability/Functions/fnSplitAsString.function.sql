CREATE   FUNCTION [dbo].[fnSplitAsString](@list NVARCHAR(MAX), @delimChar nvarchar(1))
	RETURNS @tokens TABLE ([token] nvarchar(1000))

AS 

BEGIN
	DECLARE @token AS nvarchar(1000)
		DECLARE	@delimPos AS int

	SET @token = 0
	IF (RIGHT(@list, 1) <> @delimChar)
		SET @list = @list + @delimChar

	WHILE LEN(@list) <> 0
	BEGIN			
		SET @delimPos = CHARINDEX(@delimChar, @list)
		SET @token = SUBSTRING(@list, 0, @delimPos)
		SET @list = SUBSTRING(@list, @delimPos + 1, LEN(@list) - @delimPos + 1)
		INSERT @tokens VALUES (@token)
	END
	RETURN
END