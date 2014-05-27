﻿namespace TopHat.MySql {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using global::MySql.Data.MySqlClient;

    using TopHat.Engine;

    public class MySqlEngine : EngineBase {
        protected override IDbConnection NewConnection(string connectionString) {
            return new MySqlConnection(connectionString);
        }

        public override IEnumerable<T> Query<T>(IDbConnection connection, SelectQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, InsertEntityQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, UpdateEntityQuery<T> query) {
            throw new NotImplementedException();
        }

        public override int Execute<T>(IDbConnection connection, DeleteEntityQuery<T> query) {
            throw new NotImplementedException();
        }
    }
}