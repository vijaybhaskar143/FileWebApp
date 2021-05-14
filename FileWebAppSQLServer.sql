USE [TELERIK]
GO

/****** Object:  Table [dbo].[FilesData]    Script Date: 14-05-2021 12:53:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FilesData](
	[ItemID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[ParentID] [int] NULL,
	[FileType] [nvarchar](50) NULL,
	[IsDirectory] [tinyint] NOT NULL,
	[FileSize] [int] NULL,
	[FileContent] [image] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


INSERT INTO FilesData VALUES ('ROOT',NULL,NULL,1,NULL,NULL)
INSERT INTO FilesData VALUES ('FILES',1,NULL,1,NULL,NULL)

GO
