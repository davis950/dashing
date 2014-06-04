﻿namespace TopHat.Configuration {
    using System;
    using System.Data;
    using System.Linq;
    using System.Reflection;

    using TopHat.Extensions;

    /// <summary>
    ///     The default mapper.
    /// </summary>
    public class DefaultMapper : IMapper {
        /// <summary>
        ///     The _convention.
        /// </summary>
        private readonly IConvention convention;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultMapper" /> class.
        /// </summary>
        /// <param name="convention">
        ///     The convention.
        /// </param>
        public DefaultMapper(IConvention convention) {
            if (convention == null) {
                throw new ArgumentNullException("convention");
            }

            this.convention = convention;
        }

        /// <summary>
        ///     The map for.
        /// </summary>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        ///     The <see cref="Map" />.
        /// </returns>
        public Map<T> MapFor<T>() {
            var map = new Map<T>();
            this.Build(typeof(T), map);
            return map;
        }

        /// <summary>
        ///     The build.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="map">
        ///     The map.
        /// </param>
        private void Build(Type entity, IMap map) {
            map.Table = this.convention.TableFor(entity);
            map.Schema = this.convention.SchemaFor(entity);
            map.Columns = entity.GetProperties().Select(property => this.BuildColumn(map, entity, property)).ToDictionary(c => c.Name, c => c);
            this.ResolvePrimaryKey(entity, map);
            this.AssignFetchIds(map);
        }

        /// <summary>
        /// Assigns fetch ids to the non local columns using a consistent strategy (namely column name)
        /// </summary>
        /// <param name="map"></param>
        private void AssignFetchIds(IMap map) {
            int i = 0;
            var columns = map.Columns.Where(c => c.Value.Relationship != RelationshipType.None).OrderBy(c => c.Key);
            foreach (var column in columns) {
                column.Value.FetchId = ++i;
            }
        }

        /// <summary>
        ///     The build column.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="map"></param>
        /// <param name="property">
        ///     The property.
        /// </param>
        /// <param name="primaryKeyName"></param>
        /// <returns>
        ///     The <see cref="Column" />.
        /// </returns>
        private IColumn BuildColumn(IMap parentMap, Type entity, PropertyInfo property) {
            // TODO: this can be cached
            var column = (IColumn)Activator.CreateInstance(typeof(Column<>).MakeGenericType(property.PropertyType));
            column.Map = parentMap;
            column.Name = property.Name;
            column.IsIgnored = !(property.CanRead && property.CanWrite);

            this.ResolveRelationship(entity, property, column);
            this.ApplyAnnotations(entity, property, column);

            return column;
        }

        /// <summary>
        ///     The resolve relationship.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="property">
        ///     The property.
        /// </param>
        /// <param name="column">
        ///     The column.
        /// </param>
        private void ResolveRelationship(Type entity, PropertyInfo property, IColumn column) {
            if (property.PropertyType.IsEntityType()) {
                if (property.PropertyType.IsCollection()) {
                    this.ResolveOneToManyColumn(column);
                }
                else {
                    this.ResolveManyToOneColumn(column, property.Name);
                }
            }
            else {
                this.ResolveValueColumn(entity, column, property.Name, property.PropertyType);
            }
        }

        private void ResolveValueColumn(Type entity, IColumn column, string propertyName, Type propertyType) {
            column.Relationship = RelationshipType.None;
            column.DbName = propertyName;
            column.DbType = propertyType.GetDbType();

            // check particular types for defaults
            switch (column.DbType) {
                case DbType.Decimal:
                    column.Precision = this.convention.DecimalPrecisionFor(entity, propertyName);
                    column.Scale = this.convention.DecimalScaleFor(entity, propertyName);
                    break;

                case DbType.String:
                    column.Length = this.convention.StringLengthFor(entity, propertyName);
                    break;
            }
        }

        private void ResolveManyToOneColumn(IColumn column, string propertyName) {
            column.Relationship = RelationshipType.ManyToOne;
            column.DbName = propertyName + "Id";
            column.DbType = DbType.Int32;

            // TODO resolve column type of related primary key - be careful with infinite loops!
        }

        private void ResolveOneToManyColumn(IColumn column) {
            // assume to be OneToMany
            column.Relationship = RelationshipType.OneToMany;

            // what is the DbName?
            // what is the DbType?
        }

        /// <summary>
        ///     The apply annotations.
        /// </summary>
        /// <param name="entity">
        ///     The entity.
        /// </param>
        /// <param name="property">
        ///     The property.
        /// </param>
        /// <param name="column">
        ///     The column.
        /// </param>
        private void ApplyAnnotations(Type entity, PropertyInfo property, IColumn column) {
            /* should do something, innit! */
        }

        private void ResolvePrimaryKey(Type entity, IMap map) {
            map.PrimaryKey = map.Columns.Values.FirstOrDefault(c => c.Name.Equals(this.convention.PrimaryKeyFor(entity), StringComparison.OrdinalIgnoreCase));
            if (map.PrimaryKey != null) {
                map.PrimaryKey.IsPrimaryKey = true;
                map.PrimaryKey.IsAutoGenerated = this.convention.IsPrimaryKeyAutoGenerated(entity);
            }
        }
    }
}