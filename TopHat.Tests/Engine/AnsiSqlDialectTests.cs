﻿namespace TopHat.Tests.Engine {
    using System.Data;
    using System.Text;

    using TopHat.Configuration;
    using TopHat.Engine;

    using Xunit;

    public class AnsiSqlDialectBaseTests {
        [Fact]
        public void BinaryColumnHasBitType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.Binary });
            Assert.Equal("\"foo\" bit(0) not null", actual);
        }

        [Fact]
        public void BooleanColumnHasSmallintUnsignedType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.Boolean });
            Assert.Equal("\"foo\" smallint unsigned not null", actual);
        }

        [Fact]
        public void DateTimeColumnHasTimestampType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.DateTime });
            Assert.Equal("\"foo\" timestamp not null", actual);
        }

        [Fact]
        public void DateTime2ColumnHasTimestampType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.DateTime2 });
            Assert.Equal("\"foo\" timestamp not null", actual);
        }

        [Fact]
        public void DateTimeOffsetColumnHasTimestampTzType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.DateTimeOffset });
            Assert.Equal("\"foo\" timestamptz not null", actual);
        }

        [Fact]
        public void DoubleColumnHasDoublePrecisionType() {
            var actual = this.GetColumnSpec(new Column<int> { DbName = "foo", DbType = DbType.Double });
            Assert.Equal("\"foo\" double precision not null", actual);
        }

        private string GetColumnSpec(IColumn col) {
            var sb = new StringBuilder();
            this.MakeTarget().AppendColumnSpecification(sb, col);
            return sb.ToString();
        }

        private AnsiSqlDialect MakeTarget() {
            return new AnsiSqlDialect();
        }
    }
}