﻿using BusterWood.Data;
using NUnit.Framework;
using System.Linq;

namespace UnitTests
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void can_extend_existing_sequence()
        {
            var orig = Objects.ToDataSequence(new { Text = "hello" });
            Assert.AreEqual(1, orig.Schema.Count);
            Assert.AreEqual(typeof(string), orig.Schema["text"].Type);

            var extended = orig.Extend("Length", r => r.String("text").Length);

            Assert.AreEqual(2, extended.Schema.Count);
            Assert.AreEqual(typeof(string), extended.Schema["text"].Type);
            Assert.AreEqual(typeof(int), extended.Schema["length"].Type);
            Assert.AreEqual(1, extended.Count(), "count was wrong");

            foreach (var row in extended)
            {
                Assert.AreEqual(5, row.Int("length"));
            }
        }

        [Test]
        public void can_project_to_remove_columns()
        {
            var orig = Objects.ToDataSequence(new { Hello = "hello", World="world" });

            var result = orig.Select("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual(typeof(string), result.Schema["world"].Type);
        }

        [Test]
        public void can_create_data_sequence()
        {
            var orig = new Temp[] { new Temp { Text = "hello", Size = 1 }, new Temp { Text = "hello", Size = 2 } }.ToDataSequence();
            Assert.AreEqual(2, orig.Schema.Count);
            Assert.AreEqual(typeof(string), orig.Schema["TEXT"].Type);
            Assert.AreEqual(typeof(int), orig.Schema["SIZE"].Type);
        }

        [Test]
        public void project_removes_duplicate_rows()
        {
            var orig = new Temp[] { new Temp { Text = "hello", Size = 1 }, new Temp { Text = "hello", Size = 2 } }.ToDataSequence();
            var result = orig.Select("text");
            Assert.AreEqual(1, result.Count());
        }

        class Temp { public string Text; public int Size; };

        [Test]
        public void can_projectAway_to_remove_columns()
        {
            var orig = Objects.ToDataSequence(new { Hello = "hello", World="world" });

            var result = orig.SelectAway("world");
            Assert.AreEqual(1, result.Schema.Count);
            Assert.AreEqual("Hello", result.Schema.First().Name);
            Assert.AreEqual(typeof(string), result.Schema["hello"].Type);
        }
    }
}
