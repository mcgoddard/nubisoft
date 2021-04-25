using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    public enum State
    {
        FadeIn,
        Visible,
        FadeOut,
    }
    private Image fadeOut;
    private Text timeTakenText;
    private Text bestTimeText;
    private State state = State.FadeIn;
    private float fadeInTimer = 0.0f;
    private float visibleTimer = 0.0f;
    private float fadeOutTimer = 0.0f;
    private const float VISIBLE_TIME = 106.0f;
    private const float OUT_FADE_TIME = 5.0f;
    private const float IN_FADE_TIME = 10.0f;
    private const string TIME_TAKEN_PREFIX = "Time taken: ";
    private const string BEST_TIME_PREFIX = "Best time: ";
    private const string NEW_HIGH_SCORE = "New high score!";
    private const string TIME_PREFS_KEY = "TIME";
    private const string HIGH_SCORE_PREFS_KEY = "HIGH_SCORE";

    void Start()
    {
        fadeOut = GameObject.Find("Black").GetComponent<Image>();
        timeTakenText = GameObject.Find("TimeTaken").GetComponent<Text>();
        bestTimeText = GameObject.Find("BestTime").GetComponent<Text>();
    }

    void Update()
    {
        switch (state)
        {
            case State.FadeIn:
                fadeInTimer += Time.deltaTime;
                fadeOut.color = new Color32(0,0,0,(byte)(255 - (255*(fadeInTimer/IN_FADE_TIME))));
                if (fadeInTimer >= IN_FADE_TIME)
                {
                    state = State.Visible;
                    var timeTaken = PlayerPrefs.GetFloat(TIME_PREFS_KEY, 99999999.9f);
                    var highScore = PlayerPrefs.GetFloat(HIGH_SCORE_PREFS_KEY, 99999999.9f);
                    timeTakenText.text = TIME_TAKEN_PREFIX + (int)timeTaken;
                    if (timeTaken < highScore)
                    {
                        PlayerPrefs.SetFloat(HIGH_SCORE_PREFS_KEY, timeTaken);
                        bestTimeText.text = NEW_HIGH_SCORE;
                    }
                    else
                    {
                        bestTimeText.text = BEST_TIME_PREFIX + (int)highScore;
                    }
                }
                break;
            case State.Visible:
                visibleTimer += Time.deltaTime;
                if (visibleTimer >= VISIBLE_TIME)
                {
                    state = State.FadeOut;
                    // We don't actually fade here, just cut to black
                    fadeOut.color = new Color32(0,0,0,255);
                    timeTakenText.text = "";
                    bestTimeText.text = "";
                }
                break;
            case State.FadeOut:
                fadeOutTimer += Time.deltaTime;
                if (fadeOutTimer >= OUT_FADE_TIME)
                {
                    // Load back to the menu automatically after the timeout
                    SceneManager.LoadScene("Menu", LoadSceneMode.Single);
                }
                break;
        }
    }
}
