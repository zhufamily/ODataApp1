/****** Object:  Table [dbo].[Airport]    Script Date: 12/30/2023 2:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Airport](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[City] [nvarchar](50) NOT NULL,
	[CountryID] [int] NOT NULL,
 CONSTRAINT [PK_Airport] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AirportMetric]    Script Date: 12/30/2023 2:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AirportMetric](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AirportID] [int] NOT NULL,
	[Score] [float] NULL,
 CONSTRAINT [PK_AirportMetric] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Country]    Script Date: 12/30/2023 2:20:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Country](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[Abbreviation] [nvarchar](3) NOT NULL,
 CONSTRAINT [PK_Country] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Airport]  WITH CHECK ADD  CONSTRAINT [FK_Airport_Country] FOREIGN KEY([CountryID])
REFERENCES [dbo].[Country] ([ID])
GO
ALTER TABLE [dbo].[Airport] CHECK CONSTRAINT [FK_Airport_Country]
GO
ALTER TABLE [dbo].[AirportMetric]  WITH CHECK ADD  CONSTRAINT [FK_AirportMetric_Airport] FOREIGN KEY([AirportID])
REFERENCES [dbo].[Airport] ([ID])
GO
ALTER TABLE [dbo].[AirportMetric] CHECK CONSTRAINT [FK_AirportMetric_Airport]
GO

