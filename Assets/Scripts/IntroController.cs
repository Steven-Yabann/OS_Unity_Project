using UnityEngine;

public class IntroController : MonoBehaviour
{
    public GameObject introPanel;

    public void StartSimulation()
    {
        introPanel.SetActive(false); // Hide the introduction panel
    }
}