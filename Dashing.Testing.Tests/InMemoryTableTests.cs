﻿namespace Dashing.Testing.Tests {
    using System;
    using System.Linq;

    using Dashing.Engine.InMemory;
    using Dashing.Testing.Tests.TestDomain;

    using Xunit;

    public class InMemoryTableTests {
        [Fact]
        public void InsertReturnsPrimaryKey() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post = new Post() { Title = "Foo" };
            table.Insert(post);
            Assert.Equal(1, post.PostId);
        }

        [Fact]
        public void InsertReturns1() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post = new Post() { Title = "Foo" };
           Assert.Equal(1, table.Insert(post));
        }

        [Fact]
        public void InsertNonAutoGeneratedWorks() {
            var table = new InMemoryTable<ThingWithStringPrimaryKey, string>(new TestConfiguration());
            var thing = new ThingWithStringPrimaryKey() { Id = "Foo", Name = "Bar" };
            Assert.Equal(1, table.Insert(thing));
        }

        [Fact]
        public void InsertWithSameKeyThrows() {
            var table = new InMemoryTable<ThingWithStringPrimaryKey, string>(new TestConfiguration());
            var thing = new ThingWithStringPrimaryKey() { Id = "Foo", Name = "Bar" };
            var dupeThing = new ThingWithStringPrimaryKey() { Id = "Foo", Name = "Car" };
            table.Insert(thing);
            Assert.Throws<Exception>(() => table.Insert(dupeThing));
        }

        [Fact]
        public void InsertWithLongWorks() {
            var table = new InMemoryTable<ThingWithLongPrimaryKey, long>(new TestConfiguration());
            var thing = new ThingWithLongPrimaryKey { Name = "Foo" };
            var pk = table.Insert(thing);
            Assert.Equal(1, thing.Id);

            var secondThing = new ThingWithLongPrimaryKey { Name = "Bar" };
            var pk2 = table.Insert(secondThing);
            Assert.Equal(2, secondThing.Id);
        }

        [Fact]
        public void GetReturnsNull() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            Assert.Null(table.Get(1));
        }

        [Fact]
        public void GetReturnsClone() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post = new Post { Title = "Foo" };
            table.Insert(post);
            var returnedPost = table.Get(1);
            Assert.NotNull(returnedPost);
            Assert.Equal("Foo", returnedPost.Title);
            Assert.False(post == returnedPost);
            Assert.True(post.Equals(returnedPost));
        }

        [Fact]
        public void QueryWorks() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post1 = new Post() { Title = "Foo" };
            var post2 = new Post() { Title = "Bar" };
            table.Insert(post1);
            table.Insert(post2);
            var posts = table.Query().ToArray();
            Assert.Equal(2, posts.Length);
            Assert.True(post1.Equals(posts[0]));
            Assert.True(post2.Equals(posts[1]));
        }

        [Fact]
        public void UpdateReturns0ForNonExistant() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post1 = new Post() { Title = "Foo" };
            Assert.Equal(0, table.Update(post1));
        }

        [Fact]
        public void UpdateWorks() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post1 = new Post() { Title = "Foo" };
            table.Insert(post1);
            post1.Title = "Bar";
            table.Update(post1);
            var updatedPost = table.Get(1);
            Assert.False(post1 == updatedPost);
            Assert.True(post1.Equals(updatedPost));
            Assert.Equal("Bar", updatedPost.Title);
        }

        [Fact]
        public void DeleteReturns0ForNonExistant() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post1 = new Post() { Title = "Foo" };
            Assert.Equal(0, table.Delete(post1));
        }

        [Fact]
        public void DeleteWorks() {
            var table = new InMemoryTable<Post, int>(new TestConfiguration());
            var post1 = new Post() { Title = "Foo" };
            table.Insert(post1);
            table.Delete(post1);
            Assert.Empty(table.Query());
        }
    }
}
