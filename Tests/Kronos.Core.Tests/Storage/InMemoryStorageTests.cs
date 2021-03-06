﻿using System;
using System.Text;
using Kronos.Core.Storage;
using NSubstitute;
using Xunit;

namespace Kronos.Core.Tests.Storage
{
    public class InMemoryStorageTests
    {
        [Fact]
        public void CanInsertAndGetObject()
        {
            string key = "key";
            string objectWord = "lorem ipsum";
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider); storage.AddOrUpdate(key, DateTime.MaxValue, Encoding.UTF8.GetBytes(objectWord));

            byte[] objFromBytes;
            bool success = storage.TryGet(key, out objFromBytes);
            string stringFromBytes = Encoding.UTF8.GetString(objFromBytes);

            Assert.True(success);
            Assert.Equal(objectWord, stringFromBytes);
        }

        [Fact]
        public void CanUpdateExistingObject()
        {
            string key = "key";
            string first = "first";
            string second = "second";
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);

            byte[] firstObject = Encoding.UTF8.GetBytes(first);
            byte[] secondObject = Encoding.UTF8.GetBytes(second);

            storage.AddOrUpdate(key, DateTime.MaxValue, firstObject);
            storage.AddOrUpdate(key, DateTime.MaxValue, secondObject);

            byte[] objFromBytes;
            bool success = storage.TryGet(key, out objFromBytes);
            string stringFromBytes = Encoding.UTF8.GetString(objFromBytes);

            Assert.True(success);
            Assert.Equal(stringFromBytes, second);
        }

        [Fact]
        public void TryRemove_RemovesEntryFromStorage()
        {
            byte[] package = Encoding.UTF8.GetBytes("lorem ipsum");
            const string firstKey = "key1";
            const string secondKey = "key2";

            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);
            storage.AddOrUpdate(firstKey, DateTime.MaxValue, package);
            storage.AddOrUpdate(secondKey, DateTime.MaxValue, package);

            bool deleted = storage.TryRemove(firstKey);

            Assert.True(deleted);
            Assert.Equal(storage.Count, 1);
        }

        [Fact]
        public void TryRemove_DoestNotRemoveEntryFromStorageWhenKeyDoesNotExist()
        {
            byte[] package = Encoding.UTF8.GetBytes("lorem ipsum");
            const string firstKey = "key1";
            const string secondKey = "key2";

            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);
            storage.AddOrUpdate(firstKey, DateTime.MaxValue, package);

            bool deleted = storage.TryRemove(secondKey);

            Assert.False(deleted);
            Assert.Equal(storage.Count, 1);
        }

        [Fact]
        public void Contains_ReturnsTrueWhenDataExists()
        {
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);

            string key = "lorem ipsum";
            storage.AddOrUpdate(key, DateTime.MaxValue, new byte[0]);
            storage.AddOrUpdate("second", DateTime.MaxValue, new byte[0]);

            bool result = storage.Contains(key);

            Assert.True(result);
        }

        [Fact]
        public void Contains_ReturnsTrueWhenDataDoesNotExist()
        {
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);

            string key = "lorem ipsum";

            bool result = storage.Contains(key);

            Assert.False(result);
        }

        [Fact]
        public void CanClear()
        {
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);

            storage.AddOrUpdate("first", DateTime.MaxValue, new byte[0]);
            storage.AddOrUpdate("second", DateTime.MaxValue, new byte[0]);

            storage.Dispose();
            Assert.Equal(storage.Count, 0);
        }

        [Fact]
        public void ReturnsNullWhenObjectDoesNotExist()
        {
            IExpiryProvider expiryProvider = Substitute.For<IExpiryProvider>();
            IStorage storage = new InMemoryStorage(expiryProvider);

            byte[] objFromBytes;
            bool success = storage.TryGet("lorem ipsum", out objFromBytes);

            Assert.Null(objFromBytes);
            Assert.False(success);
        }
    }
}
