using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CPUProcessSimulator : MonoBehaviour
{
    [Header("Process Settings")]
    public GameObject[] processSprites;   // Array of available sprites
    public float spacing = 2f;            // Horizontal spacing between processes
    public float moveSpeed = 2f;          // Speed at which processes move up

    [Header("CPU Settings")]
    public GameObject cpuSprite;  // CPU sprite or object
    public float cpuMoveSpeed = 2f;

    [Header("UI Elements")]
    public Dropdown spriteDropdown;       // Dropdown to select the sprite
    public Dropdown processingTimeDropdown; // Dropdown for selecting processing time
    public Button addButton;              // Button to add a process
    public Button startSimulationButton;  // Button to start the FCFS simulation
    public Button resetButton;            // Button to reset the simulation
    public Text resultsText;              // UI Text to display results

    private List<Process> processes = new List<Process>(); // List to store processes
    private int spawnCount = 0;           // Tracks position of spawned processes in a row
    private bool isProcessing = false;    // Flag to indicate if a process is being executed
    private float currentTime = 0f;       // Tracks simulation time

    void Start()
    {
        // Populate dropdowns with options
        PopulateSpriteDropdown();
        PopulateProcessingTimeDropdown();

        // Set up the Add button to call AddProcess when clicked
        addButton.onClick.AddListener(AddProcess);

        // Set up the Start Simulation button to begin FCFS processing
        startSimulationButton.onClick.AddListener(StartFCFSSimulation);

        // Set up the Reset button to reset the simulation
        resetButton.onClick.AddListener(ResetSimulation);

        cpuSprite.transform.position = new Vector3(-6f, 0f, 0f);
    }

    // Populate the sprite dropdown with sprite names
    void PopulateSpriteDropdown()
    {
        spriteDropdown.ClearOptions();
        List<string> options = new List<string>();

        foreach (GameObject sprite in processSprites)
        {
            // Use the GameObject's name as the dropdown option
            options.Add(sprite.name);
        }

        spriteDropdown.AddOptions(options);
    }

    // Populate the processing time dropdown with options from 1 to 6
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

    // Method to add a new process based on user input
    void AddProcess()
    {
        if (spriteDropdown.value < 0 || processingTimeDropdown.value < 0)
        {
            Debug.LogWarning("Invalid input: Please select a sprite and processing time.");
            return;
        }

        // Get the selected sprite index from the dropdown
        int selectedSpriteIndex = spriteDropdown.value;

        // Get the selected processing time from the dropdown
        int selectedProcessingTimeIndex = processingTimeDropdown.value;
        float processingTime = selectedProcessingTimeIndex + 1;

        // Calculate spawn position based on the row and column
        float spawnX = spawnCount * spacing; // Position along the X-axis
        float spawnY = -4.5f;  // Fixed Y position at the bottom of the screen

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        // Instantiate the selected sprite as the process
        GameObject selectedSprite = Instantiate(processSprites[selectedSpriteIndex], spawnPosition, Quaternion.identity);

        // Add a label to show status, process number, and processing time
        GameObject statusText = new GameObject("StatusText");
        statusText.transform.SetParent(selectedSprite.transform); // Parent the text to the sprite

        // Add and configure the TextMesh component
        TextMesh textMesh = statusText.AddComponent<TextMesh>();
        textMesh.text = $"P{spawnCount + 1}\nWaiting\nBurst Time: {processingTime}s";
        textMesh.fontSize = 15; // Smaller font size
        textMesh.color = Color.white; // Set font color to white
        textMesh.anchor = TextAnchor.MiddleCenter; // Center align the text

        statusText.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);

        // Position the text above the sprite
        statusText.transform.localPosition = new Vector3(0, 1.2f, 0); // Adjust Y position for better spacing

        // Create a new process instance and add it to the list
        Process newProcess = new Process(selectedSprite, processingTime, currentTime);
        processes.Add(newProcess);

        // Increment spawn count for horizontal placement
        spawnCount++;
    }

    // Coroutine to move the CPU towards the process
    IEnumerator MoveCPUTowardsProcess(float targetX)
    {
        Debug.Log("Moving CPU towards X: " + targetX);  // Debug log to verify

        float startX = cpuSprite.transform.position.x;
        float elapsedTime = 0f;
        float duration = 1f;  // Increase duration to make it slower

        // Move the CPU from its current position to the process's position
        while (elapsedTime < duration)
        {
            float newX = Mathf.Lerp(startX, targetX, (elapsedTime / duration));
            cpuSprite.transform.position = new Vector3(newX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cpuSprite.transform.position = new Vector3(targetX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);  // Final position
    }


    // Coroutine to move the process smoothly
    IEnumerator MoveProcessSmoothly(GameObject process, float targetY)
    {
        float startY = process.transform.position.y;
        float elapsedTime = 0f;
        float duration = 1f;  // Duration of the movement (you can adjust this to control speed)

        // Move the process from its current position to the target Y position
        while (elapsedTime < duration)
        {
            process.transform.position = Vector3.Lerp(new Vector3(process.transform.position.x, startY, 0),
                                                       new Vector3(process.transform.position.x, targetY, 0),
                                                       (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the process ends up exactly at the target Y position
        process.transform.position = new Vector3(process.transform.position.x, targetY, 0);
    }



    // Start the FCFS simulation
    void StartFCFSSimulation()
    {
        if (!isProcessing && processes.Count > 0)
        {
            StartCoroutine(ProcessFCFS());
        }
    }

    // Coroutine to process tasks in FCFS order
    IEnumerator ProcessFCFS()
    {
        isProcessing = true;

        foreach (var process in processes)
        {
            Debug.Log("Moving CPU to process " + process.processObject.name);
            float cpuTargetX = process.processObject.transform.position.x;
            yield return StartCoroutine(MoveCPUTowardsProcess(cpuTargetX));

            // Highlight the process being executed
            SpriteRenderer spriteRenderer = process.processObject.GetComponent<SpriteRenderer>();
            TextMesh statusText = process.processObject.GetComponentInChildren<TextMesh>();

            if (spriteRenderer != null)
            {
                // spriteRenderer.color = Color.yellow; // Highlight in yellow
            }
            if (statusText != null)
            {
                statusText.text = $"P{spawnCount}\nExecuting\nTime: {process.processingTime}s";
                // statusText.color = Color.green; // Green for executing
            }

            // Move the process up to simulate execution
            float targetY = process.processObject.transform.position.y + 3f; // Move up by 3 units
            yield return StartCoroutine(MoveProcessSmoothly(process.processObject, targetY));

            // Simulate processing time
            Debug.Log($"Processing {process.processObject.name} for {process.processingTime} seconds...");
            yield return new WaitForSeconds(process.processingTime);

            // Reset the highlight and update status
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white; // Reset to default color
            }
            if (statusText != null)
            {
                statusText.text = $"P{spawnCount}\nCompleted\nTime: {process.processingTime}s";
                // statusText.color = Color.blue; // Blue for completed
            }

            // Calculate completion time, turnaround time, and waiting time
            process.completionTime = currentTime + process.processingTime;
            process.turnaroundTime = process.completionTime - process.arrivalTime;
            process.waitingTime = process.turnaroundTime - process.processingTime;

            currentTime += process.processingTime;
        }

        // Calculate and display averages
        DisplayResults();

        isProcessing = false;
        Debug.Log("FCFS Simulation Completed!");
    }



    // Display results (waiting times, turnaround times, averages)
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

    // Reset the simulation
    void ResetSimulation()
    {
        StopAllCoroutines();
        isProcessing = false;

        foreach (var process in processes)
        {
            Destroy(process.processObject);
        }
        processes.Clear();

        resultsText.text = "Process Results:\n";
        currentTime = 0f;
        spawnCount = 0;

        Debug.Log("Simulation Reset!");
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
