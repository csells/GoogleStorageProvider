using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;

namespace StorageProvider {
  public class GCloud {
    static string GcloudCmd {
      get {
        var usercmd = @"C:\Users\Chris\AppData\Local\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd";
        var allusercmd = @"C:\Program Files\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd";
        if (File.Exists(usercmd)) { return usercmd; }
        else if (File.Exists(allusercmd)) { return allusercmd; }
        else { throw new Exception("Google Cloud SDK not found"); }
      }
    }

    public static Process Exec(string args) {
      var gcloud = Process.Start(new ProcessStartInfo {
        FileName = GcloudCmd,
        Arguments = args + " --format=json",
        UseShellExecute = false,
        ErrorDialog = false,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        RedirectStandardOutput = true,
      });

      gcloud.WaitForExit();

      if (gcloud.ExitCode != 0) {
        var stderr = gcloud.StandardError.ReadToEnd();
        var errorIndex = stderr.IndexOf("ERROR: ");
        throw new Exception(stderr.Substring(errorIndex));
      }

      return gcloud;
    }

    public static JToken ExecParse(string args) {
      return JToken.ReadFrom(new JsonTextReader(Exec(args).StandardOutput));
    }

    public static T ExecDeserialize<T>(string args) {
      return JsonConvert.DeserializeObject<T>(Exec(args).StandardOutput.ReadToEnd());
    }

    public class Project {
      public DateTime createTime { get; set; }
      public string lifecycleState { get; set; }
      public string name { get; set; }
      public string projectId { get; set; }
      public long projectNumber { get; set; }
    }

    /* C:\>gcloud projects list --format=json
        [
          {
            "createTime": "2016-03-09T16:05:53.167Z",
            "lifecycleState": "ACTIVE",
            "name": "csells-foo14",
            "projectId": "csells-foo1",
            "projectNumber": "431243450493"
          },
          ...
        ]
    */
    public static Project[] ListProjects() {
      return ExecDeserialize<Project[]>("projects list");
    }

  }
}