using UnityEngine;

public class Thruster : ShipComponent
{
    [SerializeField] private Animator thrustAnimator;

    public enum ThrustMode
    {
        Off,
        Start,
        On,
        End
    }

    public void SetAnimatorState(ThrustMode inThrustMode)
    {
        int mode = (int)inThrustMode;
        //Debug.Log("SetAnimatorState("+ mode + ")");
        this.thrustAnimator.SetInteger("Mode", mode);
    }
}