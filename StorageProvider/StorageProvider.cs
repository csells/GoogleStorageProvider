// from https://msdn.microsoft.com/en-us/library/dn727071(v=vs.85).aspx
using Google.Apis.Services;
using Google.Apis.Storage.v1;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using System.Threading.Tasks;

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
      if (path == "") { return true; }
      return base.ItemExists(path);
    }

    public StorageService CreateStorageClient() {
      var credentials = Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync().Result;

      if (credentials.IsCreateScopedRequired) {
        credentials = credentials.CreateScoped(new[] { StorageService.Scope.DevstorageFullControl });
      }

      var serviceInitializer = new BaseClientService.Initializer() {
        ApplicationName = "Google Cloud Storage PowerShell Provider",
        HttpClientInitializer = credentials
      };

      return new StorageService(serviceInitializer);
    }

    // dir gcs:/
    // dir gcs:\
    protected override void GetChildItems(string path, bool recurse) {
      Debug.WriteLine($"GetChildItems: path= {path}, recurse= {recurse}");

      // TODO: put client and projectId into properties that do the right thing
      //var client = StorageClient.FromApplicationCredentials("PowerShell-StorageProvider").Result;
      var projectId = "firm-site-126023";

      //foreach( var bucket in client.ListBuckets(projectId)) { WriteItemObject(bucket, $"gcs:/{bucket.Name}", true); }
      var client = CreateStorageClient();
      foreach( var bucket in client.Buckets.List(projectId).Execute().Items ) { WriteItemObject(bucket, $"gcs:/{bucket.Name}", true); }
    }

  }
}
