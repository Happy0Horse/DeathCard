using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HexViewManager : MonoBehaviour
{
    public enum ViewMode { Orbit, TopDown, FirstPerson }
    public ViewMode CurrentView { get; private set; } = ViewMode.Orbit;
    public bool IsLocked { get; set; }

    [Header("Camera References")]
    public Camera worldCamera;
    public Camera fpCamera;

    [Header("Orbit Settings")]
    public float orbitDistance = 25f;
    public float orbitSensitivity = 60f;
    public float minPitch = 20f, maxPitch = 85f;
    public float defaultPitch = 45f;

    [Header("Top-Down Settings")]
    public float topFollowSmoothness = 15f;

    [Header("First-Person Settings")]
    public float fpMinPitch = -30f;
    public float fpMaxPitch = 30f;
    public float fpSensitivity = 40f;

    [Header("Transition Settings")]
    public float transitionDuration = 0.5f;

    [Header("Zoom Settings")]
    public float zoomSensitivity = 2f;
    public float minZoom = 10f;
    public float maxZoom = 40f;

    private Transform _gridCenter;
    private float _yaw;
    private float _pitch;
    private Vector2 _lookInput;
    private float _scrollInput;
    private float _currentZoom;
    private float _transitionTimer = 0f;

    void Awake()
    {
        _currentZoom = orbitDistance;
        ResetOrbitAngles();
        SwitchCameraHardware();
    }

    private void OnEnable() => GameEvents.OnRequestAttackMode += ToggleAttackMode;
    private void OnDisable() => GameEvents.OnRequestAttackMode -= ToggleAttackMode;

    void Start() => UpdateCursorState();

    void LateUpdate()
    {
        if (IsLocked) return;

        if (CurrentView != ViewMode.FirstPerson)
        {
            HandleZoom();
            if (CurrentView == ViewMode.Orbit && _gridCenter != null) HandleOrbit();
            else if (CurrentView == ViewMode.TopDown) HandleTopDown();
        }
        else
        {
            HandleFirstPerson();
        }
    }

    private void HandleZoom()
    {
        if (Mathf.Abs(_scrollInput) > 0.01f)
        {
            _currentZoom -= _scrollInput * zoomSensitivity;
            _currentZoom = Mathf.Clamp(_currentZoom, minZoom, maxZoom);
        }
    }

    private void HandleOrbit()
    {
        _yaw += _lookInput.x * orbitSensitivity * Time.deltaTime;
        _pitch -= _lookInput.y * orbitSensitivity * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

        Quaternion targetRot = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3 targetPos = _gridCenter.position - (targetRot * Vector3.forward * _currentZoom);

        if (_transitionTimer > 0)
        {
            _transitionTimer -= Time.deltaTime;
            float t = 1f - (_transitionTimer / transitionDuration);
            worldCamera.transform.position = Vector3.Lerp(worldCamera.transform.position, targetPos, t);
            worldCamera.transform.rotation = Quaternion.Slerp(worldCamera.transform.rotation, targetRot, t);
        }
        else
        {
            worldCamera.transform.SetPositionAndRotation(targetPos, targetRot);
        }
    }

    private void HandleTopDown()
    {
        Vector3 targetPos = transform.position + Vector3.up * _currentZoom;
        Quaternion targetRot = Quaternion.Euler(90f, 0f, 0f);

        worldCamera.transform.position = Vector3.Lerp(worldCamera.transform.position, targetPos, Time.deltaTime * topFollowSmoothness);
        worldCamera.transform.rotation = Quaternion.Slerp(worldCamera.transform.rotation, targetRot, Time.deltaTime * topFollowSmoothness);
    }

    private void HandleFirstPerson()
    {
        _yaw += _lookInput.x * fpSensitivity * Time.deltaTime;
        _pitch -= _lookInput.y * fpSensitivity * Time.deltaTime;

        _pitch = Mathf.Clamp(_pitch, fpMinPitch, fpMaxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0);
    }

    private void SwitchCameraHardware()
    {
        bool isFP = CurrentView == ViewMode.FirstPerson;
        if (fpCamera != null) fpCamera.enabled = isFP;
        if (worldCamera != null) worldCamera.enabled = !isFP;
    }

    private void ResetOrbitAngles()
    {
        _yaw = transform.eulerAngles.y;
        _pitch = defaultPitch;
    }

    private void UpdateCursorState()
    {
        bool shouldLock = (CurrentView == ViewMode.Orbit || CurrentView == ViewMode.FirstPerson);
        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }

    public void ToggleAttackMode(CardData data, Action onComplete)
    {
        if (data.category != CardData.CardCategory.Attack) return;

        CurrentView = (CurrentView == ViewMode.FirstPerson) ? ViewMode.Orbit : ViewMode.FirstPerson;

        if (CurrentView == ViewMode.FirstPerson)
        {
            GameEvents.OnCancelCurrentAction?.Invoke();
            _yaw = transform.eulerAngles.y;
            _pitch = 0f;
        }
        else
        {
            _transitionTimer = transitionDuration;
            ResetOrbitAngles();
        }

        SwitchCameraHardware();
        UpdateCursorState();
    }

    public void OnToggleView(InputValue value)
    {
        if (!value.isPressed) return;

        GameEvents.OnCancelCurrentAction?.Invoke();

        if (CurrentView == ViewMode.FirstPerson)
        {
            CurrentView = ViewMode.Orbit;
            _currentZoom = maxZoom;
            SwitchCameraHardware();
        }
        else
        {
            CurrentView = (CurrentView == ViewMode.Orbit) ? ViewMode.TopDown : ViewMode.Orbit;
            _currentZoom = maxZoom;
        }

        UpdateCursorState();
    }

    public void ExitFirstPerson()
    {
        if (CurrentView != ViewMode.FirstPerson) return;

        CurrentView = ViewMode.Orbit;
        _transitionTimer = transitionDuration;

        SwitchCameraHardware();
        UpdateCursorState();
    }

    public void EnterFirstPerson()
    {
        if (CurrentView == ViewMode.FirstPerson) return;

        GameEvents.OnCancelCurrentAction?.Invoke();
        CurrentView = ViewMode.FirstPerson;

        _yaw = transform.eulerAngles.y;
        _pitch = 0f;

        SwitchCameraHardware();
        UpdateCursorState();
    }

    public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();
    public void OnScroll(InputValue value) => _scrollInput = value.Get<Vector2>().y;
    public void SetGridCenter(Transform center) => _gridCenter = center;
}