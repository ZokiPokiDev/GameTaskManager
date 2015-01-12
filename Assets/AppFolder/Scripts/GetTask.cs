using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class GetTask : MonoBehaviour {

    GoogleDrive drive;
    bool uploadTextInProgress;
    bool initInProgress;

	// Use this for initialization
	IEnumerator Start () {
        initInProgress = true;

        drive = new GoogleDrive();
        drive.ClientID = "897584417662-rnkgkl5tlpnsau7c4oc0g2jp08cpluom.apps.googleusercontent.com";
        drive.ClientSecret = "tGNLbYnrdRO2hdFmwJAo5Fbt";

        var authorization = drive.Authorize();
        yield return StartCoroutine(authorization);

        if (authorization.Current is Exception) {
            Debug.LogWarning(authorization.Current as Exception);
            goto finish;
        }
        else
            Debug.Log("User Account: " + drive.UserAccount);

        Debug.Log("User ClientID: " + drive.ClientID);
        Debug.Log("User ClientSecret: " + drive.ClientSecret);

        // Get all files in AppData folder and view text file.
        {
            var listFiles = drive.ListFiles(drive.AppData);
            yield return StartCoroutine(listFiles);
            var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(listFiles);

            if (files != null) {
                foreach (var file in files) {
                    Debug.Log(file);

                    if (file.Title.EndsWith(".txt")) {
                        var download = drive.DownloadFile(file);
                        yield return StartCoroutine(download);

                        var data = GoogleDrive.GetResult<byte[]>(download);
                        Debug.Log(System.Text.Encoding.UTF8.GetString(data));
                    }
                }
            }
            else {
                Debug.LogError(listFiles.Current);
            }
        }

    finish:
        initInProgress = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
	}

    public void UploadTextToDrive(){
        StartCoroutine("UploadText");
    }

    /// <summary>
    /// <para>Update 'my_text.txt' in the root folder.</para>
    /// <para>The file has json data.</para>
    /// </summary>
    IEnumerator UploadText() {
        if (drive == null || !drive.IsAuthorized || uploadTextInProgress)
            yield break;

        uploadTextInProgress = true;

        // Get 'my_text.txt'.
        var list = drive.ListFilesByQueary("title = 'my_text.txt'");
        yield return StartCoroutine(list);

        GoogleDrive.File file;
        Dictionary<string, object> data;

        var files = GoogleDrive.GetResult<List<GoogleDrive.File>>(list);

        if (files == null || files.Count > 0) {
            // Found!
            file = files[0];

            // Download file data.
            var download = drive.DownloadFile(file);
            yield return StartCoroutine(download);

            var bytes = GoogleDrive.GetResult<byte[]>(download);
            try {
                // Data is json format.
                var reader = new JsonFx.Json.JsonReader(Encoding.UTF8.GetString(bytes));
                data = reader.Deserialize<Dictionary<string, object>>();
            }
            catch (Exception e) {
                Debug.LogWarning(e);

                data = new Dictionary<string, object>();
            }
        }
        else {
            // Make a new file.
            file = new GoogleDrive.File(new Dictionary<string, object>
			{
				{ "title", "test_text.txt" },
				{ "mimeType", "text/plain" },
				{ "description", "test" }
			});
            data = new Dictionary<string, object>();
        }

        // Update file data.
        data["date"] = DateTime.Now.ToString();
        if (data.ContainsKey("count"))
            data["count"] = (int)data["count"] + 1;
        else
            data["count"] = 0;

        data["game 1"] = "zoki";

        // And uploading...
        {
            var bytes = Encoding.UTF8.GetBytes(JsonFx.Json.JsonWriter.Serialize(data));

            var upload = drive.UploadFile(file, bytes);
            yield return StartCoroutine(upload);

            if (!(upload.Current is Exception)) {
                Debug.Log("Upload complete!");
            }
        }

        uploadTextInProgress = false;
    }
}
