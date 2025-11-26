using UnityEngine;

public class AngleDetect : MonoBehaviour
{
    public GameObject target;
    [SerializeField] private LineRenderer lineRenderer;

    private void Update()
    {
        var vectorA = new Vector2(this.transform.position.x, this.transform.position.y);
        var vectorB = new Vector2(this.target.transform.position.x, this.target.transform.position.y);
        var subtract = vectorA - vectorB;
        var angle = -1 * Mathf.Atan2(subtract.x, -subtract.y) * Mathf.Rad2Deg;

        this.lineRenderer.SetPosition(0, this.transform.position);
        this.lineRenderer.SetPosition(1, this.target.transform.position);

        var signedAngle = Vector2.SignedAngle(vectorA, vectorB);

        //Debug.Log($"subtract = {subtract}    signedAngle = {signedAngle}   atan2.angle = {angle}");

        Vector3 up = this.transform.TransformDirection(Vector3.up);
        Vector3 right = this.transform.TransformDirection(Vector3.right);
        Vector3 toOther = this.target.transform.position - transform.position;

        var dotUp = Vector3.Dot(up.normalized, toOther.normalized);//         > 0 = in front     < 0 = behind
        var dotRight = Vector3.Dot(right.normalized, toOther.normalized);//   > 0 = right        < 0 = left

        Debug.Log($"dotUp = {dotUp}   dotRight = {dotRight}  angle = {angle}");
    }
}