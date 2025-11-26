using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 RotationAmount => this.rotationAmount;
    [SerializeField] private Vector3 rotationAmount;

    [Tooltip("If true, the object will not rotate and will always face the LockRotation amount.")]
    [SerializeField] private bool lockRotation;

    private void Update()
    {
        if (this.lockRotation)
        {
            this.transform.eulerAngles = this.rotationAmount;
            return;
        }

        this.transform.Rotate(this.rotationAmount * Time.deltaTime);
    }

    public void SetRotationAmount(float inX, float inY, float inZ)
    {
        this.rotationAmount = new Vector3(inX, inY, inZ);
        this.enabled = this.rotationAmount != Vector3.zero;
    }

    public void SetRotationAmount2D(float inZ)
    {
        this.rotationAmount = new Vector3(0, 0, inZ);
        this.enabled = this.rotationAmount != Vector3.zero;
    }
}