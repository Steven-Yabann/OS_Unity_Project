using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CPUProcessSimulator : MonoBehaviour
{
    [Header("Process Settings")]
    public GameObject[] processSprites;  
    public float spacing = 2f;           
    public float moveSpeed = 2f;         

    [Header("CPU Settings")]
    public GameObject cpuSprite;  
    public float cpuMoveSpeed = 2f;

    [Header("UI Elements")]
    public Dropdown spriteDropdown;       
    public Dropdown processingTimeDropdown; 
    public Button addButton;              
    public Button startSimulationButton;  
    public Button resetButton;            
    public Text resultsText;              

    private List<Process> processes = new List<Process>();
    private int spawnCount = 0;           
    private bool isProcessing = false;    
    private float currentTime = 0f;       

    void Start()
    {
        PopulateSpriteDropdown();
        PopulateProcessingTimeDropdown();

        addButton.onClick.AddListener(AddProcess);
        startSimulationButton.onClick.AddListener(StartFCFSSimulation);
        resetButton.onClick.AddListener(ResetSimulation);

        cpuSprite.transform.position = new Vector3(-6f, 0f, 0f);
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

        float spawnX = spawnCount * spacing;
        float spawnY = -4.5f;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        GameObject selectedSprite = Instantiate(processSprites[selectedSpriteIndex], spawnPosition, Quaternion.identity);

        GameObject statusText = new GameObject("StatusText");
        statusText.transform.SetParent(selectedSprite.transform);

        TextMesh textMesh = statusText.AddComponent<TextMesh>();
        textMesh.text = $"P{spawnCount + 1}\nWaiting\nBurst Time: {processingTime}s";
        textMesh.fontSize = 15; 
        textMesh.color = Color.white; 
        textMesh.anchor = TextAnchor.MiddleCenter; 

        statusText.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        statusText.transform.localPosition = new Vector3(0, 1.2f, 0); 

        Process newProcess = new Process(selectedSprite, processingTime, currentTime);
        processes.Add(newProcess);

        spawnCount++;
    }

    IEnumerator MoveCPUTowardsProcess(float targetX)
    {
        float startX = cpuSprite.transform.position.x;
        float elapsedTime = 0f;
        float duration = 1f;

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

    void StartFCFSSimulation()
    {
        if (!isProcessing && processes.Count > 0)
        {
            StartCoroutine(ProcessFCFS());
        }
    }

    IEnumerator ProcessFCFS()
    {
        isProcessing = true;

        foreach (var process in processes)
        {
            float cpuTargetX = process.processObject.transform.position.x;
            yield return StartCoroutine(MoveCPUTowardsProcess(cpuTargetX));

            TextMesh statusText = process.processObject.GetComponentInChildren<TextMesh>();
            if (statusText != null)
            {
                statusText.text = $"P{spawnCount}\nExecuting\nTime Left: {process.processingTime}s";
                statusText.color = Color.green;
            }

            float targetY = process.processObject.transform.position.y + 3f;
            yield return StartCoroutine(MoveProcessSmoothly(process.processObject, targetY));

            float remainingTime = process.processingTime;
            while (remainingTime > 0)
            {
                if (statusText != null)
                {
                    statusText.text = $"P{spawnCount}\nExecuting\nTime Left: {remainingTime.ToString("0.##")}s";
                }

                remainingTime -= Time.deltaTime;
                yield return null;
            }

            if (statusText != null)
            {
                statusText.text = $"P{spawnCount}\nCompleted\nTime: {process.processingTime.ToString("0.##")}s";
                statusText.color = Color.gray;
            }

            process.completionTime = currentTime + process.processingTime;
            process.turnaroundTime = process.completionTime - process.arrivalTime;
            process.waitingTime = process.turnaroundTime - process.processingTime;

            currentTime += process.processingTime;
        }

        DisplayResults();

        isProcessing = false;
    }

    void DisplayResults()
    {
        float totalWaitingTime = 0f;
        float totalTurnaroundTime = 0f;

        resultsText.text = "Process Results:\n";

        foreach (var process in processes)
        {
            resultsText.text += $"{process.processObject.name}: " +
                                $"WT = {process.waitingTime:F2}, " +
                                $"TAT = {process.turnaroundTime:F2}\n";

            totalWaitingTime += process.waitingTime;
            totalTurnaroundTime += process.turnaroundTime;
        }

        float avgWaitingTime = totalWaitingTime / processes.Count;
        float avgTurnaroundTime = totalTurnaroundTime / processes.Count;

        resultsText.text += $"\nAverage Waiting Time: {avgWaitingTime:F2}\n";
        resultsText.text += $"Average Turnaround Time: {avgTurnaroundTime:F2}";
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

[System.Serializable]
public class Process
{
    public GameObject processObject;
    public float processingTime;
    public float arrivalTime;
    public float completionTime;
    public float turnaroundTime;
    public float waitingTime;

    public Process(GameObject processObject, float processingTime, float arrivalTime)
    {
        this.processObject = processObject;
        this.processingTime = processingTime;
        this.arrivalTime = arrivalTime;
    }
}
