using System;
using System.Reflection;
using Xunit;

namespace Dapper.GraphQL.Test
{
    public class SqlQueryBuilderTests
    {
        private readonly MethodInfo parseAliasMethod;
        private readonly SqlQueryBuilder sqlQueryBuilder;

        public SqlQueryBuilderTests()
        {
            sqlQueryBuilder = new SqlQueryBuilder();
            parseAliasMethod = typeof(SqlQueryBuilder).GetMethod("ParseAlias", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Theory(DisplayName = "Alias parsing should fail")]
        [InlineData("too.many.sections.Table table")]
        [InlineData("(SELECT * FROM test.Table table) table")]
        public void AliasParsingShouldFail(string value)
        {
            var alias = parseAliasMethod.Invoke(sqlQueryBuilder, new[] { value });
            Assert.Null(alias);
        }

        [Theory(DisplayName = "Alias parsing should succeed")]
        [InlineData("test.Table table")]
        [InlineData("[test].Table table")]
        [InlineData("  [test].Table AS  table")]
        [InlineData("[test].[Table] [table]")]
        [InlineData("[test].[Table] AS [table]")]
        [InlineData("[test].[Table] table")]
        [InlineData("test.[Table] table")]
        [InlineData("database.schema.Table table")]
        [InlineData("[database].[schema].[Table] table")]
        [InlineData("[database].[schema].[Table] [table]")]
        [InlineData("   test.Table  table")]
        [InlineData("[Table] table")]
        [InlineData("Table table")]
        [InlineData("test.Table table INNER JOIN other.OtherTable otherTable ON table.Id = otherTable.Id")]
        public void AliasParsingShouldSucceed(string value)
        {
            var alias = parseAliasMethod.Invoke(sqlQueryBuilder, new[] { value });
            Assert.Equal("table", alias);
        }

        [Fact(DisplayName = "Duplicate aliases should throw an InvalidOperationException")]
        public void DuplicateAliasesShouldThrow()
        {
            var query = new SqlQueryBuilder();
            query.From("test.Table table");
            Assert.Throws<InvalidOperationException>(() => query.From("test.Table table"));
        }
    }
}