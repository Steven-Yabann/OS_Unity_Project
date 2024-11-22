using UnityEngine;
using TMPro;
using System.Collections;

public class Teller : MonoBehaviour
{
    public int tellerId; // Unique ID for this teller (0, 1, or 2)
    public TextMeshPro cashDrawerText; // Reference to the cash drawer's TextMeshPro
    public TextMeshPro tellerStatusText; // Reference to this teller's status text
    public TextMeshPro debugText; // Reference to DebugText for real-time updates

    private void Start()
    {
        StartCoroutine(ServeCustomers());
    }

    private IEnumerator ServeCustomers()
    {
        while (true)
        {
            // Wait until it's safe to enter the critical section
            yield return StartCoroutine(EnterCriticalSection());

            // Critical section: Using the drawer
            tellerStatusText.text = "Accessing Drawer";
            cashDrawerText.text = $"Teller {tellerId + 1} is using the drawer";
            yield return new WaitForSeconds(2); // Simulate drawer usage time

            ExitCriticalSection();

            // Exit critical section
            tellerStatusText.text = "Waiting";
            cashDrawerText.text = "Drawer is idle";
            yield return new WaitForSeconds(2); // Simulate waiting time
        }
    }

    private IEnumerator EnterCriticalSection()
    {
        DrawerController.Instance.Flag[tellerId] = true;
        DrawerController.Instance.Turn = (tellerId + 1) % 3; // Determine the next teller's turn

        // Update debug text
        UpdateDebugText();

        // Wait until it’s this teller’s turn or no other teller is interested
        while (AnyOtherTellerInterested() && DrawerController.Instance.Turn != tellerId)
        {
            yield return null; // Non-blocking wait
        }
    }

    private void ExitCriticalSection()
    {
        DrawerController.Instance.Flag[tellerId] = false;

        // Update debug text
        UpdateDebugText();
    }

    private bool AnyOtherTellerInterested()
    {
        // Check if any other teller is interested in the critical section
        for (int i = 0; i < DrawerController.Instance.Flag.Length; i++)
        {
            if (i != tellerId && DrawerController.Instance.Flag[i]) return true;
        }
        return false;
    }

    private void UpdateDebugText()
    {
        bool[] flags = DrawerController.Instance.Flag; // Access flag array
        int turn = DrawerController.Instance.Turn; // Access turn variable

        // Format and update the debug text
        debugText.text = $"Flag[0]: {flags[0]}\nFlag[1]: {flags[1]}\nFlag[2]: {flags[2]}\nTurn: {turn}";
    }
}
