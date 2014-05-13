ALTER TABLE [dbo].[FileSystem]
    ADD CONSTRAINT [DF_File_ParentId] DEFAULT ((0)) FOR [ParentId];

