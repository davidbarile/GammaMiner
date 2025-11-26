using System.Diagnostics;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BenchmarkTest))]
public class Benchmarker : MonoBehaviour
{
    [Range(1, 1000000), SerializeField] private int iterations = 1000;

    [SerializeField, ReadOnly] private string elapsedMilliseconds;

    private BenchmarkTest benchmarkTest;

    private void Start()
    {
        this.benchmarkTest = GetComponent<BenchmarkTest>();
        this.elapsedMilliseconds = string.Empty;
    }

    [Button("Run Benchmark", ButtonSizes.Large)]
    private void RunBenchmark()
    {
        if (!Application.isPlaying) return;
            
        Stopwatch stopwatch = Stopwatch.StartNew();
        stopwatch.Start();

        for (int i = 0; i < this.iterations; i++)
        {
            this.benchmarkTest.PerformTest();
        }

        stopwatch.Stop();

        this.elapsedMilliseconds = $"{stopwatch.ElapsedMilliseconds} ms";

        //UnityEngine.Debug.Log($"Benchmark completed in: {stopwatch.ElapsedMilliseconds} ms");
    }
}