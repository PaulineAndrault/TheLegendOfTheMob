using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreGoogle : MonoBehaviour
{
    private string team;
    private string score;
    private string time;
    [SerializeField] private string BASE_URL = "https://docs.google.com/forms/d/e/1FAIpQLSegxenE-Aj8SAzD0Zm8cO5EwXXHclySjF-IAthwTJEXJDskNg/formResponse";

    public void Send()
    {
        team = PlayerPrefs.GetString("Name");
        score = PlayerPrefs.GetString("Score");
        time = PlayerPrefs.GetString("Time");

        StartCoroutine(Post(team, score, time));
    }

    IEnumerator Post(string team, string score, string time)
    {

        // VOIR DOC : https://www.youtube.com/watch?v=z9b5aRfrz7M
        // Pour trouver les 10 entry digits des Fields, voir : https://ninest.vercel.app/html/google-forms-embed

        WWWForm form = new WWWForm();
        form.AddField("entry.97402625", team);
        form.AddField("entry.1369830714", score);
        form.AddField("entry.83495186", time);

        UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }
}
