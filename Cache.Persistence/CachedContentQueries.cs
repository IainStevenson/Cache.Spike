namespace Cache.Persistence
{
    public static class CachedContentQueries
    {
        public const string GETCACHEDCONTENT = "SELECT [Id],[Uri],[Created],[Content],[MediaType] FROM [dbo].[CachedContent] WHERE [Uri] = @resourceIdentifier";
        public const string GETALLCACHEDCONTENT = "SELECT [Id],[Uri],[Created],[Content],[MediaType] FROM [dbo].[CachedContent]";
        public const string DELETECACHEDCONTENT = "DELETE FROM [dbo].[CachedContent]  WHERE [Uri] = @resourceIdentifier";
        public const string DELETEALLCACHEDCONTENT = "DELETE FROM [dbo].[CachedContent]";
        public const string SETCACHEDCONTENT = "MERGE [dbo].[CachedContent] as target " +
            "USING (SELECT [Id] ,[Uri] ,[Created] ,[Content] ,[MediaType] " +
            "FROM [dbo].[CachedContent] WHERE [Uri] = @Uri ) as source ([Id] ,[Uri] ,[Created] ,[Content] ,[MediaType])" +
            "ON(target.[Uri] = source.[Uri])" +
            "WHEN MATCHED THEN UPDATE SET [Uri] = source..Uri ,[Created] = source.Created ,[Content] = source.Content ,[MediaType] = source.MediaType" +
            "WHEN NOT MATCHED THEN INSERT([Uri] , [Created] , [Content] , [MediaType]) " +
            "VALUES(source.Uri , source.Created , source.Content , source.MediaType);";
    }
}
