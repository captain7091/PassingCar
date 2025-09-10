using FluentMigrator.Runner.VersionTableInfo;

namespace PassingCarApis.SQL
{
    [VersionTableMetaData]
    public class VersionTable : IVersionTableMetaData
    {
        public string ColumnName => "Version";

        public string SchemaName => "dbo";

        public string TableName => "VersionInfo";

        public string UniqueIndexName => "UC_Version";

        public virtual string AppliedOnColumnName => "AppliedOn";

        public virtual string DescriptionColumnName => "Description";

        public object? ApplicationContext { get; set; }

        public bool OwnsSchema { get; set; }
    }
}