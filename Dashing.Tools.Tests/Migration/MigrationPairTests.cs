﻿namespace Dashing.Tools.Tests.Migration {
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;
    using Dashing.Tools.Migration;
    using Dashing.Tools.Tests.TestDomain;

    using Moq;

    using Xunit;

    public class MigrationPairTests {
        [Fact]
        public void MatchingMapsDontRequireMigration() {
            var from = this.GenerateMap();
            var to = this.GenerateMap();

            var target = MigrationPair.Of(from, to);

            string dontcare;
            Assert.False(target.RequiresUpdate(out dontcare));
        }

        [Fact]
        public void DifferentDbNameRequiresMigration() {
            this.AssertRequiresUpdate(to => to.Property(p => p.PostId).DbName = "foo");
        }

        [Fact]
        public void DifferentDbTypeRequiresMigration() {
            this.AssertRequiresUpdate(to => to.Property(p => p.PostId).DbType = DbType.Guid);
        }

        [Fact]
        public void DifferentPrimaryKeyRequiresMigration() {
            this.AssertRequiresUpdate(to => to.Property(p => p.PostId).IsPrimaryKey = false);
        }

        [Fact]
        public void DifferentAutoGenerationRequiresMigration() {
            this.AssertRequiresUpdate(to => to.Property(p => p.PostId).IsAutoGenerated = false);
        }

        [Fact]
        public void DifferentNullableRequiresMigration() {
            this.AssertRequiresUpdate(to => to.Property(p => p.PostId).IsNullable = true);
        }

        private void AssertRequiresUpdate(Action<IMap<Post>> action) {
            var from = this.GenerateMap();
            var to = this.GenerateMap();
            action(to);

            var target = MigrationPair.Of(@from, to);

            string dontcare;
            Assert.True(target.RequiresUpdate(out dontcare));
        }

        private IMap<Post> GenerateMap() {
            var dialect = new SqlServerDialect();
            var config = new Mock<IConfiguration>(MockBehavior.Strict);

            var map = new Map<Post> { Table = "Posts", Schema = null, Configuration = config.Object };

            map.Columns.Add(
                "PostId",
                new Column<int> {
                                    Name = "PostId",
                                    DbName = "PostId",
                                    DbType = DbType.Int32,
                                    Map = map,
                                    IsPrimaryKey = true,
                                    IsAutoGenerated = true,
                                    IsNullable = false
                                });

            map.PrimaryKey = map.Columns.First().Value;
            return map;
        }
    }
}