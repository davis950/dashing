﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.SqlServer.Fixtures {
    using Dashing.Configuration;
    using Dashing.Engine.DDL;
    using Dashing.IntegrationTests.TestDomain;
    using Dashing.Tools.Migration;

    public class SqlServerFixture : IDisposable {
        public ISession Session { get; private set; }

        public string DatabaseName { get; private set; }

        private IConfiguration config;

        public SqlServerFixture() {
            config = new SqlServerConfiguration();
            this.DatabaseName = "DashingIntegration_" + Guid.NewGuid().ToString("D").Substring(0, 8);

            // load the data
            var migrator = new Migrator(
                new CreateTableWriter(config.Engine.SqlDialect),
                new DropTableWriter(config.Engine.SqlDialect),
                new AlterTableWriter(config.Engine.SqlDialect));
            IEnumerable<string> warnings, errors;
            var createStatement = migrator.GenerateSqlDiff(new List<IMap>(), config.Maps, out warnings, out errors);
            using (var transactionLessSession = config.BeginTransactionLessSession()) {
                transactionLessSession.Dapper.Execute("create database " + this.DatabaseName);
                transactionLessSession.Dapper.Execute("use " + this.DatabaseName);
                transactionLessSession.Dapper.Execute(createStatement);
            }

            this.Session = config.BeginSession();
            this.Session.Dapper.Execute("use " + this.DatabaseName);
            InsertData();
        }

        private void InsertData() {
            var users = new List<User>();
            for (var i = 0; i < 10; i++) {
                var user = new User { Username = "User_" + i };
                users.Add(user);
                this.Session.Insert(user);
            }

            var blogs = new List<Blog>();
            for (var i = 0; i < 10; i++) {
                var blog = new Blog { Title = "Blog_" + i };
                blogs.Add(blog);
                this.Session.Insert(blog);
            }

            var posts = new List<Post>();
            for (var i = 0; i < 20; i++) {
                var userId = i / 2;
                var blogId = i / 2;
                var post = new Post { Author = users[userId], Blog = blogs[blogId], Title = "Post_" + i };
                this.Session.Insert(post);
                posts.Add(post);
            }

            for (var i = 0; i < 30; i++) {
                var comment = new Comment { Post = posts[i / 2], User = users[i / 3], Content = "Comment_" + i };
                this.Session.Insert(comment);
            }

            var tags = new List<Tag>();
            for (var i = 0; i < 20; i++) {
                var tag = new Tag { Content = "Tag_" + i };
                tags.Add(tag);
                this.Session.Insert(tag);
            }

            for (var i = 0; i < 30; i++) {
                var postTag = new PostTag { Post = posts[i / 2], Tag = tags[i / 2] };
                this.Session.Insert(postTag);
            }
        }

        public void Dispose() {
            this.Session.Dispose();
            using (var transactionLessSession = config.BeginTransactionLessSession()) {
                transactionLessSession.Dapper.Execute("drop database " + this.DatabaseName);
            }
        }
    }
}
