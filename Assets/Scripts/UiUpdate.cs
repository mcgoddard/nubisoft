using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiUpdate : MonoBehaviour
{
    private const string timePrefsKey = "TIME";
    // Start is called before the first frame update
    public int targetSacrificies;
    public int currentLevel;
    private static string[] sceneNames = { "Game", "Level2", "Level3", "Level4", "Level5" };
    private int _sacrifices;
    public int sacrifices
    {
        set
        {
            _sacrifices = value;
            if (_sacrifices >= targetSacrificies) {
                if (currentLevel < sceneNames.Length) {
                    // TODO effect? Fade out or rumble or something
                    PlayerPrefs.SetFloat(timePrefsKey, time);
                    var nextSceneName = sceneNames[currentLevel + 1];
                    SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
                }
                else
                {
                    // TODO end game
                    PlayerPrefs.SetFloat(timePrefsKey, 0.0f);
                }
            }
        }
        get
        {
            return _sacrifices;
        }
    }
    public int kills = 0;
    public int bunnies = 0;
    public float fear = 5.0f;
    public float time = 0.0f;
    private Text sacrificesText;
    private Text killsText;
    private Text fearText;
    private Text bunniesText;
    private Text timeText;
    void Start()
    {
        if (currentLevel > 0) {
            time = PlayerPrefs.GetFloat(timePrefsKey, 0.0f);
        } else {
            time = 0.0f;
        }
        sacrificesText = this.transform.Find("Sacrifices").GetComponent<Text>();
        killsText = this.transform.Find("Kills").GetComponent<Text>();
        fearText = this.transform.Find("Fear").GetComponent<Text>();
        bunniesText = this.transform.Find("Bunnies").GetComponent<Text>();
        timeText = this.transform.Find("Time").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        sacrificesText.text = "Bunnies Sacrificed: " + sacrifices;
        killsText.text = "Bunnies Killed: " + kills;
        fearText.text = "Fear Level: " + fear;
        bunniesText.text = "Bunnies: " + bunnies;
        timeText.text = "Time: " + (int)time;
    }
}
