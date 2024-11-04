using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject processPrefab; // Reference to the Process prefab
    public Transform queuePanel;     // Reference to the Queue panel
    public Transform cpuPanel;       // Reference to the CPU panel

    private List<Process> processQueue = new List<Process>();
    private float currentTime = 0f;

    private void Start()
    {
        InitializeProcesses();
        StartCoroutine(FCFSSimulation());
    }

    private void InitializeProcesses()
    {
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
            while (currentTime < process.arrivalTime)
            {
                yield return null;
                currentTime += Time.deltaTime;
            }
            yield return StartCoroutine(ExecuteProcess(process));
        }
    }

    private IEnumerator ExecuteProcess(Process process)
    {
        process.transform.SetParent(cpuPanel); // Move to CPU
        float startTime = currentTime;
        float burstDuration = process.burstTime;

        while (currentTime < startTime + burstDuration)
        {
            yield return null;
            currentTime += Time.deltaTime;
        }

        process.waitingTime = startTime - process.arrivalTime;
        process.turnaroundTime = currentTime - process.arrivalTime;
        process.transform.SetParent(null);
    }
}