﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BusterWood.Data
{
    public static partial class Extensions
    {
        public static DataSequence Distinct(this DataSequence seq) => new DerivedDataSequence(seq.Schema, ((IEnumerable<Row>)seq).Distinct());

        public static DataSequence Distinct(this DataSequence seq, bool enabled) => enabled ? Distinct(seq) : seq;

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static DataSequence Restrict(this DataSequence seq, Func<Row, bool> predicate) => new DerivedDataSequence(seq.Schema, Enumerable.Where(seq, predicate));

        /// <summary>Filter the source releation, e.g. a "Where" clause</summary>
        public static DataSequence RestrictAway(this DataSequence seq, Func<Row, bool> predicate) => new DerivedDataSequence(seq.Schema, Enumerable.Where(seq, row => !predicate(row)));

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="seq"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static DataSequence Project(this DataSequence seq, IEnumerable<string> columns)
        {
            var set = new HashSet<string>(columns, Column.NameEquality);
            var toRemove = seq.Schema.Where(c => !set.Contains(c.Name));
            return ProjectAway(seq, toRemove);
        }

        /// <summary>Returns a new sequence with that only contains the requested <paramref name="columns"/> from the source <paramref name="seq"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static DataSequence Project(this DataSequence seq, params string[] columns) => Project(seq, (IEnumerable<string>) columns);

        /// <summary>Returns a new sequence with <paramref name="columns"/> removed from the source <paramref name="seq"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static DataSequence ProjectAway(this DataSequence seq, params string[] columns) => ProjectAway(seq, (IEnumerable<string>)columns);

        /// <summary>Returns a new sequence with <paramref name="columns"/> removed from the source <paramref name="seq"/></summary>
        /// <remarks>Duplicates are removed from the resulting sequence</remarks>
        public static DataSequence ProjectAway(this DataSequence seq, IEnumerable<string> columns)
        {
            var set = new HashSet<string>(columns, Column.NameEquality);
            var toRemove = seq.Schema.Where(c => set.Contains(c.Name));
            return ProjectAway(seq, toRemove);
        }

        public static DataSequence ProjectAway(this DataSequence seq, IEnumerable<Column> columns)
        {
            var copy = seq.Schema.Except(columns).ToArray();
            var newSchema = new Schema("", copy);
            var newRows = seq.Select(r => new RowWithReducedSchema(newSchema, r));
            return new DerivedDataSequence(newSchema, newRows); 
        }

        /// <summary>Adds a new calculated column to an existing <paramref name="seq"/></summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq">The base data</param>
        /// <param name="columnName">name of the new column</param>
        /// <param name="func">function to calculate the value of the new column</param>
        public static DataSequence Extend<T>(this DataSequence seq, string columnName, Func<Row, T> func)
        {
            var col = new Column(columnName, typeof(T));
            var copy = seq.Schema.Concat(Enumerable.Repeat(col, 1)).ToArray();
            var newSchema = new Schema("", copy);
            var existing = seq.Schema.columns;
            var newRows = seq.Select(r => new RowWithAddedColumn(newSchema, r, new ColumnValue(col, func(r))));
            return new DerivedDataSequence(newSchema, newRows);
        }

        public static DataSequence DifferenceAll(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, seq.Except(other));
        }

        public static DataSequence Difference(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, seq.Except(other).Distinct());
        }

        public static DataSequence IntersectAll(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, ((IEnumerable<Row>)seq).Intersect(other));
        }

        public static DataSequence Intersect(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, ((IEnumerable<Row>)seq).Intersect(other).Distinct());
        }

        public static DataSequence UnionAll(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, seq.Concat(other));
        }

        public static DataSequence Union(this DataSequence seq, DataSequence other)
        {
            if (seq.Schema != other.Schema)
                throw new ArgumentException($"Schemas '{seq.Schema}' and '{other.Schema}' are incompatible");

            return new DerivedDataSequence(seq.Schema, seq.Concat(other).Distinct());
        }

        private class RowWithAddedColumn : Row
        {
            private Row inner;
            private ColumnValue extra;

            public RowWithAddedColumn(Schema schema, Row row, ColumnValue extra) : base(schema)
            {
                this.inner = row;
                this.extra = extra;
            }

            public override object Get(string name)
            {
                if (extra.Column.NameEquals(name))
                    return extra.Value;
                return inner.Get(name);
            }

            //TODO: override other methods?
        }

        private class RowWithReducedSchema : Row
        {
            private Row inner;

            public RowWithReducedSchema(Schema schema, Row row) : base(schema)
            {
                this.inner = row;
            }

            public override object Get(string name)
            {
                Schema.ThrowWhenUnknownColumn(name);
                return inner.Get(name);
            }

            //TODO: override other methods?
        }
    }
}