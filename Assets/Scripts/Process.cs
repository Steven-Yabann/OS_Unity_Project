using UnityEngine;
using UnityEngine.UI;

public class Process : MonoBehaviour
{
    public string processID;
    public float arrivalTime;
    public float burstTime;
    public float remainingTime;
    public float waitingTime;
    public float turnaroundTime;

    public Text processText;

    // Initialize the process with ID, arrival, and burst times
    public void InitializeProcess(string id, float arrival, float burst)
    {
        processID = id;
        arrivalTime = arrival;
        burstTime = burst;
        remainingTime = burst;
        UpdateText();
    }

    public void UpdateText()
    {
        processText.text = $"ID: {processID} | BT: {remainingTime}";
    }

    
}
