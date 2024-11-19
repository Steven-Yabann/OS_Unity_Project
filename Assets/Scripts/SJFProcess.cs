using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SJFProcess : MonoBehaviour
{
    [Header("Process Settings")]
    public GameObject[] processSprites;   // Array of available sprites
    public float spacing = 2f;            // Horizontal spacing between processes
    public float moveSpeed = 2f;          // Speed at which processes move up

    [Header("UI Elements")]
    public Dropdown spriteDropdown;       // Dropdown to select the sprite
    public Dropdown processingTimeDropdown; // Dropdown for selecting processing time
    public Button addButton;              // Button to add a process
    public Button startSimulationButton;  // Button to start the SJF simulation
    public Button resetButton;            // Button to reset the simulation
    public Text resultsText;              // UI Text to display results

    [Header("CPU Elements")]
    public GameObject cpuSprite;          // CPU sprite to move
    public float cpuMoveSpeed = 2f;       // Speed at which the CPU moves

    private List<Process> processes = new List<Process>(); // List to store processes
    private List<Process> completedProcesses = new List<Process>(); // Completed processes
    private bool isProcessing = false;    // Flag to indicate if a process is being executed
    private float currentTime = 0f;       // Tracks simulation time
    private int spawnCount = 0;           // Tracks position of spawned processes in a row

    void Start()
    {
        // Populate dropdowns with options
        PopulateSpriteDropdown();
        PopulateProcessingTimeDropdown();

        // Set up the Add button to call AddProcess when clicked
        addButton.onClick.AddListener(AddProcess);

        // Set up the Start Simulation button to begin SJF processing
        startSimulationButton.onClick.AddListener(StartSJFSimulation);

        // Set up the Reset button to reset the simulation
        resetButton.onClick.AddListener(ResetSimulation);
    }

    // Populate the sprite dropdown with sprite names
    void PopulateSpriteDropdown()
    {
        spriteDropdown.ClearOptions(); // Clear any existing options
        List<string> options = new List<string>();

        foreach (GameObject sprite in processSprites)
        {
            options.Add(sprite.name);
        }

        spriteDropdown.AddOptions(options); // Add the list of sprite names to the dropdown
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
        int selectedSpriteIndex = spriteDropdown.value;
        int selectedProcessingTimeIndex = processingTimeDropdown.value;
        float processingTime = selectedProcessingTimeIndex + 1;

        float spawnX = processes.Count * spacing;
        float spawnY = -4.5f;

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        GameObject selectedSprite = Instantiate(processSprites[selectedSpriteIndex], spawnPosition, Quaternion.identity);

        // Add a label to show status, process number, and processing time
        GameObject statusText = new GameObject("StatusText");
        statusText.transform.SetParent(selectedSprite.transform); // Parent the text to the sprite

        // Add and configure the TextMesh component
        TextMesh textMesh = statusText.AddComponent<TextMesh>();
        textMesh.text = $"P{processes.Count + 1}\nWaiting\nBurst Time: {processingTime}s";
        textMesh.fontSize = 15; // Smaller font size
        textMesh.color = Color.white; // Set font color to white
        textMesh.anchor = TextAnchor.MiddleCenter; // Center align the text

        statusText.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
        statusText.transform.localPosition = new Vector3(0, 1.2f, 0); // Adjust Y position for better spacing

        Process newProcess = new Process(selectedSprite, processingTime, currentTime);
        processes.Add(newProcess);
    }

    // Start the SJF simulation
    void StartSJFSimulation()
    {
        if (!isProcessing && processes.Count > 0)
        {
            StartCoroutine(ProcessSJF());
        }
    }

    // Coroutine to process tasks in SJF order
    IEnumerator ProcessSJF()
    {
        isProcessing = true;

        while (processes.Count > 0)
        {
            // Find the process with the shortest processing time
            Process shortestProcess = FindShortestProcess();

            // Move the CPU to the current process
            float cpuTargetX = shortestProcess.processObject.transform.position.x;
            yield return StartCoroutine(MoveCPUTowardsProcess(cpuTargetX));

            // Change the status text to 'Executing' and set color to green
            TextMesh statusText = shortestProcess.processObject.GetComponentInChildren<TextMesh>();
            if (statusText != null)
            {
                statusText.text = $"P{processes.Count}\nExecuting\nTime: {shortestProcess.processingTime}s";
                statusText.color = Color.green; // Green for executing
            }

            // Move the process up to simulate execution
            float targetY = shortestProcess.processObject.transform.position.y + 3f;
            while (shortestProcess.processObject.transform.position.y < targetY)
            {
                shortestProcess.processObject.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                yield return null;
            }

            // Simulate processing time
            Debug.Log($"Processing {shortestProcess.processObject.name}: Start Time = {currentTime}");
            yield return new WaitForSeconds(shortestProcess.processingTime);

            // Reset the status and update it to 'Completed' and set color to blue
            if (statusText != null)
            {
                statusText.text = $"P{processes.Count}\nCompleted\nTime: {shortestProcess.processingTime}s";
                statusText.color = Color.blue; // Blue for completed
            }

            // Calculate completion time, turnaround time, and waiting time
            shortestProcess.completionTime = currentTime + shortestProcess.processingTime;
            shortestProcess.turnaroundTime = shortestProcess.completionTime - shortestProcess.arrivalTime;
            shortestProcess.waitingTime = shortestProcess.turnaroundTime - shortestProcess.processingTime;

            currentTime += shortestProcess.processingTime;

            // Remove from processes list and add to completed list
            processes.Remove(shortestProcess);
            completedProcesses.Add(shortestProcess);
        }

        DisplayResults();

        isProcessing = false;
    }

    // Find the process with the shortest processing time
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

    // Coroutine to move the CPU towards the process
    IEnumerator MoveCPUTowardsProcess(float targetX)
    {
        float startX = cpuSprite.transform.position.x;
        float elapsedTime = 0f;
        float duration = 3f;  // Adjust duration to control CPU speed

        // Move the CPU from its current position to the process's position
        while (elapsedTime < duration)
        {
            float newX = Mathf.Lerp(startX, targetX, (elapsedTime / duration));
            cpuSprite.transform.position = new Vector3(newX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);
            elapsedTime += Time.deltaTime * cpuMoveSpeed; // Use cpuMoveSpeed to adjust speed
            yield return null;
        }

        // Ensure the CPU reaches exactly the target position
        cpuSprite.transform.position = new Vector3(targetX, cpuSprite.transform.position.y, cpuSprite.transform.position.z);
    }

    // Display results (waiting times, turnaround times, averages)
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

        // Check to avoid division by zero
        if (completedProcesses.Count > 0)
        {
            float avgWaitingTime = totalWaitingTime / completedProcesses.Count;
            float avgTurnaroundTime = totalTurnaroundTime / completedProcesses.Count;

            resultsText.text += $"\nAverage Waiting Time: {avgWaitingTime:F2}\n";
            resultsText.text += $"Average Turnaround Time: {avgTurnaroundTime:F2}";
        }
        else
        {
            resultsText.text += "\nNo processes to calculate averages.";
        }
    }

    // Reset the simulation
    void ResetSimulation()
    {
        StopAllCoroutines(); // Stop all coroutines to halt any running processes
        isProcessing = false;

        // Reset CPU position
        cpuSprite.transform.position = new Vector3(-6f, 0f, 0f);

        // Destroy all instantiated processes
        foreach (var process in processes)
        {
            Destroy(process.processObject);
        }

        // Clear the lists and reset simulation parameters
        processes.Clear();
        completedProcesses.Clear();
        currentTime = 0f;
        spawnCount = 0;

        // Reset results text
        resultsText.text = "Process Results:\n";

        Debug.Log("Simulation Reset!");
    }
}
