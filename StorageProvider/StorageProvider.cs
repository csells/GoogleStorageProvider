// from https://msdn.microsoft.com/en-us/library/dn727071(v=vs.85).aspx
using Google.Storage.V1;
using Google.Storage.V1.Demo;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace StorageProvider {
  [CmdletProvider("GoogleStorageProvider", ProviderCapabilities.None)]
  public class StorageProvider : ContainerCmdletProvider {
    protected override Collection<PSDriveInfo> InitializeDefaultDrives() {
      return new Collection<PSDriveInfo> { new PSDriveInfo("GCS", ProviderInfo, "", "Provider for Google Cloud Storage", null) };
    }

    protected override bool IsValidPath(string path) {
      Debug.WriteLine($"IsValidPath: path= {path}");
      throw new NotImplementedException("IsValidPath");
    }

    // dir gcs:/
    // dir gcs:\
    protected override bool ItemExists(string path) {
      Debug.WriteLine($"ItemExists: path= {path}");
      var spath = StoragePath.Parse(path);
      // TODO: check if the object exists (could be an implicit folder) and cache results
      return true;
    }

    protected override void GetItem(string path) {
      Debug.WriteLine($"GetItem: path= {path}");
      var spath = StoragePath.Parse(path);
      var client = StorageClient.Create();

      switch (spath.Type) {
        case StoragePathType.Empty:
          WriteItemObject(this, spath.FullName, true);
          break;

        case StoragePathType.Bucket:
          var bucket = client.Service.Buckets.Get(spath.Bucket).Execute();
          WriteItemObject(bucket, spath.FullName, true);
          break;

        case StoragePathType.Folder:
          // TODO: handle implicit folders
          var folder = client.GetObject(spath.Bucket, spath.StorageObjectName);
          WriteItemObject(folder, spath.FullName, true);
          break;

        case StoragePathType.File:
          var file = client.GetObject(spath.Bucket, spath.StorageObjectName);
          WriteItemObject(file, spath.FullName, false);
          break;

        default:
          throw new Exception($"Unknown StoragePathType: {spath.Type}");
      }
    }

    // ls gcs:
    // ls gcs:/
    // ls gcs:\
    // ls gcs:/bucket
    // ls gcs:/bucket/
    protected override void GetChildItems(string path, bool recurse) {
      Debug.WriteLine($"GetChildItems: path= {path}, recurse= {recurse}");

      var client = StorageClient.Create();
      var spath = StoragePath.Parse(path);

      // TODO: support for globs
      foreach (var projectId in GCloud.ListProjects().Select(p => p.projectId)) {
        switch (spath.Type) {
          // list the buckets
          case StoragePathType.Empty:
            foreach (var bucket in client.ListBuckets(projectId)) {
              WriteItemObject(bucket, StoragePath.Parse(bucket.Name).FullName, true);
            }
            break;

          case StoragePathType.Bucket:
          case StoragePathType.Folder:
            // list files
            foreach (var file in client.ListFiles(spath.Bucket, spath.Folder)) {
              WriteItemObject(file, StoragePath.Parse(spath.Bucket, file.Name).FullName, false);
            }

            // TODO: list folders (including implicit folders)
            break;

          case StoragePathType.File:
            throw new NotImplementedException("GetChildItems: StoragePathType.File -- files don't have children");

          default:
            throw new Exception($"Unknown StoragePathType: {spath.Type}");
        }
      }

    }

  }

}
