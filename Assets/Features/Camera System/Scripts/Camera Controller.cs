using System;
using ArenaPrototype.Feature.GridSystem;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    private enum ViewType
    {
        Perspective,
        Orthographic

    }

    [Header("Camera To Control")]
    [SerializeField] private Camera m_camera;

    [Header("View Settings")]
    [SerializeField] private Vector2 m_startingPosition = new Vector2(0f, 0f);
    [SerializeField] private float m_viewAngle = 45;
    [SerializeField] private float m_nearClipPlane = 0.1f;
    [SerializeField] private float m_farClipPlane = 200;
    [SerializeField] private ViewType m_viewType = ViewType.Orthographic;

    [Header("Zoom Settings")]
    [SerializeField, Min(0), Tooltip("How far the player can zoom in.")] private int m_maxZoom = 1;
    [SerializeField, Min(1), Tooltip("How far the player can zoom out.")] private int m_minZoom = 5;
    [SerializeField] private float m_zoomSpeed = 1f;
    [SerializeField] private bool m_canZoom = true;

    [Header("Pan Settings")]
    [SerializeField, Min(1)] private float m_panningSpeed = 1f;
    [SerializeField, Min(1)] private float m_rotationSpeed = 1f;
    public Vector2Int boardBounds;
    [SerializeField] private bool m_canPan = true;

    [Header("Debug Settings")]
    [SerializeField] private bool m_debugMode;

    // INFO: Gamepad Settings
    [SerializeField] private float m_gamepadRotationSensitivity = 1.5f;
    [SerializeField] private float m_gamepadPanningSensitivity = 200f;


    #region Inputs
    // INFO: Input Tracking
    private enum InputDeviceType { Mouse, Gamepad, Keyboard }
    private InputDeviceType m_currentInputDeviceType;
    private Mouse m_mouse;
    private Gamepad m_gamepad;

    #endregion

    #region Runtime Calculations
    private Vector3 m_centrePoint;
    private float m_currentRotationAngle = 0f;
    private Vector2 m_lastInputPosition;

    #endregion

    private void Start()
    {
        InitialiseCamera();
        if (!FindCurrentInputDevice()) return;


    }

    private void Update()
    {
        FindCurrentInputDevice();

    }

    private void LateUpdate()
    {



        // INFO: Zoom
        if (m_mouse.scroll.up.IsActuated() || m_mouse.scroll.down.IsActuated() || (m_gamepad != null && (m_gamepad.rightStick.up.IsActuated() || m_gamepad.rightStick.down.IsActuated())))
        {
            HandleZoom(m_currentInputDeviceType == InputDeviceType.Mouse ? -m_mouse.scroll.y.ReadValue() : -m_gamepad.rightStick.y.ReadValue());

        }

        // INFO: Rotating
        if ((m_mouse != null && m_mouse.rightButton.isPressed) || (m_gamepad != null && (m_gamepad.rightStick.right.IsActuated() || m_gamepad.rightStick.left.IsActuated())))
        {
            HandleRotating(m_currentInputDeviceType == InputDeviceType.Mouse ? GetMousePosition() : GetGamepadStickPosition(true));

        }

        // INFO: Panning
        if ((m_mouse != null && m_mouse.middleButton.isPressed) || (m_gamepad != null && m_gamepad.leftStick.IsActuated()))
        {
            HandlePanning(m_currentInputDeviceType == InputDeviceType.Mouse ? GetMousePosition() : GetGamepadStickPosition(false));
        }
        else
        {
            if (m_currentInputDeviceType == InputDeviceType.Mouse)
                m_lastInputPosition = GetMousePosition();
        }

    }



    private void HandleRotating(Vector2 newPosition)
    {
        Vector2 inputDelta = newPosition - m_lastInputPosition;
        inputDelta *= m_currentInputDeviceType == InputDeviceType.Gamepad ? m_gamepadRotationSensitivity : Time.deltaTime;
        m_lastInputPosition = newPosition;

        m_currentRotationAngle += inputDelta.x * m_rotationSpeed * m_camera.orthographicSize * Time.deltaTime;
        m_camera.transform.rotation = Quaternion.Euler(m_viewAngle, m_currentRotationAngle, 0f);

    }


    #region Setup
    private void InitialiseCamera()
    {
        if (!m_camera) { ShutdownIfError($"Camera has not been set!"); return; }
        if (!m_camera.isActiveAndEnabled) m_camera.enabled = true;

        m_camera.transform.position = m_startingPosition;
        m_camera.transform.rotation = Quaternion.Euler(m_viewAngle, 0f, 0f);
        m_centrePoint = new Vector3(m_startingPosition.x, 0f, m_startingPosition.y);

        m_camera.orthographic = m_viewType == ViewType.Orthographic ? true : false;
        if (m_camera.orthographic)
        {
            m_camera.orthographicSize = m_minZoom / 2 * 1.2f;

        }
        else
        {
            m_camera.fieldOfView = m_minZoom / 2 * 1.2f;

        }

        Mathf.Clamp(m_viewType == ViewType.Orthographic ? m_camera.orthographicSize : m_camera.fieldOfView, 0, m_minZoom);
        m_camera.nearClipPlane = m_nearClipPlane;
        m_camera.farClipPlane = m_farClipPlane;

        // INFO: Debug
        if (!m_debugMode) return;
        Debug.Log($"Camera Initialised:");
        Debug.Log($"Position: {m_camera.transform.position} | Rotation: {m_viewAngle} | Size: {m_camera.orthographicSize}");
        Debug.Log($"Near: {m_camera.nearClipPlane}, Far: {m_camera.farClipPlane}, Angle: {m_viewAngle}°");

    }

    #endregion

    #region Zoom
    private void HandleZoom(float zoomInput)
    {
        if (!m_canZoom) { Debug.LogWarning($"Can zoom is disabled!"); return; }
        float zoomDelta = zoomInput * m_zoomSpeed;

        if (m_currentInputDeviceType == InputDeviceType.Gamepad)
            zoomDelta *= m_gamepadRotationSensitivity * Time.deltaTime;

        // INFO: Up
        switch (m_viewType)
        {
            case ViewType.Orthographic:
                m_camera.orthographicSize = Mathf.Clamp(m_camera.orthographicSize + zoomDelta, m_maxZoom, m_minZoom);
                break;

            case ViewType.Perspective:
                m_camera.fieldOfView = Mathf.Clamp(m_camera.fieldOfView + zoomDelta, m_maxZoom, m_minZoom);
                break;

        }

        float updatedValue = m_viewType == ViewType.Orthographic ? m_camera.orthographicSize : m_camera.fieldOfView;
        if (m_debugMode && (m_camera.orthographicSize == m_maxZoom || m_camera.orthographicSize == m_minZoom || m_camera.fieldOfView == m_maxZoom || m_camera.fieldOfView == m_minZoom))
            Debug.LogWarning($"At max zoom! {updatedValue}");

    }

    #endregion

    #region Panning
    private void HandlePanning(Vector2 newPosition)
    {
        if (!m_canPan) { Debug.LogWarning($"Panning is disabled!"); return; }

        // INFO: Track mouse position
        Vector2 inputDelta = newPosition - m_lastInputPosition;
        if (m_currentInputDeviceType == InputDeviceType.Gamepad)
            inputDelta *= m_gamepadPanningSensitivity;

        m_lastInputPosition = newPosition;

        // INFO: Get Direction
        Vector3 rightDirection = m_camera.transform.right;
        rightDirection.y = 0f;
        rightDirection.Normalize();

        Vector3 forwardDirection = m_camera.transform.up;
        forwardDirection.y = 0f;
        forwardDirection.Normalize();

        // INFO: Calculate new centre
        m_centrePoint += (rightDirection * inputDelta.x + forwardDirection * inputDelta.y) * m_panningSpeed * m_camera.orthographicSize * Time.deltaTime;

        // INFO: Clamp to actual board bounds
        m_centrePoint.x = Mathf.Clamp(m_centrePoint.x, 0, boardBounds.x);
        m_centrePoint.z = Mathf.Clamp(m_centrePoint.z, -boardBounds.y / 3f, boardBounds.y);

        // INFO: Update camera position
        float heightOffset = m_minZoom;
        Vector3 cameraPosition = m_centrePoint + new Vector3(0f, heightOffset, 0f);
        m_camera.transform.position = cameraPosition;
        m_camera.transform.rotation = Quaternion.Euler(m_viewAngle, m_currentRotationAngle, 0f);

    }
    #endregion

    #region Helper
    private Vector2 GetMousePosition() => m_mouse.position.ReadValue();
    private Vector2 GetGamepadStickPosition(bool rightStick = true) => rightStick ? m_gamepad.rightStick.value : m_gamepad.leftStick.value;

    #endregion

    #region Utility
    private void ShutdownIfError(object message)
    {
        Debug.LogError($"{message}");
        enabled = false;

    }

    private bool FindCurrentInputDevice()
    {
        m_mouse = Mouse.current;
        m_gamepad = Gamepad.current;

        if (m_gamepad != null)
        {
            bool gamepadUsed = m_gamepad.leftStick.ReadValue().sqrMagnitude > 0.01f ||
                               m_gamepad.rightStick.ReadValue().sqrMagnitude > 0.01f ||
                               m_gamepad.IsPressed();

            if (gamepadUsed)
            {
                m_currentInputDeviceType = InputDeviceType.Gamepad;
                return true;
            }
        }

        // 2. Check for Intentional Mouse Activity (Buttons or physical movement)
        // Avoid raw .wasUpdatedThisFrame here as it is too sensitive
        if (m_mouse != null)
        {
            bool mouseMoved = m_mouse.delta.ReadValue().sqrMagnitude > 0.1f;
            bool mouseClicked = m_mouse.leftButton.isPressed ||
                                m_mouse.rightButton.isPressed ||
                                m_mouse.middleButton.isPressed;
            bool mouseScrolled = m_mouse.scroll.ReadValue().sqrMagnitude > 0.01f;

            if (mouseMoved || mouseClicked || mouseScrolled)
            {
                m_currentInputDeviceType = InputDeviceType.Mouse;
                return true;

            }

        }

        if (m_mouse == null && m_gamepad == null)
        {
            Debug.LogError($"No valid input device detected!");
            enabled = false;
            return false;

        }

        return true;

    }


    #endregion


}
