using TMPro;
using UnityEngine;

public class AlgorithmDebugger : MonoBehaviour
{
    public TextMeshProUGUI debugText; // Reference to the TextMeshPro object
    public bool[] flag = new bool[3]; // Flags for tellers (Teller 0, Teller 1, Teller 2)
    public int turn; // Current turn (shared variable)

    void Start()
    {
        // Initialize flags and turn
        flag[0] = false;
        flag[1] = false;
        flag[2] = false;
        turn = 0;

        // Display initial values
        UpdateDebugLog();
    }

    public void UpdateAlgorithmState(bool[] flags, int currentTurn)
    {
        // Update variables
        flag = flags;
        turn = currentTurn;

        // Update the debug text on screen
        UpdateDebugLog();
    }

    void UpdateDebugLog()
    {
        debugText.text = $"Flag[0]: {flag[0]}\nFlag[1]: {flag[1]}\nFlag[2]: {flag[2]}\nTurn: {turn}";
    }
}
