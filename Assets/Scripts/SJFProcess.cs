using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SJFProcess : MonoBehaviour
{
    [Header("Process Settings")]
    public GameObject[] processSprites;   
    public float spacing = 2f;            
    public float moveSpeed = 2f;          

    [Header("UI Elements")]
    public Dropdown spriteDropdown;       
    public Dropdown processingTimeDropdown; 
    public Button addButton;              
    public Button startSimulationButton;  
    public Button resetButton;            
    public Text resultsText;              

    [Header("CPU Elements")]
    public GameObject cpuSprite;          
    public float cpuMoveSpeed = 2f;       

    private List<Process> processes = new List<Process>();
    private List<Process> completedProcesses = new List<Process>();
    private bool isProcessing = false;    
    private float currentTime = 0f;       
    private int spawnCount = 0;           

    void Start()
    {
        PopulateSpriteDropdown();
        PopulateProcessingTimeDropdown();

        addButton.onClick.AddListener(AddProcess);
        startSimulationButton.onClick.AddListener(StartSJFSimulation);
        resetButton.onClick.AddListener(ResetSimulation);
    }

    void PopulateSpriteDropdown()
    {
        spriteDropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (GameObject sprite in processSprites)
        {
            options.Add(sprite.name);
        }
        spriteDropdown.AddOptions(options);
    }

    void PopulateProcessingTimeDropdown()
    {
        processingTimeDropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 1; i <= 6; i++)
        {
            options.Add(i + " seconds");
        }
        processingTimeDropdown.AddOptions(options);
    }

    void AddProcess()
    {
        int selectedSpriteIndex = spriteDropdown.value;
        int selectedProcessingTimeIndex = processingTimeDropdown.value;
        float processingTime = selectedProcessingTimeIndex + 1;

        float spawnX = processes.Count * spacing;
        float spawnY = -4.5f;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        GameObject selectedSprite = Instantiate(processSprites[selectedSpriteIndex], spawnPosition, Quaternion.identity);

        GameObject statusText = new GameObject("StatusText");
        statusText.transform.SetParent(selectedSprite.transform);

        TextMesh textMesh = statusText.AddComponent<TextMesh>();
        textMesh.text = $"P{processes.Count + 1}\nWaiting\nBurst Time: {processingTime}s";
        textMesh.fontSize = 15; 
        textMesh.color = Color.white; 
        textMesh.anchor = TextAnchor.MiddleCenter;

        statusText.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        statusText.transform.localPosition = new Vector3(0, 1.2f, 0);

        Process newProcess = new Process(selectedSprite, processingTime, currentTime);
        processes.Add(newProcess);
    }

    void StartSJFSimulation()
    {
        if (!isProcessing && processes.Count > 0)
        {
            StartCoroutine(ProcessSJF());
        }
    }

    IEnumerator ProcessSJF()
    {
        isProcessing = true;

        while (processes.Count > 0)
        {
            Process shortestProcess = FindShortestProcess();
            float cpuTargetX = shortestProcess.processObject.transform.position.x;
            yield return StartCoroutine(MoveCPUTowardsProcess(cpuTargetX));

            TextMesh statusText = shortestProcess.processObject.GetComponentInChildren<TextMesh>();
            if (statusText != null)
            {
                statusText.text = $"P{processes.Count}\nExecuting\nTime Left: {shortestProcess.processingTime.ToString("0.##")}s";
                statusText.color = Color.green;
            }

            float targetY = shortestProcess.processObject.transform.position.y + 3f;
            yield return StartCoroutine(MoveProcessSmoothly(shortestProcess.processObject, targetY));

            float remainingTime = shortestProcess.processingTime;
            while (remainingTime > 0)
            {
                if (statusText != null)
                {
                    statusText.text = $"P{processes.Count}\nExecuting\nTime Left: {remainingTime.ToString("0.##")}s";
                }
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (statusText != null)
            {
                statusText.text = $"P{processes.Count}\nCompleted\nTime: {shortestProcess.processingTime.ToString("0.##")}s";
                statusText.color = Color.gray;
            }

            shortestProcess.completionTime = currentTime + shortestProcess.processingTime;
            shortestProcess.turnaroundTime = shortestProcess.completionTime - shortestProcess.arrivalTime;
            shortestProcess.waitingTime = shortestProcess.turnaroundTime - shortestProcess.processingTime;

            currentTime += shortestProcess.processingTime;
            processes.Remove(shortestProcess);
            completedProcesses.Add(shortestProcess);
        }

        DisplayResults();
        isProcessing = false;
    }

    Process FindShortestProcess()
    {
        Process shortest = processes[0];
        foreach (var process in processes)
        {
            if (process.processingTime < shortest.processingTime)
            {
                shortest = process;
            }
        }
        return shortest;
    }

    IEnumerator MoveCPUTowardsProcess(float targetX)
    {
        float startX = cpuSprite.transform.position.x;
        float elapsedTime = 0f;
        float duration = 1.5f;

        while (elapsedTime < duration)
        {
            float newX = Mathf.Lerp(startX, targetX, (elapsedTime / duration));
            cpuSprite.transform.position = new Vector3(newX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cpuSprite.transform.position = new Vector3(targetX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);
    }

    IEnumerator MoveProcessSmoothly(GameObject process, float targetY)
    {
        float startY = process.transform.position.y;
        float elapsedTime = 0f;
        float duration = 1f;

        while (elapsedTime < duration)
        {
            process.transform.position = Vector3.Lerp(new Vector3(process.transform.position.x, startY, 0),
                                                       new Vector3(process.transform.position.x, targetY, 0),
                                                       (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        process.transform.position = new Vector3(process.transform.position.x, targetY, 0);
    }

    void DisplayResults()
    {
        float totalWaitingTime = 0f;
        float totalTurnaroundTime = 0f;

        resultsText.text = "Process Results:\n";

        foreach (var process in completedProcesses)
        {
            resultsText.text += $"{process.processObject.name}: " +
                                $"WT = {process.waitingTime:F2}, " +
                                $"TAT = {process.turnaroundTime:F2}\n";

            totalWaitingTime += process.waitingTime;
            totalTurnaroundTime += process.turnaroundTime;
        }

        if (completedProcesses.Count > 0)
        {
            float avgWaitingTime = totalWaitingTime / completedProcesses.Count;
            float avgTurnaroundTime = totalTurnaroundTime / completedProcesses.Count;

            resultsText.text += $"\nAverage Waiting Time: {avgWaitingTime:F2}\n";
            resultsText.text += $"Average Turnaround Time: {avgTurnaroundTime:F2}";
        }
    }

    void ResetSimulation()
    {
        StopAllCoroutines();
        isProcessing = false;

        cpuSprite.transform.position = new Vector3(-6f, 0f, 0f);

        foreach (var process in processes)
        {
            Destroy(process.processObject);
        }

        processes.Clear();
        resultsText.text = "Process Results:\n";
        currentTime = 0f;
        spawnCount = 0;
    }
}

