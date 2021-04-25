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
    private State state = State.FadeIn;
    private float fadeInTimer = 0.0f;
    private float visibleTimer = 0.0f;
    private float fadeOutTimer = 0.0f;
    private const float VISIBLE_TIME = 106.0f;
    private const float OUT_FADE_TIME = 5.0f;
    private const float IN_FADE_TIME = 10.0f;

    void Start()
    {
        fadeOut = GameObject.Find("Black").GetComponent<Image>();
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
                }
                break;
            case State.Visible:
                visibleTimer += Time.deltaTime;
                if (visibleTimer >= VISIBLE_TIME)
                {
                    state = State.FadeOut;
                }
                break;
            case State.FadeOut:
                fadeOutTimer += Time.deltaTime;
                // We don't actually fade here, just cut to black
                fadeOut.color = new Color32(0,0,0,255);
                if (fadeOutTimer >= OUT_FADE_TIME)
                {
                    // Load back to the menu automatically after the timeout
                    SceneManager.LoadScene("Menu", LoadSceneMode.Single);
                }
                break;
        }
    }
}
