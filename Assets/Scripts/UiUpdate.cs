using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiUpdate : MonoBehaviour
{
    // Start is called before the first frame update
    public static int sacrifices = 0;
    public static int kills = 0;
    public static int bunnies = 0;
    public static float fear = 5.0f;
    public static float time = 0.0f;
    private Text sacrificesText;
    private Text killsText;
    private Text fearText;
    private Text bunniesText;
    private Text timeText;
    void Start()
    {
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
