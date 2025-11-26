using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.Cinemachine;

public class InputManager : MonoBehaviour
{
    public static InputManager IN;

    public static bool IsInputBlocked;

    public static bool IsMissileButtonPressed;

    public static Action OnShootRailgunsHold;

    public static Action OnMissileTargetingPress;
    public static Action OnMissileTargetingRelease;
    public static Action OnShootMissilePress;

    public static Action OnLaserHold;
    public static Action OnLaserRelease;

    public static Action OnDashPress;
    public static Action OnDashRelease;

    public static Action<float> OnThrustPress;
    public static Action<float> OnBrakePress;
    public static Action OnNoThrust;

    public static Action<float> OnTurnLeftPress;
    public static Action<float> OnTurnRightPress;
    public static Action OnNoTurnPress;

    public static Action OnShieldsHold;
    public static Action OnShieldsRelease;

    public static Action OnEscapePress;

    public static float FramerateNormalize;

    [Header("Wait X seconds before fire new rail round")]
    [Range(0, 1)] public float RapidFireRate;
    [Header("Press Mouse X seconds before autofire")]
    [Range(0, 1)][SerializeField] private float delayBeforeRapidFire;
    [Space]
    [Header("Very low value to stabilize for variable framerate")]
    [Range(0, .1f)][SerializeField] private float framerateNormalize;

    [SerializeField] private CinemachineCamera vCam;
    [SerializeField] private Vector2 cameraZoomMinMax;
    [Range(0, 5)][SerializeField] private float mouseScrollSensitivity = 1;

    [SerializeField] private DirectionStick joystick;

    private float horizAxisValue = 0;
    private float horizAxisDeltaTime = -1;

    private float vertAxisValue = 0;
    private float vertAxisDeltaTime = -1;

    private float rightMouseButtonDeltaTime = -1;
    private float spaceBarDeltaTime = -1;

    private float leftClickStartTime = -1;
    private float leftMouseButtonDeltaTime = -1;
    private bool isRapidFiring;

    private bool isRailgunTouchButtonDown;
    private bool isRailgunTouchButtonUp;
    private bool isRailgunDownDone;

    private bool isLasersTouchButtonDown;
    private bool isShieldsTouchButtonDown;

    public bool IsShiftPressed => this.isShiftPressed;
    private bool isShiftPressed;

    private bool isMouseOverUiThisFrame;
    private float distanceFromCenter;

    private void Awake()
    {
        if (this.joystick != null)
        {
            this.joystick.OnStickMove += OnJoystickMove;
            this.joystick.OnSnapBack += OnJoystickSnapBack;
        }
    }

    private void OnDestroy()
    {
        if (this.joystick != null)
        {
            this.joystick.OnStickMove -= OnJoystickMove;
            this.joystick.OnSnapBack -= OnJoystickSnapBack;
        }
    }

    private void Update()
    {
        if (IsInputBlocked) return;
        
        this.isMouseOverUiThisFrame = IsMouseOverUI();

        FramerateNormalize = this.framerateNormalize;

        HandleZoomInput();

        HandleCockpitInput();

        HandleEscapeInput();

        HandleTurnInput();
        HandleThrustInput();

        HandleDashInput();
        HandleShieldsInput();

        HandleRailgunInput();
        HandleLaserInput();
        HandleMissileInput();
    }

    private bool IsMouseOverUI()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE && !UNITY_WEBGL
        return false;
#endif

        var eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
        };
        var results = new List<RaycastResult>();

        if (EventSystem.current)
        {
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            foreach (var result in results)
            {
                if (result.gameObject.layer == 5)
                    return true;
            }
        }

        return false;
    }

    public void HandleZoomInput()
    {
        var mouseWheelValue = Input.GetAxisRaw("Mouse ScrollWheel");

        if (mouseWheelValue != 0)
        {
            if (this.vCam.Lens.Orthographic)
            {
                this.vCam.Lens.OrthographicSize += (mouseWheelValue * this.mouseScrollSensitivity);
                this.vCam.Lens.OrthographicSize = Mathf.Clamp(this.vCam.Lens.OrthographicSize, this.cameraZoomMinMax.x, this.cameraZoomMinMax.y);
            }
            else
            {
                this.vCam.Lens.FieldOfView -= (mouseWheelValue * this.mouseScrollSensitivity * 10f);
                this.vCam.Lens.FieldOfView = Mathf.Clamp(this.vCam.Lens.FieldOfView, this.cameraZoomMinMax.x, this.cameraZoomMinMax.y);
            }
        }
    }

    private void HandleCockpitInput()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Cockpit.IN.ToggleVisibility();
        }
    }

     private void HandleEscapeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnEscapePress?.Invoke();
        }
    }

    private void OnJoystickMove(float inXAxis, float inYAxis, float inXRaw, float inYRaw)
    {
        //Debug.Log($"OnJoystickMove({inXAxis}, {inYAxis})");
        this.distanceFromCenter = Vector2.Distance(Vector2.zero, new Vector2(inXAxis, inYAxis));
        this.vertAxisValue = this.distanceFromCenter;

        var angle = (Mathf.Atan2(inXRaw, -inYRaw) * Mathf.Rad2Deg) + 180;
        var shipAngle = (SpaceShip.PlayerShip.transform.rotation.eulerAngles.z + 360) % 360;

        var angleDifference = shipAngle - angle;
        if (angleDifference < -180)
            angleDifference += 360;
        else if (angleDifference > 180)
            angleDifference -= 360;

        var buffer = .5f;

        if (angleDifference > buffer)
            this.horizAxisValue = Math.Min(1, angleDifference * .02f);
        else if (angleDifference < -buffer)
            this.horizAxisValue = Math.Max(-1, angleDifference * .02f);
        else
            this.horizAxisValue = 0;
    }

    private void OnJoystickSnapBack()
    {
        this.horizAxisValue = 0;
        this.vertAxisValue = 0;
    }

    public void HandleTurnLeftPress()
    {
        this.horizAxisValue = -1;
    }

    public void HandleTurnLeftRelease()
    {
        this.horizAxisValue = 0;
    }

    public void HandleTurnRightPress()
    {
        this.horizAxisValue = 1;
    }

    public void HandleTurnRightRelease()
    {
        this.horizAxisValue = 0;
    }

    public void HandleForwardPress()
    {
        this.vertAxisValue = 1;
    }

    public void HandleForwardRelease()
    {
        this.vertAxisValue = 0;
    }

    public void HandleBackPress()
    {
        this.vertAxisValue = -1;
    }

    public void HandleBackRelease()
    {
        this.vertAxisValue = 0;
    }

    public void HandleRailgunPress()
    {
        if (!this.isRailgunDownDone)
            this.isRailgunTouchButtonDown = true;

        this.isRailgunDownDone = true;
    }

    public void HandleRailgunRelease()
    {
        this.isRailgunTouchButtonUp = true;
    }

    public void HandleShieldsPress()
    {
        this.isShieldsTouchButtonDown = true;
    }

    public void HandleShieldsRelease()
    {
        this.isShieldsTouchButtonDown = false;
    }

    public void HandleLasersPress()
    {
        this.isLasersTouchButtonDown = true;
    }

    public void HandleLasersRelease()
    {
        this.isLasersTouchButtonDown = false;
    }

    public void HandleMissilesClick()
    {
        OnShootMissilePress?.Invoke();
        CursorManager.IN.ClearLockCursor();
    }

    public void HandleDashButtonPress()
    {
        OnDashPress?.Invoke();
    }

    public void HandleDashButtonRelease()
    {
        OnDashRelease?.Invoke();
    }

    private float keyboardHorizValue = 0f;
    private float keyboardVertValue = 0f;

    private void HandleTurnInput()
    {
        var isKeyInput = false;

        var rotationAmount = 10f * Time.deltaTime;

        if(Input.GetKey(KeyCode.A))
        {
            isKeyInput = true;
            if(this.keyboardHorizValue > 0)
                this.keyboardHorizValue = 0;

            if(this.keyboardHorizValue > -1f)
                this.keyboardHorizValue -= rotationAmount;

            this.keyboardHorizValue = Mathf.Max(-1f, this.keyboardHorizValue);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            isKeyInput = true;
            if(this.keyboardHorizValue < 0)
                this.keyboardHorizValue = 0;

            if(this.keyboardHorizValue < 1f)
                this.keyboardHorizValue += rotationAmount;

            this.keyboardHorizValue = Mathf.Min(1f, this.keyboardHorizValue);
        }
        else
        {
            if(Math.Abs(this.keyboardHorizValue) > .05f)
            {
                isKeyInput = true;
                this.keyboardHorizValue *= .8f - Time.deltaTime;

                if(Math.Abs(this.keyboardHorizValue) <= .05f)
                {
                    isKeyInput = false;
                    this.horizAxisValue = 0;
                    this.keyboardHorizValue = 0;
                }
            }
        }

        if(isKeyInput)
            this.horizAxisValue = this.keyboardHorizValue;

        if (this.keyboardHorizValue == 0 && this.distanceFromCenter < .01f && Mathf.Abs(this.horizAxisValue) != 1)
            this.horizAxisValue = Input.GetAxisRaw("Horizontal");

        if (this.horizAxisValue != 0)
        {
            if (this.horizAxisDeltaTime == -1)
                this.horizAxisDeltaTime = 0;
        }
        else
        {
            this.horizAxisDeltaTime = -1;
            OnNoTurnPress?.Invoke();
        }

        if (this.horizAxisDeltaTime != -1)
        {
            if (this.horizAxisDeltaTime > this.framerateNormalize)
            {
                this.horizAxisDeltaTime = 0;

                //TODO: make this take a value so joystick control is more sensitive
                if (this.horizAxisValue > 0)
                    OnTurnRightPress?.Invoke(this.horizAxisValue);
                else
                    OnTurnLeftPress?.Invoke(this.horizAxisValue);
            }

            this.horizAxisDeltaTime += Time.deltaTime;
        }
    }

    private void HandleThrustInput()
    {
        var isKeyInput = false;

        var accellerationAmount = 10f * Time.deltaTime;

        if(Input.GetKey(KeyCode.W))
        {
            isKeyInput = true;
            if(this.keyboardVertValue < 0)
                this.keyboardVertValue = 0;

            if(this.keyboardVertValue < 1f)
                this.keyboardVertValue += accellerationAmount;

            this.keyboardVertValue = Mathf.Min(1f, this.keyboardVertValue);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            isKeyInput = true;
            if(this.keyboardVertValue > 0)
                this.keyboardVertValue = 0;

            if(this.keyboardVertValue > -1f)
                this.keyboardVertValue -= accellerationAmount;

            this.keyboardVertValue = Mathf.Max(-1f, this.keyboardVertValue);
        }
        else
        {
            if(Math.Abs(this.keyboardVertValue) > .05f)
            {
                isKeyInput = true;
                this.keyboardVertValue *= .8f - Time.deltaTime;

                if(Math.Abs(this.keyboardVertValue) <= .05f)
                {
                    isKeyInput = false;
                    this.vertAxisValue = 0;
                    this.keyboardVertValue = 0;
                }
            }
        }

        if(isKeyInput)
            this.vertAxisValue = this.keyboardVertValue;

        if (keyboardVertValue == 0 && this.distanceFromCenter < .01f && Mathf.Abs(this.vertAxisValue) != 1)
            this.vertAxisValue = Input.GetAxisRaw("Vertical");

        if (this.vertAxisValue != 0)
        {
            if (this.vertAxisDeltaTime == -1)
                this.vertAxisDeltaTime = 0;
        }
        else
        {
            this.vertAxisDeltaTime = -1;
            OnNoThrust?.Invoke();
        }

        if (this.vertAxisDeltaTime != -1)
        {
            if (this.vertAxisDeltaTime > this.framerateNormalize)
            {
                this.vertAxisDeltaTime = 0;

                if (this.vertAxisValue > 0)
                    OnThrustPress?.Invoke(this.vertAxisValue);
                else
                    OnBrakePress?.Invoke(this.vertAxisValue);
            }

            this.vertAxisDeltaTime += Time.deltaTime;
        }
    }

    private void HandleDashInput()
    {
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            OnDashPress?.Invoke();

        if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Return))
            OnDashRelease?.Invoke();
    }

    private void HandleShieldsInput()
    {
        //shields - hold down to charge up
        if (Input.GetKey(KeyCode.Space) || this.isShieldsTouchButtonDown)
        {
            if (this.spaceBarDeltaTime == -1)
                this.spaceBarDeltaTime = 0;
        }
        else
        {
            if (this.spaceBarDeltaTime > 0)
                OnShieldsRelease?.Invoke();

            this.spaceBarDeltaTime = -1;
        }

        if (this.spaceBarDeltaTime != -1)
        {
            if (this.spaceBarDeltaTime > this.framerateNormalize)
            {
                this.spaceBarDeltaTime = 0;

                OnShieldsHold?.Invoke();
            }

            this.spaceBarDeltaTime += Time.deltaTime;
        }
    }

    private void HandleRailgunInput()
    {
        if (CursorManager.IN.IsTargetModeActive || this.isShiftPressed) return;

        //this allows for 1 shot for quick press, and a time threshold before rapidfire
        if ((Input.GetMouseButtonDown(0) && !this.isMouseOverUiThisFrame) || this.isRailgunTouchButtonDown)
        {
            this.isRailgunTouchButtonDown = false;

            this.isRapidFiring = false;
            this.leftMouseButtonDeltaTime = 0;
            this.leftClickStartTime = Time.time;
        }

        if ((Input.GetMouseButtonUp(0) && !this.isMouseOverUiThisFrame) || this.isRailgunTouchButtonUp)
        {
            this.isRailgunDownDone = false;
            this.isRailgunTouchButtonUp = false;

            this.leftClickStartTime = -1;

            if (!this.isRapidFiring)
                OnShootRailgunsHold?.Invoke();
        }

        if (this.leftClickStartTime != -1)
        {
            //TODO: remove RapidFireRate and connect timing to individual railguns based on their fire rate data
            if (Time.time - this.leftClickStartTime > this.delayBeforeRapidFire && (this.RapidFireRate == 0 || this.leftMouseButtonDeltaTime >= this.RapidFireRate))
            {
                this.leftMouseButtonDeltaTime = 0;
                this.isRapidFiring = true;
                OnShootRailgunsHold?.Invoke();
            }

            this.leftMouseButtonDeltaTime += Time.deltaTime;
        }
    }

    private void HandleLaserInput()
    {
        //lasers - hold down to charge up
        if ((Input.GetMouseButton(1) && !this.isMouseOverUiThisFrame) || this.isLasersTouchButtonDown)
        {
            if (this.rightMouseButtonDeltaTime == -1)
                this.rightMouseButtonDeltaTime = 0;
        }
        else
        {
            if (this.rightMouseButtonDeltaTime != -1)
                OnLaserRelease?.Invoke();

            this.rightMouseButtonDeltaTime = -1;
        }

        if (this.rightMouseButtonDeltaTime != -1)
        {
            if (this.rightMouseButtonDeltaTime > this.framerateNormalize)
            {
                this.rightMouseButtonDeltaTime = 0;

                OnLaserHold?.Invoke();
            }

            this.rightMouseButtonDeltaTime += Time.deltaTime;
        }
    }

    private void HandleMissileInput()
    {
        //missiles - press once to shoot
        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || IsMissileButtonPressed;

        if (isShift && !this.isShiftPressed)
        {
            CursorManager.IN.SetTargetModeActive(isShift);
            OnMissileTargetingPress?.Invoke();
        }
        else if (!isShift && this.isShiftPressed)
        {
            CursorManager.IN.SetTargetModeActive(isShift);
            OnMissileTargetingRelease?.Invoke();
        }

        this.isShiftPressed = isShift;

        if (Input.GetMouseButtonDown(0) && !this.isMouseOverUiThisFrame && CursorManager.IN.IsTargetModeActive)
            TryFireMissile();
    }

    public void HandleMissileUIButtonDown()
    {
        IsMissileButtonPressed = true;
    }

    public void HandleMissileUIButtonUp()
    {
        IsMissileButtonPressed = false;

        if (CursorManager.IN.IsTargetModeActive && !CursorManager.IN.IsCursorOverUI)
            TryFireMissile();
    }

    private void TryFireMissile()
    {
        OnShootMissilePress?.Invoke();

        CursorManager.IN.ClearLockCursor();
    }
}