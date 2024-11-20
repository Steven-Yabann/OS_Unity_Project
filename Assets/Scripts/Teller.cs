using UnityEngine;
using TMPro;
using System.Collections;

public class Teller : MonoBehaviour
{
    public int tellerId; // Unique ID for this teller (0 or 1)
    public TextMeshPro cashDrawerText; // Reference to the cash drawer's TextMeshPro
    public TextMeshPro tellerStatusText; // Reference to this teller's status text

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
        int otherTeller = 1 - tellerId; // Determine the other teller's ID
        DrawerController.Instance.Flag[tellerId] = true;
        DrawerController.Instance.Turn = otherTeller;

        // Wait until it’s this teller’s turn or the other teller isn’t interested
        while (DrawerController.Instance.Flag[otherTeller] &&
               DrawerController.Instance.Turn == otherTeller)
        {
            yield return null; // Non-blocking wait
        }
    }

    private void ExitCriticalSection()
    {
        DrawerController.Instance.Flag[tellerId] = false;
    }
}
