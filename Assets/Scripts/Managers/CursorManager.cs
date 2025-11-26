using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager IN;

    [SerializeField] private Vector3 mobileCursorOffset;

    public bool IsTargetModeActive { get; private set; }

    private static readonly int ANIM_STATE = Animator.StringToHash("state");

    private ECursorMode cursorMode = ECursorMode.None;

    private Animator animator;

    private Camera mainCamera;

    public Rigidbody2D LockedTarget { get; private set; }
    public Vector2 WorldPosition { get; private set; }

    public bool IsCursorOverUI { get; private set; }

    private Rigidbody2D selectedTarget;

    private bool shouldRefreshCursor = true;

    private Vector3 mousePos;
    private Vector3 initMousePos;

    public enum ECursorMode
    {
        Default,
        MissileNoTarget,
        MissileOverTarget,
        MissileLockTarget,
        None
    }

    private void Awake()
    {
        if (IN == null)
            IN = this;
        else
            DestroyImmediate(this.gameObject);

        this.animator = this.GetComponent<Animator>();

        this.mainCamera = Camera.main;

        SetTargetModeActive(false);
    }

    private void LateUpdate()
    {
        if (!this.IsTargetModeActive)
            return;

        this.mousePos = Input.mousePosition;

        if (InputManager.IsMissileButtonPressed)
        {
            this.mousePos += this.mobileCursorOffset;

            //make it move faster along X axis, so you don't need to move your finger as far
            var delta = this.mousePos - this.initMousePos;
            this.mousePos += new Vector3(delta.x * 2, delta.y, 0);

            var distance = Vector3.Distance(this.initMousePos, this.mousePos);
            this.IsCursorOverUI = distance < 120f;

            //print($"IsCursorOverUI = {this.IsCursorOverUI}   distance from start = {distance}   initPos = {this.initMousePos}   mousePos = {this.mousePos}");
        }

        this.transform.position = this.mousePos;

        this.WorldPosition = this.mainCamera.ScreenToWorldPoint(new Vector3(this.mousePos.x, this.mousePos.y, -this.mainCamera.transform.localPosition.z));

        var hit = Physics2D.Raycast(this.WorldPosition, Vector2.zero, 1000f, LayerMask.GetMask("Default", "Rocks", "Both Maps", "Enemy Ship", "Space Stations"));

        if (hit.collider != null)
        {
            var healthObj = HealthEntity.GetHealthEntity(hit.collider.gameObject);

            if (healthObj != null)
            {
                this.selectedTarget = healthObj.GetComponent<Rigidbody2D>();

                if (this.shouldRefreshCursor)
                {
                    if (this.cursorMode != ECursorMode.MissileLockTarget)
                        SetCursorMode(ECursorMode.MissileOverTarget);
                }
                else
                    SetCursorMode(ECursorMode.MissileOverTarget);

                return;
            }
            else
            {
                SetCursorMode(ECursorMode.MissileNoTarget);
            }
        }
        else
        {
            SetCursorMode(ECursorMode.MissileNoTarget);
        }

        SetCursorMode(ECursorMode.MissileNoTarget);
    }

    public void SetTargetModeActive(bool inIsActive)
    {
        //print($"SetTargetModeActive({inIsActive})");

        if (SpaceShip.PlayerShip != null && SpaceShip.PlayerShip.IsMissileHudButtonDisabled)
            inIsActive = false;

        this.IsTargetModeActive = inIsActive;

        Cursor.visible = !inIsActive;

        if (inIsActive)
            this.initMousePos = Input.mousePosition;

        if (!inIsActive)
            SetCursorMode(ECursorMode.Default);
    }

    public void SetCursorMode(ECursorMode inCursorMode)
    {
        if (inCursorMode != this.cursorMode)
        {
            this.cursorMode = inCursorMode;

            if (inCursorMode != ECursorMode.MissileOverTarget && inCursorMode != ECursorMode.MissileLockTarget)
            {
                this.LockedTarget = null;
                this.selectedTarget = null;
            }

            this.animator.SetInteger(ANIM_STATE, (int)inCursorMode);
        }
    }

    /// <summary>
    /// called from Animation timeline CursorTarget-OverTarget when it finishes
    /// </summary>
    public void SetTargetLocked()
    {
        SetCursorMode(ECursorMode.MissileLockTarget);

        this.LockedTarget = this.selectedTarget;
    }

    public void ClearLockCursor()
    {
        if (this.cursorMode == ECursorMode.MissileLockTarget)
        {
            this.shouldRefreshCursor = false;

            Invoke(nameof(EnableCursorRefresh), .1f);
        }
    }

    private void EnableCursorRefresh()
    {
        this.shouldRefreshCursor = true;
    }
}