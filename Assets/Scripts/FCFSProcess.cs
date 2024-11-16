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

    [Header("UI Elements")]
    public Dropdown spriteDropdown;       // Dropdown to select the sprite
    public Dropdown processingTimeDropdown; // Dropdown for selecting processing time
    public Button addButton;              // Button to add a process
    public Button startSimulationButton;  // Button to start the FCFS simulation
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
    }

    // Populate the sprite dropdown with sprite names
    void PopulateSpriteDropdown()
    {
        spriteDropdown.ClearOptions();
        List<string> options = new List<string>();

        for (int i = 0; i < processSprites.Length; i++)
        {
            options.Add("Sprite " + (i + 1)); // Add custom names as options
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
        // Get the selected sprite index from the dropdown
        int selectedSpriteIndex = spriteDropdown.value;

        // Get the selected processing time from the dropdown
        int selectedProcessingTimeIndex = processingTimeDropdown.value;
        float processingTime = selectedProcessingTimeIndex + 1; // Dropdown indices start at 0 (1 second = index 0)

        // Calculate spawn position based on the row and column
        float spawnX = spawnCount * spacing; // Position along the X-axis
        float spawnY = -4.5f;  // Fixed Y position at the bottom of the screen

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);

        // Instantiate the selected sprite as the process
        GameObject selectedSprite = Instantiate(processSprites[selectedSpriteIndex], spawnPosition, Quaternion.identity);

        // Create a new process instance and add it to the list
        Process newProcess = new Process(selectedSprite, processingTime, currentTime);
        processes.Add(newProcess);

        // Increment spawn count for horizontal placement
        spawnCount++;
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
            // Move the process up to simulate execution
            float targetY = process.processObject.transform.position.y + 3f; // Move up by 3 units
            while (process.processObject.transform.position.y < targetY)
            {
                process.processObject.transform.position += Vector3.up * moveSpeed * Time.deltaTime;
                yield return null;
            }

            // Simulate processing time
            Debug.Log($"Processing {process.processObject.name} for {process.processingTime} seconds...");
            yield return new WaitForSeconds(process.processingTime);

            // Calculate completion time, turnaround time, and waiting time
            process.completionTime = currentTime + process.processingTime;
            process.turnaroundTime = process.completionTime - process.arrivalTime;
            process.waitingTime = process.turnaroundTime - process.processingTime;

            
            // Increment simulation time
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
