using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiUpdate : MonoBehaviour
{
    public float fearModifier;
    private const string TIME_PREFS_KEY = "TIME";
    public int targetSacrificies;
    public int currentLevel;
    private static string[] sceneNames = { "Level1", "Level2", "Level3", "Level4", "Level5" };
    private int _sacrifices;
    public int sacrifices
    {
        set
        {
            _sacrifices = value;
            if (_sacrifices >= targetSacrificies && !fadingOut) {
                _fadingOut = true;
                sacrificesText.text = "Bunnies Sacrificed: " + targetSacrificies + "/" + targetSacrificies;
                victoryMusic.Play();
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
    public Text sacrificesText;
    public Text timeText;
    public Image fadeOut;
    private AudioSource backgroundMusic;
    private AudioSource victoryMusic;
    private bool _fadingOut = false;
    public bool fadingOut
    {
        get
        {
            return _fadingOut;
        }
    }
    private float fadeOutTimer = 0.0f;
    private bool fadingIn = true;
    private float fadeInTimer = 0.0f;
    private const float OUT_FADE_TIME = 20.0f;
    private const float IN_FADE_TIME = 2.0f;
    void Start()
    {
        backgroundMusic = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        victoryMusic = GameObject.Find("VictoryFade").GetComponent<AudioSource>();
        if (currentLevel > 0) {
            time = PlayerPrefs.GetFloat(TIME_PREFS_KEY, 0.0f);
        } else {
            time = 0.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fadingOut)
        {
            fadeOutTimer += Time.deltaTime;
            if (fadeOutTimer >= OUT_FADE_TIME)
            {
                PlayerPrefs.SetFloat(TIME_PREFS_KEY, time);
                if (currentLevel + 1 < sceneNames.Length)
                {
                    var nextSceneName = sceneNames[currentLevel + 1];
                    SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
                }
                else
                {
                    // We're already on the final level, head to the victory screen
                    SceneManager.LoadScene("End", LoadSceneMode.Single);
                }
            }
            else
            {
                fadeOut.color = new Color32(0,0,0,(byte)(Mathf.Min(255, 255 * (fadeOutTimer / 10f))));
                // Fade background and bring in victory over the first 2 seconds
                backgroundMusic.volume = Mathf.Max(0.0f, 1.0f - (fadeOutTimer / 2f));
                victoryMusic.volume = Mathf.Min(1.0f, fadeOutTimer / 2f);
            }
        }
        else if (fadingIn)
        {
            fadeInTimer += Time.deltaTime;
            if (fadeInTimer >= IN_FADE_TIME)
            {
                fadingIn = false;
            }
            else
            {
                fadeOut.color = new Color32(0,0,0,(byte)(255 - (255*(fadeInTimer/IN_FADE_TIME))));
            }
        }
        else
        {
            time += Time.deltaTime;
            sacrificesText.text = "Bunnies Sacrificed: " + sacrifices + "/" + targetSacrificies;
            timeText.text = "Time: " + (int)time;
        }
    }
}
