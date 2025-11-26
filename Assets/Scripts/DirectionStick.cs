using TMPro;
using UnityEngine;

public class DirectionStick : StickController 
{
	[SerializeField] private TextMeshProUGUI[] labels;

	private readonly string[] labelPrefixes = { "X: ", "Y: " };

	protected override void Start() 
	{
		this.AxisValues = new float[2];

		base.Start();
	}

	protected override void OnSnapBackComplete()
	{
		for( int i = 0; i < this.AxisValues.Length; ++i )
		{
			this.AxisValues[i] = 0;

			if( i < labels.Length )
				labels[i].text = $"{this.labelPrefixes[i]}{this.AxisValues[i] * 100:##0}%";
		}

		this.OnSnapBack?.Invoke();
	}

	protected override void CalculateAxisValues( float inXPos, float inYPos, float inRadius, float inAngleDeg )
	{
		this.AxisValues[0] = inXPos / this.maxDragRadius;
		this.AxisValues[1] = inYPos / this.maxDragRadius;

		for( int i = 0; i < this.AxisValues.Length; ++i )
		{
			if( i < this.labels.Length )
				this.labels[i].text = this.labelPrefixes[i] + ( this.AxisValues[i] * 100 ).ToString( "##0" ) + "%";
		}
	}
}
