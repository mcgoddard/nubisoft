using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    private string[] tutorialText = {
        "Welcome to the island of Pthleng, Great One. Iä! Iä!",
        "Only the raising of the ancient ziggurat Ubbo-Sathla will restore you once more to this world.",
        "This requires that sacrifices must be made. Bunnies. All of them.",
        "Allow terror to spread for the ritual to begin, thus…",
        "Caress the mortal with your left tentacle to bring motivation to his puny mind.",
        "Slap the ground with your right tentacle to bring a hateful hoppity rabbit up from his hole.",
        "But beware. The vermin seem to calm our servants. They work against us! Maintain the horror!",
        "A mortal that knows too much horror is a useless mortal.",
        "A mortal that does not know fear is a useless mortal.",
        "Proceed, Great One! Iä! Iä!",
    };
    private float tutorialTime = 0.0f;
    private Text uiText;

    // Start is called before the first frame update
    void Start()
    {
        uiText = GameObject.Find("Tutorial").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        tutorialTime += Time.deltaTime;
        var tutorialIndex = Mathf.FloorToInt(tutorialTime / 8.0f);
        if (tutorialIndex < tutorialText.Length)
        {
            uiText.text = tutorialText[tutorialIndex];
        }
    }
}
