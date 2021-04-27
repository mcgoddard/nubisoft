using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject controls;
    public GameObject credits;
    private Image fadeOut;
    private bool fading = false;
    private float fadeOutTimer = 0.0f;
    private const float FADE_OUT_TIME = 3.0f;

    void Start()
    {
        fadeOut = GameObject.Find("Black").GetComponent<Image>();
    }

    void Update()
    {
        if (fading)
        {
            fadeOutTimer += Time.deltaTime;
            if (fadeOutTimer >= FADE_OUT_TIME)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
            else
            {
                fadeOut.color = new Color32(0,0,0,(byte)(255*(fadeOutTimer/FADE_OUT_TIME)));
            }
        }
        if (Input.GetKeyDown("escape"))
        {
            HideControls();
            HideCredits();
        }
    }

    public void PlayGame()
    {
        fading = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DisplayControls()
    {
        controls.SetActive(true);
    }

    public void HideControls()
    {
        controls.SetActive(false);
    }

    public void DisplayCredits()
    {
        credits.SetActive(true);
    }

    public void HideCredits()
    {
        credits.SetActive(false);
    }
}
