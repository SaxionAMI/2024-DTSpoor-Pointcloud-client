using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.XR;


//TODO:
//  Point number?                                           
//  How often does performance drop below 90?               
//	load times                                              
//	track the scaling of different mehtods                  
//	Separate moving camera test for culling                 



public class PerformanceTestRunner : MonoBehaviour
{
    // Test parameters
    [Header("Test parameters")]
    [SerializeField] int iterationCount = 1;
    [SerializeField] int iterationCurrent = 0;

    [SerializeField] float testDuration = 30.0f; // Extended duration for the test
    [SerializeField] float elapsedTime = 0f;

    [Tooltip("Arbitary text message")]
    [SerializeField] bool bypassBatteryCheck = false;

    // Performance check
    [Header("Performance check")]
    [SerializeField] bool isTestingPerformance = false;

    // Performance metrics
    [Header("Performance metrics")]
    [SerializeField] int frameCount = 0;
    [SerializeField] float minFps = float.MaxValue;
    [SerializeField] float maxFps = 0f;
    [SerializeField] float totalMemoryUsage = 0f;

    // Choose test type
    [Header("Addtional test scenario")]
    [SerializeField] bool setStressTest = false;
    [SerializeField] bool setMovingTest = false;
    [SerializeField] bool setCullingTest = false;
    [SerializeField] bool setFoveatedRenderingTest = false;


    // Misc.
    [Header("Misc")]
    [SerializeField] GameObject pointCloudExample;
    [SerializeField] bool logToCSV = true;
    [SerializeField] bool logToText = true;
    private float fps = 0;
    private List<float> fpsMeasurement = new List<float>();
    [SerializeField] Camera mainCam;
    //private bool testComplete = false;


    static string generalFilePath = "C:/Users/mikus/Desktop/Unity/UnityProjects/FPTest/Assets/Profiler/";

    string logFilePath = generalFilePath + "/performance_log.txt";
    string csvFilePath = generalFilePath + "/performance_csv.csv";
    string stressCSVFilePath = generalFilePath + "/stress_csv.csv";
    string cullingCSVFilePath = generalFilePath + "/culling.csv.csv";
    string foveatedCSVFilePath = generalFilePath + "/foveated.csv.csv";

    private void Start()
    {
        //logFilePath = Application.persistentDataPath + "/performance_log.txt";
        // Starting test

        if (batteryStatus()) startPerformanceTest(testDuration);
        LogHeader();
        if ((iterationCurrent == iterationCount + 1) || !isTestingPerformance)
        {
            Debug.Log($"Performance testing completed.");
        }
    }

    void Update()
    {
        float currentFps = 1 / Time.deltaTime;
        fps = currentFps;
        if (isTestingPerformance)
        {
            elapsedTime += Time.deltaTime;
            frameCount++;
            currentFps = 1 / Time.deltaTime;
            fpsMeasurement.Add(currentFps);    //Casting integer is not accurate TODO: Improve
            minFps = Mathf.Min(minFps, currentFps);
            maxFps = Mathf.Max(maxFps, currentFps);

            if (elapsedTime >= testDuration)
            {
                endPerformanceTest();
                if (iterationCurrent < iterationCount)
                {
                    StartCoroutine(WaitAndRestartTest(2.0f));
                }
            }
        }
    }

    public void startPerformanceTest(float duration)
    {
        if (isTestingPerformance)
        {
            return;
        }
        if (iterationCount != 0)
        {
            isTestingPerformance = true;
        }
        else
        {
            isTestingPerformance = false;
            Debug.Log("Iteration Count set to 0, skipping initial test");
        }

        clearState(duration);

        Debug.Log($"Performance test {iterationCurrent + 1} started.");
        // Reset memory usage tracker
        totalMemoryUsage = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // Bytes to Megabytes

        // TODO: Start test scenario as a coroutine
        if (setStressTest || setMovingTest || setCullingTest)
        {
           StartCoroutine(TestScenario());
        }
    }

    private void clearState(float duration)
    {
        testDuration = duration;
        elapsedTime = 0f;
        frameCount = 0;
        minFps = float.MaxValue;
        maxFps = 0f;
        fpsMeasurement.Clear();
    }

    private float CalculateOnePercentLow(List<float> fpsList)
    {
        int numValuesToConsider = Mathf.Max(1, (int)(fpsList.Count * 0.01f));
        var sortedFps = fpsList.OrderBy(x => x).ToList();
        return sortedFps.Take(numValuesToConsider).Average();
    }

    private void endPerformanceTest()
    {
        isTestingPerformance = false;
        float averageFps = frameCount / elapsedTime;
        float endMemoryUsage = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // Bytes to Megabytes
        float memoryUsageIncrease = endMemoryUsage - totalMemoryUsage;
        float onePercentLows = CalculateOnePercentLow(fpsMeasurement);
        float verticeCount = 0;

        /*  Counting Vertices this way is too expensive >>> Find new way
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            // Get the MeshFilter component, if it exists
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                // Add the vertex count of the mesh to the total
                verticeCount += meshFilter.sharedMesh.vertexCount;
            }
        }
        */

        Debug.Log($"<color=green>Performance test {iterationCurrent + 1} completed. Average FPS: {averageFps}, Min FPS: {minFps}, Max FPS: {maxFps}, Memory Usage Increase: {memoryUsageIncrease} MB</color>");
        if (logToText)
        {
            LogResultsToFile(averageFps, minFps, maxFps, memoryUsageIncrease, verticeCount);

        }
        if (logToCSV)
        {
            LogResultsToCSV(averageFps, minFps, maxFps, memoryUsageIncrease, verticeCount, onePercentLows);
        }
    }
    
    private void LogHeader()
    {
        string logHeader = $"Test Date: {System.DateTime.Now}\n" +
                     $"Device name: {SystemInfo.deviceName} \n" +
                     $"Device GPU: {SystemInfo.graphicsDeviceName} \n" +
                     $"Device CPU: {SystemInfo.processorType} \n" +
                     $"Device OS: {SystemInfo.operatingSystem} \n" +
                     $"Battery status: {SystemInfo.batteryStatus} \n" +
                     $"\n";

        File.AppendAllText(logFilePath, logHeader);
    }

    private void LogResultsToFile(float averageFps, float minFps, float maxFps, float memoryUsageIncrease, float verticeCount)
    {
        // Write data to file
        string log = 
                     $"Average FPS: {averageFps}\n" +
                     $"Min FPS: {minFps}\n" +
                     $"Max FPS: {maxFps}\n" +
                     $"Memory Usage Increase: {memoryUsageIncrease} MB\n" +
                     $"Number of vertices: {verticeCount}  \n" +
                     $"Test duration: {testDuration} seconds \n" +
                     $"Test iteration: {iterationCurrent + 1} out of {iterationCount} \n" + 

                     $"---------------------------------\n";
        File.AppendAllText(logFilePath, log);

        iterationCurrent++;
    }

    private void LogResultsToCSV(float averageFps, float minFps, float maxFps, float memoryUsageIncrease, float verticeCount, float onePercentLows)
    {
        StringBuilder csvFile = new StringBuilder();
        if (!File.Exists(csvFilePath))
        {
            csvFile.AppendLine("Test Date, Average FPS, Min FPS, Max FPS, 1% lows, Memory Usage Increase, Number of Vertices, Test duration, Test iterations");
        }
        csvFile.AppendLine($"{System.DateTime.Now}, {averageFps}," +
                           $" {minFps}, {maxFps}, {onePercentLows}, {memoryUsageIncrease}," +
                           $" {verticeCount}, {testDuration}, {iterationCurrent} ");

        File.AppendAllText(csvFilePath, csvFile.ToString());
        Debug.Log("Data logged to csv");
    }

    IEnumerator WaitAndRestartTest(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        startPerformanceTest(testDuration);
    }

    IEnumerator TestScenario()
    {
        while (isTestingPerformance) 
        {
            yield return new WaitForSeconds(1); // Wait for the scenario to stabilize

        }
        yield return new WaitForSeconds(1); // Wait for the scenario to stabilize

        Debug.Log("Beginning additional testing scenarios");
        //pointCloudExample.SetActive(false);

        // TODO: Implement setup 
        if (setStressTest)
        {
            yield return stressTest();
        }

        if (setMovingTest)
        {
            // TODO: Implement Moving test
        }

        if (setCullingTest)
        {
            yield return cullingTest();
        }
    }
    
    IEnumerator stressTest()
    {
        yield return new WaitForSeconds(1); // Wait for the scenario to stabilize

        Debug.Log("Beginning Stress Test");
        int i = 0;
        yield return new WaitForSeconds(2);
        ArrayList clones = new ArrayList();
        GameObject clone = null;
        float memoryUsageIncrease;
        float verticeCount = 0;

        while (fps > 90f)
        {
            yield return new WaitForSeconds(1);
            // Check number of verts
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                // Get the MeshFilter component, if it exists
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Add the vertex count of the mesh to the total
                    verticeCount += meshFilter.sharedMesh.vertexCount;
                }
            }

            clone = Instantiate(pointCloudExample);
            clones.Add(clone);
            i++;
            Debug.Log($"Rendering #{i} with framerate {fps}");

            float endMemoryUsage = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // Bytes to Megabytes
            memoryUsageIncrease = endMemoryUsage - totalMemoryUsage;

            LogStressResultsToFile(fps, i, memoryUsageIncrease, verticeCount);
            LogStressResultsToCSV(fps, i, memoryUsageIncrease, verticeCount);

        }
        Debug.Log($"Ran below 90 frames per second at #{i}, ending stress test.");
        foreach (object obj in clones)
        {
            Destroy((GameObject)obj, 1.0f);
        }
    }
    
    private void LogStressResultsToFile(float fps, int iteration, float memoryUsageIncrease, float verticeCount)
    {
        if (logToText)
        {
            // Write data to file
            string log = $"Stress Test at: {System.DateTime.Now}, iteration {iteration} \n" +
                         $"FPS at the time: {fps}\n" +
                         $"Memory Usage Increase: {memoryUsageIncrease} MB\n" +
                         $"Vertice Count: {verticeCount} \n" +
                         $"---------------------------------\n";
            File.AppendAllText(logFilePath, log);
        }

    }
    
    private void LogStressResultsToCSV(float fps, int iteration, float memoryUsageIncrease, float verticeCount)
    {
        if (logToCSV)
        {
            StringBuilder csvFile = new StringBuilder();
            if (!File.Exists(stressCSVFilePath))
            {
                csvFile.AppendLine("Test Date, FPS, Memory Usage Increase, Number of Vertices, Test iteration");
            }
            csvFile.AppendLine($"{System.DateTime.Now}, {fps}," +
                               $" {memoryUsageIncrease}, {verticeCount}, {iteration}");

            File.AppendAllText(stressCSVFilePath, csvFile.ToString());
            Debug.Log("Stress data logged to csv");
        }

    }
    
    IEnumerator cullingTest()
    {
        yield return new WaitForSeconds(1); // Wait for the scenario to stabilize

       // mainCam.GetComponent<CameraOrbit>().SetOrbit(false);
       // mainCam.GetComponent<CameraOrbit>().SetPosition(new Vector3(-300, 0, -300));

        Debug.Log("Beginning Culling Test");
        int i = 0;
        yield return new WaitForSeconds(2);
        ArrayList clones = new ArrayList();
        GameObject clone = null;
        float memoryUsageIncrease;
        float verticeCount = 0;

        while (fps > 90f)
        {
            yield return new WaitForSeconds(1);
            // Check number of verts
            foreach (GameObject obj in FindObjectsOfType<GameObject>())
            {
                // Get the MeshFilter component, if it exists
                MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    // Add the vertex count of the mesh to the total
                    verticeCount += meshFilter.sharedMesh.vertexCount;
                }
            }

            clone = Instantiate(pointCloudExample);
            clones.Add(clone);
            i++;
            Debug.Log($"Rendering #{i} with framerate {fps}");

            float endMemoryUsage = Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024; // Bytes to Megabytes
            memoryUsageIncrease = endMemoryUsage - totalMemoryUsage;

            LogCullingResultsToFile(fps, i, memoryUsageIncrease, verticeCount);
            LogCullingResultsToCSV(fps, i, memoryUsageIncrease, verticeCount);
        }
        Debug.Log($"Ran below 90 frames per second at #{i}, ending stress test.");
        foreach (object obj in clones)
        {
            Destroy((GameObject)obj, 1.0f);
        }
    }
    
    private void LogCullingResultsToFile(float fps, int iteration, float memoryUsageIncrease, float verticeCount)
    {
        if (logToText)
        {
            // Write data to file
            string log = $"Culling Test at: {System.DateTime.Now}, iteration {iteration} \n" +
                         $"FPS at the time: {fps}\n" +
                         $"Memory Usage Increase: {memoryUsageIncrease} MB\n" +
                         $"Vertice Count: {verticeCount} \n" +
                         $"---------------------------------\n";
            File.AppendAllText(logFilePath, log);
        }
    }
    
    private void LogCullingResultsToCSV(float fps, int iteration, float memoryUsageIncrease, float verticeCount)
    {
        if (logToCSV)
        {
            StringBuilder csvFile = new StringBuilder();
            if (!File.Exists(cullingCSVFilePath))
            {
                csvFile.AppendLine("Test Date, FPS, Memory Usage Increase, Number of Vertices, Test iteration");
            }
            csvFile.AppendLine($"{System.DateTime.Now}, {fps}," +
                               $" {memoryUsageIncrease}, {verticeCount}, {iteration}");

            File.AppendAllText(cullingCSVFilePath, csvFile.ToString());
            Debug.Log("Culling data logged to csv");
        }
    }
    
    private bool batteryStatus()
    {
        if (bypassBatteryCheck)
        {
            Debug.Log("<color=red>Warning: Battery bypass is selected, proceeding with test regardless of battery state.</color>");
            return true;
        }
        else
        {
            if (SystemInfo.batteryStatus.Equals(BatteryStatus.Discharging))
            {
                Debug.Log("<color=red>Warning: Laptop is unplugged, this may affect performance. Cancelling test.</color>");
                return false;
            }
            else
            {
                Debug.Log("<color=green>Battery check success, proceeding with test.</color>");
                return true;
            }
        }
    }
}