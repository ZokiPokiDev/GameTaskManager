using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GetTask : MonoBehaviour {

    public GameObject cantAssigne;
    public GameObject mainInterfacePanel, userInterfacePanel;
    
    public void UpdateButtonText(GameObject obj) {
        if (transform.GetComponentInChildren<Text>().text == "none") {
            transform.GetComponentInChildren<Text>().text = obj.GetComponent<Text>().text;
            transform.GetComponent<Image>().color = Color.red;
        }
        else {
            if (transform.GetComponentInChildren<Text>().text == obj.GetComponent<Text>().text) {
                transform.GetComponentInChildren<Text>().text = "none";
                transform.GetComponent<Image>().color = Color.green;
            }
            else {
                cantAssigne.transform.GetComponent<Text>().enabled = true;

                foreach (Transform child in userInterfacePanel.transform) {
                    if (child.GetComponent<Button>() != null)
                        child.GetComponent<Button>().enabled = false;
                    if (child.GetComponent<Image>() != null)
                        child.GetComponent<Image>().enabled = false;
                    if (child.GetComponentInChildren<Text>() != null)
                        child.GetComponentInChildren<Text>().enabled = false;
                    if (child.transform.childCount > 1)
                        child.transform.GetChild(1).GetComponent<Text>().enabled = false;
                    if (child.GetComponent<Text>() != null)
                        child.GetComponent<Text>().enabled = false;
                }

                foreach (Transform child in mainInterfacePanel.transform.GetChild(0).transform) {
                    child.GetComponent<Button>().enabled = false;
                    child.GetComponent<Image>().enabled = false;
                    child.GetComponentInChildren<Text>().enabled = false;
                }
                foreach (Transform child in mainInterfacePanel.transform.GetChild(1).transform) {
                    child.GetComponent<Button>().enabled = false;
                    child.GetComponent<Image>().enabled = false;
                    child.GetComponentInChildren<Text>().enabled = false;
                }

                print("start corutine for access denied");
                StartCoroutine("AcessDenied", 1f);
            }
        }
    }

    IEnumerator AcessDenied(float waitTime) {
        yield return new WaitForSeconds(waitTime);

        cantAssigne.transform.GetComponent<Text>().enabled = false;

        foreach (Transform child in userInterfacePanel.transform) {
            if (child.GetComponent<Button>() != null)
                child.GetComponent<Button>().enabled = true;
            if (child.GetComponent<Image>() != null)
                child.GetComponent<Image>().enabled = true;
            if (child.GetComponentInChildren<Text>() != null)
                child.GetComponentInChildren<Text>().enabled = true;
            if (child.transform.childCount > 1)
                child.transform.GetChild(1).GetComponent<Text>().enabled = true;
            if (child.GetComponent<Text>() != null)
                child.GetComponent<Text>().enabled = true;
        }

        foreach (Transform child in mainInterfacePanel.transform.GetChild(0).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }
        foreach (Transform child in mainInterfacePanel.transform.GetChild(1).transform) {
            child.GetComponent<Button>().enabled = true;
            child.GetComponent<Image>().enabled = true;
            child.GetComponentInChildren<Text>().enabled = true;
        }

        print("finished corutine for access denied");
        StopCoroutine("AcessDenied");
    }
}
