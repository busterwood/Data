﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BusterWood.Data
{
    public static class ClassExtensions
    {
        public static DataSequence ToDataSequence<T>(this IEnumerable<T> items, string name = null)
        {
            var schema = ToSchema<T>(name);
            return new ObjectSequence<T>(schema, items);
        }

        public static DataSequence ToDataSequence<T>(this T item, string name = null)
        {
            var schema = ToSchema<T>(name);
            return new SingleObjectSequence<T>(schema, item);
        }

        private static Schema ToSchema<T>(string name = null) => ToSchema(typeof(T), name);

        public static Schema ToSchema(this Type type, string name = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var cols = ReadableMembers(type).Select(m => new Column(m.Name, MemberType(m)));
            return new Schema(name ?? type.Name, cols);
        }

        static MemberInfo[] ReadableMembers(Type type) =>
            type.GetProperties().Where(p => p.CanRead)
            .Concat<MemberInfo>(type.GetFields())
            .ToArray();

        static Type MemberType(MemberInfo m) => m is PropertyInfo ? ((PropertyInfo)m).PropertyType : ((FieldInfo)m).FieldType;

        class ObjectSequence<T> : DataSequence
        {
            readonly IEnumerable<T> items;
            readonly MemberInfo[] members;

            public ObjectSequence(Schema schema, IEnumerable<T> items) : base(schema)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));
                this.items = items;
                members = ReadableMembers(typeof(T));
            }

            public override IEnumerator<Row> GetEnumerator()
            {
                foreach (var item in items)
                    yield return new ReflectionRow(Schema, members, item);
            }
        }

        class SingleObjectSequence<T> : DataSequence
        {
            readonly T item;
            readonly MemberInfo[] members;

            public SingleObjectSequence(Schema schema, T item) : base(schema)
            {
                if (item == null) throw new ArgumentNullException(nameof(item));
                this.item = item;
                members = ReadableMembers(typeof(T));
            }

            public override IEnumerator<Row> GetEnumerator()
            {
                if ((typeof(T).IsClass || IsNullableValueType()) && item == null)
                    yield return new ReflectionRow(Schema, members, item);
            }

            private static bool IsNullableValueType() => typeof(T).IsValueType && typeof(T).IsConstructedGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        class ReflectionRow : Row
        {
            readonly MemberInfo[] members;
            readonly object item;

            public ReflectionRow(Schema schema, MemberInfo[] members, object item) : base(schema)
            {
                this.members = members;
                this.item = item;
            }

            public override object Get(int index) => GetValue(members[index], item);

            static object GetValue(MemberInfo m, object item) => m is PropertyInfo ? ((PropertyInfo)m).GetValue(item) : ((FieldInfo)m).GetValue(item);
        }

    }
}
