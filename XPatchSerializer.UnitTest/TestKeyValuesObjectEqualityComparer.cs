﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XPatchLib.UnitTest
{
    [TestClass]
    public class TestKeyValuesObjectEqualityComparer
    {
        [TestMethod]
        public void TestKeyValuesObjectEqualityComparerEqualsAddForString()
        {
            string[] s1 = {"A", "B"};
            string[] s2 = {"A", "B", "D"};

            TestKeyValuesObjectEqualityComparerEqualsAdd(s1, s2);
        }

        [TestMethod]
        public void TestKeyValuesObjectEqualityComparerEqualsForChar()
        {
            char[] s1 = {'A', 'B', 'C'};
            char[] s2 = {'A', 'B', 'D'};

            TestKeyValuesObjectEqualityComparerEquals(s1, s2);
        }

        [TestMethod]
        public void TestKeyValuesObjectEqualityComparerEqualsForComplexClass()
        {
            ExampleComplexCollectionClass.OrderInfo[] s1 =
            {
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 1,
                    OrderTotal = 1.1m
                },
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 2,
                    OrderTotal = 1.2m
                },
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 3,
                    OrderTotal = 1.3m
                }
            };

            ExampleComplexCollectionClass.OrderInfo[] s2 =
            {
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 1,
                    OrderTotal = 1.1m
                },
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 2,
                    OrderTotal = 1.2m
                },
                new ExampleComplexCollectionClass.OrderInfo
                {
                    OrderId = 4,
                    OrderTotal = 1.4m
                }
            };

            TestKeyValuesObjectEqualityComparerEquals(s1, s2);
        }

        [TestMethod]
        public void TestKeyValuesObjectEqualityComparerEqualsForFloat()
        {
            float[] s1 = {1.0f, 1.1f, 1.2f};
            float[] s2 = {1.0f, 1.1f, 1.3f};

            TestKeyValuesObjectEqualityComparerEquals(s1, s2);
        }

        [TestMethod]
        public void TestKeyValuesObjectEqualityComparerEqualsForInt()
        {
            int[] s1 = {1, 2, 3};
            int[] s2 = {1, 2, 4};

            TestKeyValuesObjectEqualityComparerEquals(s1, s2);
        }

        private void TestKeyValuesObjectEqualityComparerEquals<T>(T[] s1, T[] s2)
        {
            var l1 = KeyValuesObject.Translate(s1);
            var l2 = KeyValuesObject.Translate(s2);

            var newList = l2.Except(l1, new KeyValuesObjectEqualityComparer()).ToList();
            Assert.AreEqual(newList.Count, 1);
            Assert.AreEqual(newList[0].OriValue, s2[2]);

            var removeList = l1.Except(l2, new KeyValuesObjectEqualityComparer()).ToList();
            Assert.AreEqual(removeList.Count, 1);
            Assert.AreEqual(removeList[0].OriValue, s1[2]);

            var sameList = l1.Intersect(l2, new KeyValuesObjectEqualityComparer()).ToList();
            Assert.AreEqual(sameList.Count, 2);
            Assert.AreEqual(sameList[0].OriValue, s1[0]);
            Assert.AreEqual(sameList[1].OriValue, s1[1]);
        }

        private void TestKeyValuesObjectEqualityComparerEqualsAdd<T>(T[] s1, T[] s2)
        {
            var l1 = KeyValuesObject.Translate(s1);
            var l2 = KeyValuesObject.Translate(s2);

            var newList = l2.Except(l1, new KeyValuesObjectEqualityComparer()).ToList();
            Assert.AreEqual(newList.Count, 1);
            Assert.AreEqual(newList[0].OriValue, s2[2]);
        }
    }
}