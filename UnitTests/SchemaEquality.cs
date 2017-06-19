﻿using BusterWood.Data;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class SchemaEquality
    {
        [Test]
        public void equal_when_columns_match()
        {
            var c1 = new Column("name", typeof(string));
            var s1 = new Schema("first", c1);
            var s2 = new Schema("first", c1);
            Assert.AreEqual(s1, s2);
        }

        [Test]
        public void equal_when_columns_match_regardless_of_schema_name()
        {
            var c1 = new Column("name", typeof(string));
            var s1 = new Schema("first", c1);
            var s2 = new Schema("second", c1);
            Assert.AreEqual(s1, s2);
        }

        [Test]
        public void not_equal_when_number_of_columns_differs()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("fred", typeof(string));
            var s1 = new Schema("first", c1);
            var s2 = new Schema("second", c1, c2);
            Assert.AreNotEqual(s1, s2);
        }

        [Test]
        public void not_equal_when_same_columns_but_different_order()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("fred", typeof(string));
            var s1 = new Schema("first", c1, c2);
            var s2 = new Schema("second", c2, c1);
            Assert.AreNotEqual(s1, s2);
        }

        [Test]
        public void SetEqual_when_same_columns_but_different_order()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("fred", typeof(string));
            var s1 = new Schema("first", c1, c2);
            var s2 = new Schema("second", c2, c1);
            Assert.IsTrue(Schema.SetEquals(s1, s2));
        }

        [Test]
        public void SetEqual_when_same_columns_but_different_order_symmetric()
        {
            var c1 = new Column("name", typeof(string));
            var c2 = new Column("fred", typeof(string));
            var s1 = new Schema("first", c1, c2);
            var s2 = new Schema("second", c2, c1);
            Assert.IsTrue(Schema.SetEquals(s2, s1));
        }
    }
}

