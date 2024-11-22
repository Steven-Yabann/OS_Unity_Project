using UnityEngine;
using UnityEngine.UI;

public class SimulationController : MonoBehaviour
{
    public Button manualModeButton;
    public Button restartButton;
    public Teller teller1;
    public Teller teller2;
    public Teller teller3;

    void Start()
    {
        manualModeButton.onClick.AddListener(EnableManualMode);
        restartButton.onClick.AddListener(RestartSimulation);
    }

    void EnableManualMode()
    {
        // Example of manual mode logic
        StopAllCoroutines(); // Stops automatic alternation
        Debug.Log("Manual Mode Enabled!");
    }

    void RestartSimulation()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
