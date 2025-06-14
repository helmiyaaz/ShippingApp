USE [master]
GO
/****** Object:  Database [OPS_PROD]    Script Date: 12/06/2025 10:11:10 ******/
CREATE DATABASE [OPS_PROD]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'OPS_PROD', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\OPS_PROD.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'OPS_PROD_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\OPS_PROD_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [OPS_PROD] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [OPS_PROD].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [OPS_PROD] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [OPS_PROD] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [OPS_PROD] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [OPS_PROD] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [OPS_PROD] SET ARITHABORT OFF 
GO
ALTER DATABASE [OPS_PROD] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [OPS_PROD] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [OPS_PROD] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [OPS_PROD] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [OPS_PROD] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [OPS_PROD] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [OPS_PROD] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [OPS_PROD] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [OPS_PROD] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [OPS_PROD] SET  DISABLE_BROKER 
GO
ALTER DATABASE [OPS_PROD] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [OPS_PROD] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [OPS_PROD] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [OPS_PROD] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [OPS_PROD] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [OPS_PROD] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [OPS_PROD] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [OPS_PROD] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [OPS_PROD] SET  MULTI_USER 
GO
ALTER DATABASE [OPS_PROD] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [OPS_PROD] SET DB_CHAINING OFF 
GO
ALTER DATABASE [OPS_PROD] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [OPS_PROD] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [OPS_PROD] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [OPS_PROD] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [OPS_PROD] SET QUERY_STORE = ON
GO
ALTER DATABASE [OPS_PROD] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [OPS_PROD]
GO
/****** Object:  Table [dbo].[DMSFGStock]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMSFGStock](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Part_Number] [varchar](128) NULL,
	[Description] [varchar](128) NULL,
	[Order_Number] [varchar](128) NULL,
	[Batch_Number] [varchar](128) NULL,
	[Quantity] [decimal](18, 0) NULL,
	[Field1] [varchar](128) NULL,
	[Field2] [varchar](128) NULL,
	[Field3] [varchar](128) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMSMasterShipPlan]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMSMasterShipPlan](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Part_Number] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[Customer] [varchar](max) NULL,
	[Qty] [numeric](18, 0) NULL,
	[PSD] [date] NULL,
	[COS] [decimal](18, 0) NULL,
	[Ttl_COS] [decimal](18, 0) NULL,
 CONSTRAINT [PK_DMSMasterShipPlan] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMSPortal]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMSPortal](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Part_Number] [varchar](50) NULL,
	[Description] [varchar](max) NULL,
	[PO] [varchar](50) NULL,
	[Sch_Line] [varchar](50) NULL,
	[Qty] [int] NULL,
	[Customer] [varchar](50) NULL,
	[ASN] [varchar](50) NULL,
	[Req_Date] [date] NULL,
	[OTD_Date] [date] NULL,
	[Commit_Date] [date] NULL,
	[Remarks] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMSShipmentLog]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMSShipmentLog](
	[id_log] [int] IDENTITY(1,1) NOT NULL,
	[Doc_Number] [varchar](128) NULL,
	[SO] [varchar](128) NULL,
	[SO_Line] [varchar](128) NULL,
	[PO] [varchar](128) NULL,
	[PO_Line] [varchar](128) NULL,
	[Part_Number] [varchar](128) NULL,
	[Description] [varchar](128) NULL,
	[Qty] [decimal](18, 0) NULL,
	[Batch_Number] [varchar](128) NULL,
	[Serial_Number] [varchar](128) NULL,
	[Customer] [varchar](128) NULL,
	[Delivery_Point] [varchar](128) NULL,
	[ID] [varchar](128) NULL,
	[SSD] [datetime] NULL,
	[LSD] [datetime] NULL,
	[CRD] [datetime] NULL,
	[Plan_Ship_Date] [datetime] NULL,
	[Week] [varchar](128) NULL,
	[Act_Ship_Date] [datetime] NULL,
	[Status] [varchar](128) NULL,
	[COS] [decimal](18, 2) NULL,
	[Ttl_COS] [decimal](18, 2) NULL,
	[Price] [decimal](18, 2) NULL,
	[Ttl_Price] [decimal](18, 2) NULL,
	[Weight] [decimal](18, 2) NULL,
	[Ttl_Weight] [decimal](18, 2) NULL,
	[Ctn_Number] [numeric](18, 0) NULL,
	[Mode] [varchar](128) NULL,
	[DN] [varchar](128) NULL,
	[ASN] [varchar](128) NULL,
	[AWB] [varchar](128) NULL,
	[Ship_Number] [varchar](128) NULL,
	[Bill_Doc] [varchar](128) NULL,
	[Shipper] [varchar](128) NULL,
	[POD] [varchar](128) NULL,
	[Remarks] [varchar](128) NULL,
	[Drawing_Rev] [varchar](50) NULL,
	[PO_Rev] [varchar](50) NULL,
	[Concession] [varchar](max) NULL,
	[Production_Permit] [varchar](max) NULL,
	[KFR] [varchar](max) NULL,
	[Special_Process] [varchar](max) NULL,
	[Length] [numeric](18, 0) NULL,
	[Width] [numeric](18, 0) NULL,
	[Height] [numeric](18, 0) NULL,
	[CoC_By] [varchar](max) NULL,
	[PEB] [varchar](max) NULL,
	[PEB_Date] [date] NULL,
	[Lot_Number] [varchar](50) NULL,
	[EmailNotification] [varchar](50) NULL,
	[POD_Date] [date] NULL,
	[Dlv_LT] [numeric](18, 0) NULL,
	[Batch_Mtl] [varchar](50) NULL,
	[CoC_Date] [date] NULL,
	[LessKITE_Reason] [varchar](max) NULL,
	[PO_Type] [varchar](50) NULL,
	[Est_Dlv_Cost] [numeric](18, 0) NULL,
	[dt_mfg] [varchar](10) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DMSWeeklyShipPlan]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DMSWeeklyShipPlan](
	[SO] [varchar](max) NULL,
	[SO_Line] [varchar](max) NULL,
	[PO] [varchar](max) NULL,
	[PO_Line] [varchar](max) NULL,
	[Part_Number] [varchar](max) NULL,
	[Description] [varchar](max) NULL,
	[Qty] [numeric](18, 0) NULL,
	[Customer] [varchar](max) NULL,
	[Delivery_Point] [varchar](max) NULL,
	[SSD] [date] NULL,
	[LSD] [date] NULL,
	[CRD] [date] NULL,
	[PSD] [date] NULL,
	[Week] [varchar](max) NULL,
	[COS] [decimal](18, 2) NULL,
	[Ttl_COS] [decimal](18, 2) NULL,
	[Mode] [varchar](max) NULL,
	[Drawing_Rev] [varchar](max) NULL,
	[Remarks] [varchar](max) NULL,
	[PO_Type] [varchar](50) NULL,
	[id] [int] IDENTITY(1,1) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[eLOGMaterialMaster]    Script Date: 12/06/2025 10:11:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[eLOGMaterialMaster](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[OrderNumber] [nvarchar](255) NULL,
	[PartNumber] [nvarchar](255) NULL,
	[Description] [nvarchar](255) NULL,
	[Quantity] [float] NULL,
	[BatchNumber] [nvarchar](255) NULL,
	[Order_Status] [varchar](max) NULL,
	[Current_OP] [varchar](50) NULL,
	[Current_WC] [varchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
USE [master]
GO
ALTER DATABASE [OPS_PROD] SET  READ_WRITE 
GO
