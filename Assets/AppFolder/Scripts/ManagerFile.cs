using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManagerFile : MonoBehaviour {

    GoogleDrive drive;

    bool initInProgress;
    bool uploadTextInProgress;

    public GameObject teamButtons, userButtons, userProfileText, connectingObject,
        uploadFail, uploading, refreshButton, refreshButtonText, accessDenied, refreshingText;

    // Use this for initialization
    IEnumerator Start() {
        initInProgress = true;
        print("start progress");

        // HIDE OBJECTS ---------------------------------------------------------
        userProfileText.GetComponent<Text>().enabled = false;
        uploadFail.GetComponent<Text>().enabled = false;
        uploading.GetComponent<Text>().enabled = false;
        accessDenied.GetComponent<Text>().enabled = false;
        refreshingText.GetComponent<Text>().enabled = false;
        refreshButton.SetActive(false);
        teamButtons.SetActive(false);
        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(false);
        }
        //------------------------------------------------------------------------

        drive = new GoogleDrive();
        drive.ClientID = "897584417662-rnkgkl5tlpnsau7c4oc0g2jp08cpluom.apps.googleusercontent.com";
        drive.ClientSecret = "tGNLbYnrdRO2hdFmwJAo5Fbt";

        if (drive.UserAccount != "game.mk.host@gmail.com") {
            drive.UserAccount = "game.mk.host@gmail.com";

            print("deautorize");
            var unauthorization = drive.Unauthorize();
            yield return StartCoroutine(unauthorization);

            print("finish deautorized");
        }

        print("start autorized");
        var authorization = drive.Authorize();
        yield return StartCoroutine(authorization);

        print("finish autorized");
        if (authorization.Current is Exception) {
            Debug.LogWarning(authorization.Current as Exception);
            goto finish;
        }
        else
            Debug.Log("User Account: " + drive.UserAccount);

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

        if (drive == null || !drive.IsAuthorized || uploadTextInProgress)
            yield break;

        uploadTextInProgress = true;

        // Get 'my_text.txt'.
        var list = drive.ListFilesByQueary("title = 'my_text.txt'");
        yield return StartCoroutine(list);

        GoogleDrive.File downloadedFile;
        Dictionary<string, object> downloadedData;

        var downloadedFiles = GoogleDrive.GetResult<List<GoogleDrive.File>>(list);

        if (downloadedFiles == null || downloadedFiles.Count > 0) {
            print("Found one file");
            // Found!
            downloadedFile = downloadedFiles[0];

            // Download file data.
            var download = drive.DownloadFile(downloadedFile);
            yield return StartCoroutine(download);

            var bytes = GoogleDrive.GetResult<byte[]>(download);
            try {
                // Data is json format.
                var reader = new JsonFx.Json.JsonReader(Encoding.UTF8.GetString(bytes));
                downloadedData = reader.Deserialize<Dictionary<string, object>>();
            }
            catch (Exception e) {
                Debug.LogWarning(e);

                downloadedData = new Dictionary<string, object>();
            }
        }
        else {
            print("Make a new file");
            // Make a new file.
            downloadedFile = new GoogleDrive.File(new Dictionary<string, object>
			{
				{ "title", "my_text.txt" },
				{ "mimeType", "text/plain" },
				{ "description", "test" }
			});
            downloadedData = new Dictionary<string, object>();
        }

        // Update file data.
        downloadedData["date"] = DateTime.Now.ToString();
        if (downloadedData.ContainsKey("count"))
            downloadedData["count"] = (int)downloadedData["count"] + 1;
        else
            downloadedData["count"] = 0;        
        
        // SHOW OBJECTS ---------------------------------------------------------
        connectingObject.GetComponent<Text>().enabled = false;
        userProfileText.GetComponent<Text>().enabled = true;
        userProfileText.GetComponent<Text>().text = "profile mail: " + drive.UserAccount;
        refreshButton.SetActive(true);
        teamButtons.SetActive(true);
        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(true);
        }

        UpdateUserButtons(ref downloadedData);
        uploadTextInProgress = false;

        print("stop progress");
        //-----------------------------------------------------------------------
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKey(KeyCode.Escape))
            Application.Quit();
    }

    void UpdateUserButtons(ref Dictionary<string, object> dData) {
        if (dData.ContainsKey("game 1")) {
            teamButtons.transform.GetChild(1).GetChild(0).GetComponentInChildren<Text>().text = dData["game 1"] as string;
            if(teamButtons.transform.GetChild(1).GetChild(0).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 1"] = "none";
            teamButtons.transform.GetChild(1).GetChild(0).GetComponentInChildren<Text>().text = dData["game 1"] as string;
            teamButtons.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 2")) {
            teamButtons.transform.GetChild(1).GetChild(1).GetComponentInChildren<Text>().text = dData["game 2"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(1).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(1).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(1).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 2"] = "none";
            teamButtons.transform.GetChild(1).GetChild(1).GetComponentInChildren<Text>().text = dData["game 2"] as string;
            teamButtons.transform.GetChild(1).GetChild(1).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 3")) {
            teamButtons.transform.GetChild(1).GetChild(2).GetComponentInChildren<Text>().text = dData["game 3"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(2).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(2).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(2).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 3"] = "none";
            teamButtons.transform.GetChild(1).GetChild(2).GetComponentInChildren<Text>().text = dData["game 3"] as string;
            teamButtons.transform.GetChild(1).GetChild(2).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 4")) {
            teamButtons.transform.GetChild(1).GetChild(3).GetComponentInChildren<Text>().text = dData["game 4"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(3).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(3).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(3).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 4"] = "none";
            teamButtons.transform.GetChild(1).GetChild(3).GetComponentInChildren<Text>().text = dData["game 4"] as string;
            teamButtons.transform.GetChild(1).GetChild(3).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 5")) {
            teamButtons.transform.GetChild(1).GetChild(4).GetComponentInChildren<Text>().text = dData["game 5"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(4).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(4).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(4).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 5"] = "none";
            teamButtons.transform.GetChild(1).GetChild(4).GetComponentInChildren<Text>().text = dData["game 5"] as string;
            teamButtons.transform.GetChild(1).GetChild(4).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 6")) {
            teamButtons.transform.GetChild(1).GetChild(5).GetComponentInChildren<Text>().text = dData["game 6"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(5).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(5).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(5).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 6"] = "none";
            teamButtons.transform.GetChild(1).GetChild(5).GetComponentInChildren<Text>().text = dData["game 6"] as string;
            teamButtons.transform.GetChild(1).GetChild(5).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 7")) {
            teamButtons.transform.GetChild(1).GetChild(6).GetComponentInChildren<Text>().text = dData["game 7"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(6).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(6).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(6).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 7"] = "none";
            teamButtons.transform.GetChild(1).GetChild(6).GetComponentInChildren<Text>().text = dData["game 7"] as string;
            teamButtons.transform.GetChild(1).GetChild(6).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 8")) {
            teamButtons.transform.GetChild(1).GetChild(7).GetComponentInChildren<Text>().text = dData["game 8"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(7).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(7).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(7).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 8"] = "none";
            teamButtons.transform.GetChild(1).GetChild(7).GetComponentInChildren<Text>().text = dData["game 8"] as string;
            teamButtons.transform.GetChild(1).GetChild(7).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 9")) {
            teamButtons.transform.GetChild(1).GetChild(8).GetComponentInChildren<Text>().text = dData["game 9"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(8).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(8).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(8).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 9"] = "none";
            teamButtons.transform.GetChild(1).GetChild(8).GetComponentInChildren<Text>().text = dData["game 9"] as string;
            teamButtons.transform.GetChild(1).GetChild(8).GetComponent<Image>().color = Color.green;
        }
        if (dData.ContainsKey("game 10")) {
            teamButtons.transform.GetChild(1).GetChild(9).GetComponentInChildren<Text>().text = dData["game 10"] as string;
            if (teamButtons.transform.GetChild(1).GetChild(9).GetComponentInChildren<Text>().text == "none")
                teamButtons.transform.GetChild(1).GetChild(9).GetComponent<Image>().color = Color.green;
            else
                teamButtons.transform.GetChild(1).GetChild(9).GetComponent<Image>().color = Color.red;
        }
        else {
            dData["game 10"] = "none";
            teamButtons.transform.GetChild(1).GetChild(9).GetComponentInChildren<Text>().text = dData["game 10"] as string;
            teamButtons.transform.GetChild(1).GetChild(9).GetComponent<Image>().color = Color.green;
        }
    }

    void SetDataFile(ref Dictionary<string, object> dData) {
        dData["game 1"] = teamButtons.transform.GetChild(1).GetChild(0).GetComponentInChildren<Text>().text;
        dData["game 2"] = teamButtons.transform.GetChild(1).GetChild(1).GetComponentInChildren<Text>().text;
        dData["game 3"] = teamButtons.transform.GetChild(1).GetChild(2).GetComponentInChildren<Text>().text;
        dData["game 4"] = teamButtons.transform.GetChild(1).GetChild(3).GetComponentInChildren<Text>().text;
        dData["game 5"] = teamButtons.transform.GetChild(1).GetChild(4).GetComponentInChildren<Text>().text;
        dData["game 6"] = teamButtons.transform.GetChild(1).GetChild(5).GetComponentInChildren<Text>().text;
        dData["game 7"] = teamButtons.transform.GetChild(1).GetChild(6).GetComponentInChildren<Text>().text;
        dData["game 8"] = teamButtons.transform.GetChild(1).GetChild(7).GetComponentInChildren<Text>().text;
        dData["game 9"] = teamButtons.transform.GetChild(1).GetChild(8).GetComponentInChildren<Text>().text;
        dData["game 10"] = teamButtons.transform.GetChild(1).GetChild(9).GetComponentInChildren<Text>().text;
    }    

    public void UploadTextToDrive() {
        StartCoroutine("UploadText");
    }

    public void MakeRefresh() {
        StartCoroutine("Refresh");
    }

    /// <summary>
    /// <para>Update 'my_text.txt' in the root folder.</para>
    /// <para>The file has json data.</para>
    /// </summary>
    IEnumerator UploadText() {
        if (drive == null || !drive.IsAuthorized || uploadTextInProgress) {
            uploadFail.GetComponent<Text>().enabled = true;
            StartCoroutine("FailUpload", 5f);

            // HIDE OBJECTS ---------------------------------------------------------
            userProfileText.GetComponent<Text>().enabled = false;
            refreshButton.GetComponent<Button>().enabled = false;
            refreshButton.GetComponent<Image>().enabled = false;
            refreshButton.GetComponentInChildren<Text>().enabled = false;
            teamButtons.SetActive(false);

            foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
                child.GetComponent<Button>().enabled = false;
                child.GetComponent<Image>().enabled = false;
                child.GetComponentInChildren<Text>().enabled = false;
            }
            foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
                child.GetComponent<Button>().enabled = false;
                child.GetComponent<Image>().enabled = false;
                child.GetComponentInChildren<Text>().enabled = false;
            }
            foreach (Transform child in userButtons.transform) {
                child.gameObject.SetActive(false);
            }
            // ----------------------------------------------------------------------

            yield break;
        }

        uploadTextInProgress = true;

        // HIDE OBJECTS ---------------------------------------------------------
        userProfileText.GetComponent<Text>().enabled = false;
        refreshButton.GetComponent<Button>().enabled = false;
        refreshButton.GetComponent<Image>().enabled = false;
        refreshButton.GetComponentInChildren<Text>().enabled = false;

        foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = false;
            child.GetComponent<Image>().enabled = false;
            child.GetComponentInChildren<Text>().enabled = false;
        }
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = false;
            child.GetComponent<Image>().enabled = false;
            child.GetComponentInChildren<Text>().enabled = false;
        }

        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(false);
        }
        // ----------------------------------------------------------------------

        uploading.GetComponent<Text>().enabled = true;

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
        
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponentInChildren<Text>().enabled = true;
        }
        SetDataFile(ref data);
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponentInChildren<Text>().enabled = false;
        }

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
        uploading.GetComponent<Text>().enabled = false;

        // SHOW OBJECTS ---------------------------------------------------------
        connectingObject.GetComponent<Text>().enabled = false;
        userProfileText.GetComponent<Text>().enabled = true;
        userProfileText.GetComponent<Text>().text = "profile mail: " + drive.UserAccount;
        refreshButton.GetComponent<Button>().enabled = true;
        refreshButton.GetComponent<Image>().enabled = true;
        print("refreshButtonText = " + refreshButtonText);
        print("refreshButtonText component = " + refreshButtonText.GetComponentInChildren<Text>());
        refreshButtonText.GetComponent<Text>().enabled = true;

        foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }

        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(true);
        }
        print("stop progress");
        //-----------------------------------------------------------------------

    }

    IEnumerator FailUpload(float waitTime) {
        yield return new WaitForSeconds(waitTime);

        // SHOW OBJECTS ---------------------------------------------------------
        connectingObject.GetComponent<Text>().enabled = false;
        userProfileText.GetComponent<Text>().enabled = true;
        userProfileText.GetComponent<Text>().text = "profile mail: " + drive.UserAccount;
        refreshButton.GetComponent<Button>().enabled = true;
        refreshButton.GetComponent<Image>().enabled = true;
        refreshButtonText.GetComponentInChildren<Text>().enabled = true;

        foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }

        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(true);
        }
        print("stop fail progress");
        //-----------------------------------------------------------------------

        uploadFail.GetComponent<Text>().enabled = false;
        StopCoroutine("FailUpload");
    }



    public IEnumerator Refresh() {
        if (drive == null || !drive.IsAuthorized || uploadTextInProgress) {
            uploadFail.GetComponent<Text>().enabled = true;
            StartCoroutine("FailUpload", 5f);

            // HIDE OBJECTS ---------------------------------------------------------
            userProfileText.GetComponent<Text>().enabled = false;
            teamButtons.SetActive(false);

            foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
                child.GetComponent<Button>().enabled = false;
                child.GetComponent<Image>().enabled = false;
                child.GetComponentInChildren<Text>().enabled = false;
            }
            foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
                child.GetComponent<Button>().enabled = false;
                child.GetComponent<Image>().enabled = false;
                child.GetComponentInChildren<Text>().enabled = false;
            }
            foreach (Transform child in userButtons.transform) {
                child.gameObject.SetActive(false);
            }
            // ----------------------------------------------------------------------

            yield break;
        }

        uploadTextInProgress = true;

        // HIDE OBJECTS ---------------------------------------------------------
        userProfileText.GetComponent<Text>().enabled = false;
        uploading.GetComponent<Text>().enabled = false;
        
        foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = false;
            child.GetComponent<Image>().enabled = false;
            child.GetComponentInChildren<Text>().enabled = false;
        }
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = false;
            child.GetComponent<Image>().enabled = false;
            child.GetComponentInChildren<Text>().enabled = false;
        }

        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(false);
        }
        // ----------------------------------------------------------------------        

        refreshingText.GetComponent<Text>().enabled = true;


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

        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponentInChildren<Text>().enabled = true;
        }
        UpdateUserButtons(ref data);
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponentInChildren<Text>().enabled = false;
        }

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
        uploading.GetComponent<Text>().enabled = false;
        refreshingText.GetComponent<Text>().enabled = false;

        // SHOW OBJECTS ---------------------------------------------------------
        connectingObject.GetComponent<Text>().enabled = false;
        userProfileText.GetComponent<Text>().enabled = true;
        userProfileText.GetComponent<Text>().text = "profile mail: " + drive.UserAccount;

        foreach (Transform child in teamButtons.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }
        foreach (Transform child in teamButtons.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }

        foreach (Transform child in userButtons.transform) {
            child.gameObject.SetActive(true);
        }
        print("stop progress");
        //-----------------------------------------------------------------------
    }
}
