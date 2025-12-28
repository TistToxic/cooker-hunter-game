using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Dialogue : MonoBehaviour
{
    public TMP_Text text;
    public float textDisplayTimeSec = 2f;
    private float timer = 0f;
    private string[] dialogueLines =
    {
        "Retirement sure is boring, what can I do today?",
        "Hmm....",
        "I know!",
        "Let's relive the glory days!",
        "Now I just need to find some fresh ingredients..."
    };
    private int dialogueIndex = 0;
    private string displayedDialogue = "";
    private float timeToDisplayChar = 0.1f;
    private int charIndex = 0;

    void Update()
    {
        // If dialogue is finished
        if (dialogueIndex >= dialogueLines.Length || Input.GetKey(KeyCode.Space))
        {
            SceneManager.LoadScene("Garden");
        }

        // Advance the dialogue
        timer += Time.deltaTime;
        if (timer >= textDisplayTimeSec)
        {
            dialogueIndex++;
            timer -= textDisplayTimeSec;
            displayedDialogue = "";
            charIndex = 0;
        }
        // Dialogue typing animation
        if (timer >= timeToDisplayChar && charIndex < dialogueLines[dialogueIndex].Length)
        {
            displayedDialogue += dialogueLines[dialogueIndex][charIndex];
            charIndex++;
            timer -= timeToDisplayChar;
        }

        // Display dialogue
        if (dialogueIndex < dialogueLines.Length)
        {
            text.text = displayedDialogue;
        }
    }

}
