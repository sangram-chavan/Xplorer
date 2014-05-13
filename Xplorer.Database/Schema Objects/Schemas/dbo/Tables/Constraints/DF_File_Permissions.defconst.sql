ALTER TABLE [dbo].[FileSystem]
    ADD CONSTRAINT [DF_File_Permissions] DEFAULT ((3)) FOR [Permissions];

