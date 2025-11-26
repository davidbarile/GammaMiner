using UnityEngine;

public class Cockpit : MonoBehaviour
{
    public static Cockpit IN;

    private readonly int ANIM_STATE = Animator.StringToHash("state");

    [SerializeField] private Animator animator;

    private enum ECockpitState
    {
        Hidden,
        Show,
        Hide,
        Showing
    }

    private ECockpitState cockpitState = ECockpitState.Hidden;

    //called by LeanFingerSwipe on Managers object
    public void HandleSwipe(Vector2 inDelta)
    {
        if (Input.mousePosition.x < Screen.width * .5f) return;
        if (Input.mousePosition.y < Screen.height * .4f) return;

        SetCockpitVisible(inDelta.y < 0);
    }

    public void SetCockpitVisible(bool inIsVisible)
    {
        var state = inIsVisible ? ECockpitState.Show : ECockpitState.Hide;

        var isChanged = (inIsVisible && this.cockpitState == ECockpitState.Hidden || this.cockpitState == ECockpitState.Hide) ||
            (!inIsVisible && this.cockpitState == ECockpitState.Showing || this.cockpitState == ECockpitState.Show);

        this.cockpitState = state;

        if (isChanged)
        {
            this.animator.SetInteger(ANIM_STATE, (int)state);

            if (state == ECockpitState.Hide)
            {
                if (HUD.IN.IsComsPanelOpen)
                    HUD.IN.HideComsMonitor();

                if (HUD.IN.IsStatsPanelOpen)
                    HUD.IN.HideStatsMonitor();
            }
        }
    }

    public void ToggleVisibility()
    {
        if (this.cockpitState == ECockpitState.Hidden || this.cockpitState == ECockpitState.Hide)
            SetCockpitVisible(true);
        else
            SetCockpitVisible(false);
    }
}
