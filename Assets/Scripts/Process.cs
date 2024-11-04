using UnityEngine;
using UnityEngine.UI;

public class Process : MonoBehaviour
{
    public string processID;
    public float arrivalTime;
    public float burstTime;
    public float waitingTime;
    public float turnaroundTime;

    public Text processText;

    public void InitializeProcess(string id, float arrival, float burst)
    {
        processID = id;
        arrivalTime = arrival;
        burstTime = burst;
        UpdateText();
    }

    public void UpdateText()
    {
        if (processText != null)
        {
            processText.text = $"ID: {processID} | Burst: {burstTime}";
        }
    }
}