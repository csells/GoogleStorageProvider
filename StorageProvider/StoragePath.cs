using Google.Storage.V1.Demo;
using System;
using System.Diagnostics;

namespace StorageProvider {
  public enum StoragePathType { Empty, Bucket, Folder, File };

  public class StoragePath {
    public StoragePath(string bucket = null, string folder = null, string file = null) {
      Debug.Assert(Delimiter.Length == 1);
      Debug.Assert(Delimiter2.Length == 1);

      if (string.IsNullOrWhiteSpace(bucket) && !(string.IsNullOrEmpty(folder) && string.IsNullOrEmpty(file))) { throw new ArgumentOutOfRangeException("bucket", "bucket cannot be empty if file and folder aren't empty"); }
      if (!string.IsNullOrEmpty(folder) && folder[folder.Length - 1] != Delimiter[0]) { throw new ArgumentOutOfRangeException("folder", $"folder must end in {Delimiter}"); }
      if (bucket != null && (bucket.Contains(Delimiter) || bucket.Contains(Delimiter2))) { throw new ArgumentOutOfRangeException("bucket", $"bucket may not contain {Delimiter} or {Delimiter2}"); }
      if (file != null && (file.Contains(Delimiter) || bucket.Contains(Delimiter2))) { throw new ArgumentOutOfRangeException("file", $"file may not contain {Delimiter} or {Delimiter2}"); }
      if (folder != null && (folder.StartsWith(Delimiter) || folder.StartsWith(Delimiter2))) { throw new ArgumentOutOfRangeException("folder", $"folder may not start with {Delimiter} or {Delimiter2}"); }

      Bucket = bucket;
      Folder = Canonical(folder);
      File = file;
    }

    /// <summary>
    /// Parse path string into it's parts given that PowerShell strips the leading delimiter
    /// </summary>
    /// <param name="path">e.g. my-bucket/my/folder/path/foo.txt or foo\bar.txt</param>
    /// <returns>StoragePath object with the parts parsed properly</returns>
    public static StoragePath Parse(string path) {
      if (string.IsNullOrWhiteSpace(path)) { return new StoragePath(); }
      path = Canonical(path);
      string[] parts = path.Split(Delimiter[0]);

      string bucket = parts[0];
      string folder = null;
      string file = null;

      if (bucket.Length == 0) { throw new ArgumentOutOfRangeException("path", "bucket cannot be empty, e.g. /foo.txt"); }

      // e.g. bucket/
      if (parts.Length == 2 && parts[1] == "") { /* trailing delimiter on bucket -- that's ok */ }

      // e.g. bucket/folder/
      else if (path.EndsWith(Delimiter)) { folder = string.Join(Delimiter, parts, 1, parts.Length - 2) + Delimiter; }

      // e.g. bucket/file.txt or bucket/folder/file.txt or bucket/folder/nested-folder/file.txt
      else if (parts.Length > 1) {
        if (parts.Length > 2) { folder = string.Join(Delimiter, parts, 1, parts.Length - 2) + Delimiter; }
        file = parts[parts.Length - 1];
      }

      return new StoragePath(bucket, folder, file);
    }

    public static StoragePath Parse(string bucket, string storageObjectName) =>
      Parse(string.IsNullOrWhiteSpace(bucket) ? null : bucket + Delimiter + storageObjectName);

    static string Canonical(string path) => path == null ? null : path.Replace(Delimiter2, Delimiter);
    static string Delimiter = BucketHelper.Delimiter.ToString();
    static string Delimiter2 = @"\";

    public string Bucket { get; private set; }
    public string Folder { get; private set; }
    public string File { get; private set; }

    public StoragePathType Type {
      get {
        if (string.IsNullOrEmpty(Bucket)) { return StoragePathType.Empty; }
        if (string.IsNullOrEmpty(Folder) && string.IsNullOrEmpty(File)) { return StoragePathType.Bucket; }
        if (string.IsNullOrEmpty(File)) { Debug.Assert(!string.IsNullOrEmpty(Folder)); return StoragePathType.Folder; }
        Debug.Assert(!string.IsNullOrEmpty(File));
        return StoragePathType.File;
      }
    }

    // e.g. foo/bar.txt or foo.txt (no leading '/' at the root)
    public string StorageObjectName => Folder == null && File == null ? null : (Folder ?? "") + (File ?? "");

    // e.g. gcs:/bucket/folder/nested-folder/file.txt
    public string FullName =>
         Type == StoragePathType.Empty ? $"gcs:{Delimiter}" : $"gcs:{Delimiter}{Bucket}{Delimiter}{StorageObjectName}";
  }

}
