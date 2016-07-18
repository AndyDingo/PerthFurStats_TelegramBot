-- Script Date: 25/06/2016 11:07 PM  - ErikEJ.SqlCeScripting version 3.5.2.58
-- Database information:
-- Database: F:\Users\AndyDingo\Source\Repos\PerthFurStats_TelegramBot\pfsTelegramBot\data\pfs_tgbot_data.db
-- ServerVersion: 3.9.2
-- DatabaseSize: 4 KB
-- Created: 25/06/2016 7:23 PM

-- User Table information:
-- Number of tables: 2
-- tbl_cmduse_global: -1 row(s)
-- tbl_urls: -1 row(s)

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [tbl_urls] (
  [id] bigint NOT NULL
, [count] bigint NULL
, [date] date NULL
, [time] time NULL
, [firstname] nvarchar(50) NULL
, [username] nvarchar(50) NULL
, [url] text NULL
, CONSTRAINT [sqlite_master_PK_tbl_urls] PRIMARY KEY ([id])
);
CREATE TABLE [tbl_cmduse_global] (
  [id] bigint NOT NULL
, [command] text NULL
, [count] bigint NULL
, CONSTRAINT [sqlite_master_PK_tbl_cmduse_global] PRIMARY KEY ([id])
);
COMMIT;

