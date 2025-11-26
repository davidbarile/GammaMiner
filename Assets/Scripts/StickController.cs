using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Lean.Touch;

public class StickController : MonoBehaviour
{
	public bool ShouldSnapToCenter;
	public float[] AxisValues { get; protected set; }
	public bool IsPressed { get; protected set; }

	public Action<float, float, float, float> OnStickMove;
	public Action OnSnapBack;

	[SerializeField] protected float maxDragRadius = 0;

	[SerializeField] private float snapBackTweenDuration = .1f;
	[SerializeField] private RectTransform baseRectTrans;
	[SerializeField] private Image stick;
	[SerializeField] private Image snapActiveImage;

	[SerializeField] private Vector2 buffer;

	private Vector3 startPos;
	private bool isSnappingBack;
	private LeanFinger activeFinger;
	private Tweener snapBackTween;

	protected virtual void Start()
	{
		if (this.snapActiveImage) this.snapActiveImage.color = this.ShouldSnapToCenter ? Color.white : Color.black;

		if (this.maxDragRadius <= 0)
			this.maxDragRadius = this.baseRectTrans.rect.width * .5f;
	}

	public void HandlePress()
	{
		if (this.isSnappingBack)
			this.snapBackTween.Complete();

#if UNITY_EDITOR
		this.startPos = Input.mousePosition - this.stick.transform.localPosition;
#else
		foreach (var finger in LeanTouch.Fingers)
		{
			if(finger.StartScreenPosition.x < 400 && finger.StartScreenPosition.y < 400)
			{
				this.activeFinger = finger;
				this.startPos = new Vector3( finger.StartScreenPosition.x, finger.StartScreenPosition.y, 0) - this.stick.transform.localPosition;
			}
		}
#endif

		this.IsPressed = true;
	}

	public void HandleRelease()
	{
		if (this.isSnappingBack)
			return;

		if (this.ShouldSnapToCenter)
		{
			this.isSnappingBack = true;

			this.snapBackTween = this.stick.transform.DOMove(this.baseRectTrans.transform.position, this.snapBackTweenDuration).SetEase(Ease.InOutSine).OnComplete(new TweenCallback(() =>
			{
				this.isSnappingBack = false;
				this.IsPressed = false;
				this.activeFinger = null;

				OnSnapBackComplete();

				this.snapBackTween = null;
			})).SetUpdate(true);
		}
		else
		{
			this.IsPressed = false;
		};
	}

	public void ToggleSnapToCenter()
	{
		this.ShouldSnapToCenter = !this.ShouldSnapToCenter;

		if (this.snapActiveImage) this.snapActiveImage.color = this.ShouldSnapToCenter ? Color.white : Color.black;

		if (this.ShouldSnapToCenter)
			HandleRelease();
	}

	private void Update()
	{
		if (this.IsPressed)
		{
			Vector3 pos = Vector3.zero;

#if UNITY_EDITOR
			pos = Input.mousePosition - this.startPos;
#else
			if(this.activeFinger != null)
				pos = new Vector3(this.activeFinger.ScreenPosition.x, this.activeFinger.ScreenPosition.y, 0) - this.startPos;
#endif
			pos *= .8f;//why?   who knows...

			if (this.isSnappingBack)
				pos = this.stick.transform.localPosition;

			float radius = Mathf.Sqrt(Mathf.Pow(pos.x, 2) + Mathf.Pow(pos.y, 2));
			float angleRad = Mathf.Atan2(pos.y, pos.x);
			float angleDeg = Mathf.Rad2Deg * angleRad;

			//Debug.Log($"angleDeg = {angleDeg}  euler Z = {this.transform.rotation.eulerAngles.z}    calc = {angleDeg -= this.transform.rotation.eulerAngles.z}");
			//angleDeg -= this.transform.localRotation.eulerAngles.z;

			if (radius >= this.maxDragRadius)
			{
				pos.x = Mathf.Cos(angleRad) * this.maxDragRadius;
				pos.y = Mathf.Sin(angleRad) * this.maxDragRadius;
				//this.stick.transform.localPosition = pos;
			}

			CalculateAxisValues(pos.x, pos.y, radius, angleDeg);

			this.stick.transform.localPosition = pos;

			var xValue = Mathf.Abs(this.AxisValues[0]) < this.buffer.x ? 0 : this.AxisValues[0];
			var yValue = Mathf.Abs(this.AxisValues[1]) < this.buffer.y ? 0 : this.AxisValues[1];

			this.OnStickMove?.Invoke(xValue, yValue, this.AxisValues[0], this.AxisValues[1]);
		}
	}

	protected virtual void OnSnapBackComplete() { }
	protected virtual void CalculateAxisValues(float xPos, float yPos, float inRadius, float inAngleDeg) { }
}
