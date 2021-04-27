using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    private float timer = 0.0f;
    private const float timerLength = 118f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= timerLength || Input.anyKey)
        {
            SceneManager.LoadScene("Menu", LoadSceneMode.Single);
        }
    }
}
