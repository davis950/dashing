﻿namespace Dashing.Configuration {
    using System;
    using System.Data;

    public interface IColumn {
        /// <summary>
        ///     Gets the type.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     The map that this column belongs to
        /// </summary>
        IMap Map { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     Gets or sets the db type.
        /// </summary>
        DbType DbType { get; set; }

        /// <summary>
        ///     Gets or sets the database field name.
        /// </summary>
        string DbName { get; set; }

        /// <summary>
        ///     Gets or sets the precision.
        /// </summary>
        byte Precision { get; set; }

        /// <summary>
        ///     Gets or sets the scale.
        /// </summary>
        byte Scale { get; set; }

        /// <summary>
        /// Indicates whether the column should have it's length set to Max
        /// </summary>
        bool MaxLength { get; set; }

        /// <summary>
        ///     Gets or sets the length.
        /// </summary>
        ushort Length { get; set; }

        /// <summary>
        /// Gets or sets the default value for this column
        /// </summary>
        string Default { get; set; }

        /// <summary>
        ///     Gets or sets whether the column is nullable
        /// </summary>
        bool IsNullable { get; set; }

        /// <summary>
        ///     Gets or sets whether the column is the primary key
        /// </summary>
        bool IsPrimaryKey { get; set; }

        /// <summary>
        ///     Gets or sets whether the column is auto generated
        /// </summary>
        bool IsAutoGenerated { get; set; }

        /// <summary>
        ///     Indicates whether the column will be ignored for all queries and schema generation
        /// </summary>
        bool IsIgnored { get; set; }

        /// <summary>
        ///     Indicates whether the column will be excluded from select queries unless specifically requested
        /// </summary>
        bool IsExcludedByDefault { get; set; }

        /// <summary>
        ///     Use for indexing in to Query multimapping queries
        /// </summary>
        /// <remarks>Must be consistent across app restarts as CodeGeneration is only updated if the assemblies change</remarks>
        int FetchId { get; set; }

        /// <summary>
        ///     Gets or sets the relationship.
        /// </summary>
        RelationshipType Relationship { get; set; }

        /// <summary>
        ///     References the property on the child class that this property refers to
        /// </summary>
        /// <remarks>Only applies to OneToMany Relationship</remarks>
        IColumn ChildColumn { get; set; }

        /// <summary>
        /// References the parent map in a ManyToOne relationship
        /// </summary>
        IMap ParentMap { get; }

        /// <summary>
        /// References the opposite column in a one-to-one relationship
        /// </summary>
        IColumn OppositeColumn { get; }
    }
}