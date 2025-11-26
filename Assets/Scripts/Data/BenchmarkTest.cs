using UnityEngine;

public class BenchmarkTest : MonoBehaviour
{
    private Transform cachedTransform;

    private void Start()
    {
        this.cachedTransform = this.transform;
    }
    
    public void PerformTest()
    {
        //2ms
        //Debug.Log("Benchmark Test Performed");//29ms
        //this.cachedTransform.localScale = Vector3.one;//20ms
        //this.GetComponent<Transform>().localScale = Vector3.one;//84ms
    }
}