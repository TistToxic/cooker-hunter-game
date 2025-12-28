using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public GameObject optionsPanel;

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }
}
