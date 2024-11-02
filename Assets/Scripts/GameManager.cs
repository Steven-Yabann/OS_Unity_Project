using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject processPrefab;
    public Transform queuePanel;
    public Transform cpuPanel;

    private List<Process> processQueue = new List<Process>();
    private float currentTime = 0f;

    private void Start()
    {
        // Initialize some example processes
        InitializeProcesses();
        StartCoroutine(FCFSSimulation());
    }

    private void InitializeProcesses()
    {
        // Example process setup
        CreateProcess("P1", 0f, 3f);
        CreateProcess("P2", 2f, 6f);
        CreateProcess("P3", 4f, 1f);
    }

    private void CreateProcess(string id, float arrival, float burst)
    {
        GameObject newProcess = Instantiate(processPrefab, queuePanel);
        Process processComponent = newProcess.GetComponent<Process>();
        processComponent.InitializeProcess(id, arrival, burst);
        processQueue.Add(processComponent);
    }

    private IEnumerator FCFSSimulation()
    {
        foreach (Process process in processQueue)
        {
            // Wait until the process's arrival time
            while (currentTime < process.arrivalTime)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }

            // Execute process
            yield return StartCoroutine(ExecuteProcess(process));
        }
    }

    private IEnumerator ExecuteProcess(Process process)
    {
        process.transform.SetParent(cpuPanel); // Move process to CPU area
        process.UpdateText();

        float startTime = currentTime;
        float burstDuration = process.burstTime;

        while (currentTime < startTime + burstDuration)
        {
            yield return null;
            currentTime += Time.deltaTime;
        }

        // Mark process as completed
        process.waitingTime = startTime - process.arrivalTime;
        process.turnaroundTime = currentTime - process.arrivalTime;
        process.transform.SetParent(null); // Remove from CPU
    }

//     private void DisplayMetrics()
// {
//     float totalWaitingTime = 0f;
//     float totalTurnaroundTime = 0f;

//     foreach (Process process in processQueue)
//     {
//         totalWaitingTime += process.waitingTime;
//         totalTurnaroundTime += process.turnaroundTime;
//     }

//     float avgWaitingTime = totalWaitingTime / processQueue.Count;
//     float avgTurnaroundTime = totalTurnaroundTime / processQueue.Count;

//     // Display the averages on UI elements
//     waitingTimeText.text = $"Average Waiting Time: {avgWaitingTime:F2}";
//     turnaroundTimeText.text = $"Average Turnaround Time: {avgTurnaroundTime:F2}";
// }
}