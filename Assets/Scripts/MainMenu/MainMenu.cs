using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private Image fadeOut;
    private bool fading = false;
    private float fadeOutTimer = 0.0f;
    private const float FADE_OUT_TIME = 5.0f;

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
    }

    public void PlayGame ()
    {
        fading = true;
    }
}
