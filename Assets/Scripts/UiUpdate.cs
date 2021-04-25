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
    private static string[] sceneNames = { "Level1", "Level2", "Level3", "Level4", "Level5" };
    private int _sacrifices;
    public int sacrifices
    {
        set
        {
            _sacrifices = value;
            if (_sacrifices >= targetSacrificies) {
                if (currentLevel < sceneNames.Length) {
                    fadingOut = true;
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
    private Image fadeOut;
    private bool fadingOut = false;
    private float fadeOutTimer = 0.0f;
    private bool fadingIn = true;
    private float fadeInTimer = 0.0f;
    private const float FADE_TIME = 5.0f;
    void Start()
    {
        fadeOut = GameObject.Find("Black").GetComponent<Image>();
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
        if (fadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            if (fadeOutTimer >= FADE_TIME)
            {
                PlayerPrefs.SetFloat(timePrefsKey, time);
                var nextSceneName = sceneNames[currentLevel + 1];
                SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
            else
            {
                fadeOut.color = new Color32(0,0,0,(byte)(255*(fadeOutTimer/FADE_TIME)));
            }
        }
        else if (fadingIn)
        {
            fadeInTimer += Time.deltaTime;
            if (fadeInTimer >= FADE_TIME)
            {
                fadingIn = false;
            }
            else
            {
                fadeOut.color = new Color32(0,0,0,(byte)(255 - (255*(fadeInTimer/FADE_TIME))));
            }
        }
        else
        {
            time += Time.deltaTime;
            sacrificesText.text = "Bunnies Sacrificed: " + sacrifices;
            killsText.text = "Bunnies Killed: " + kills;
            fearText.text = "Fear Level: " + fear;
            bunniesText.text = "Bunnies: " + bunnies;
            timeText.text = "Time: " + (int)time;
        }
    }
}
