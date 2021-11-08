CREATE TABLE [dbo].[CachedContent] (
    [Id]          BIGINT             IDENTITY (1, 1) NOT NULL,
    [Uri]         NVARCHAR (4000)    NOT NULL,
    [Created]     DATETIMEOFFSET (7) NOT NULL,
    [Content]     VARBINARY (MAX)    NOT NULL,
    [MediaType]   NVARCHAR (4000)    NOT NULL,
    [MadeUp]      BIT                DEFAULT ((0)) NOT NULL,
    [MadeUpAgain] NCHAR (10)         NULL,
    CONSTRAINT [PK_CachedContent] PRIMARY KEY CLUSTERED ([Id] ASC)
);

GO
CREATE UNIQUE NONCLUSTERED INDEX [Cached-Content-UI-Uri]
    ON [dbo].[CachedContent]([Uri] ASC);

