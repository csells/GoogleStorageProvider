using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StorageProvider;

namespace UnitTests {
  [TestClass]
  public class StoragePathTests {
    [TestMethod]
    public void ParseAllSlash() {
      var spath = StoragePath.Parse("bucket/folder/file.txt");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.AreEqual("folder/", spath.Folder);
      Assert.AreEqual("file.txt", spath.File);
      Assert.AreEqual("folder/file.txt", spath.StorageObjectName);
      Assert.AreEqual("gcs:/bucket/folder/file.txt", spath.FullName);
      Assert.AreEqual(StoragePathType.File, spath.Type);
    }

    [TestMethod]
    public void ParseAllBackslash() {
      var spath = StoragePath.Parse(@"bucket\folder\file.txt");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.AreEqual("folder/", spath.Folder);
      Assert.AreEqual("file.txt", spath.File);
      Assert.AreEqual("folder/file.txt", spath.StorageObjectName);
      Assert.AreEqual("gcs:/bucket/folder/file.txt", spath.FullName);
      Assert.AreEqual(StoragePathType.File, spath.Type);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentOutOfRangeException))]
    public void ParseEmptyPart() {
      StoragePath.Parse("/file.txt");
    }

    [TestMethod]
    public void ParseBucket() {
      var spath = StoragePath.Parse("bucket");
      Assert.AreEqual("bucket", spath.Bucket);
        Assert.IsNull(spath.Folder, "Folder should be null");
      Assert.IsNull(spath.File, "File should be null");
      Assert.IsNull(spath.StorageObjectName, "StorageObjectName should be null");
      Assert.AreEqual("gcs:/bucket/", spath.FullName);
      Assert.AreEqual(StoragePathType.Bucket, spath.Type);
    }

    [TestMethod]
    public void ParseBucketTrailingDelimiter() {
      var spath = StoragePath.Parse("bucket/");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.IsNull(spath.Folder, "Folder should be null");
      Assert.IsNull(spath.File, "File shoule be null");
      Assert.IsNull(spath.StorageObjectName, "StorageObjectName should be null");
      Assert.AreEqual("gcs:/bucket/", spath.FullName);
      Assert.AreEqual(StoragePathType.Bucket, spath.Type);
    }

    [TestMethod]
    public void ParseBucketAndFile() {
      var spath = StoragePath.Parse("bucket/file.txt");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.IsNull(spath.Folder, "Folder should be null");
      Assert.AreEqual("file.txt", spath.File);
      Assert.AreEqual("file.txt", spath.StorageObjectName);
      Assert.AreEqual("gcs:/bucket/file.txt", spath.FullName);
      Assert.AreEqual(StoragePathType.File, spath.Type);
    }

    [TestMethod]
    public void ParseBucketAndFolder() {
      var spath = StoragePath.Parse("bucket/folder/");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.AreEqual("folder/", spath.Folder);
      Assert.IsNull(spath.File, "File should be null");
      Assert.AreEqual("folder/", spath.StorageObjectName);
      Assert.AreEqual("gcs:/bucket/folder/", spath.FullName);
      Assert.AreEqual(StoragePathType.Folder, spath.Type);
    }

    [TestMethod]
    public void ParseBucketAndNestFolderAndFile() {
      var spath = StoragePath.Parse("bucket/folder/nested-folder/file.txt");
      Assert.AreEqual("bucket", spath.Bucket);
      Assert.AreEqual("folder/nested-folder/", spath.Folder);
      Assert.AreEqual("file.txt", spath.File);
      Assert.AreEqual("folder/nested-folder/file.txt", spath.StorageObjectName);
      Assert.AreEqual("gcs:/bucket/folder/nested-folder/file.txt", spath.FullName);
      Assert.AreEqual(StoragePathType.File, spath.Type);
    }

    [TestMethod]
    public void ParseEmpty() {
      var spath = StoragePath.Parse("");
      Assert.IsNull(spath.Bucket, "Bucket should be null");
      Assert.IsNull(spath.Folder, "Folder should be null");
      Assert.IsNull(spath.File, "File should be null");
      Assert.IsNull(spath.StorageObjectName, "StorageObjectName should be null");
      Assert.AreEqual("gcs:/", spath.FullName);
      Assert.AreEqual(StoragePathType.Empty, spath.Type);
    }

  }
}
