using System;
using ArenaPrototype.Feature.GridSystem;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraController : MonoBehaviour
{
    [Header("Camera To Control")]
    [SerializeField] private Camera m_camera;

    [Header("View Settings")]
    [SerializeField] private Vector3 m_startingPosition = new Vector3(0f, 5f, 0f);
    [SerializeField] private float m_viewAngle = 45;

    [Header("Zoom Settings")]
    [SerializeField, Range(1, 15)] private int m_minZoom = 1;
    [SerializeField, Range(1, 15)] private int m_maxZoom = 5;
    [SerializeField] private float m_zoomSpeed = 1f;
    [SerializeField] private bool m_canZoom = true;

    [Header("Pan Settings")]
    [SerializeField] private float m_panningSpeed = 1f;
    [SerializeField] private Vector2Int m_boardBounds;
    [SerializeField] private bool m_canPan = true;
    private Vector3 m_centrePoint;

    [Header("Debug Settings")]
    [SerializeField] private bool m_debugMode;

    private void Start()
    {
        if (Mouse.current == null) { ShutdownIfError($"No mouse detected!"); return; }
        InitialiseCamera();

    }

    private void LateUpdate()
    {
        if (Mouse.current.scroll.IsActuated())
            Zoom(Mouse.current.scroll);

        // TODO: Implement rotation
        // if (Mouse.current.middleButton.isPressed)
        
        if( Mouse.current.leftButton.isPressed)
            Panning(Mouse.current.position.ReadValue());
        else
            m_lastMousePosition = Mouse.current.position.ReadValue();

    }

    private void InitialiseCamera()
    {
        if (!m_camera) { ShutdownIfError($"Camera has not been set!"); return; }

        m_camera.transform.position = m_startingPosition;
        m_camera.orthographicSize = m_maxZoom / 2;
        m_camera.transform.rotation = Quaternion.Euler(m_viewAngle, 0f, 0f);


        if (m_debugMode) Debug.Log($"Camera Initialised: Position: {m_camera.transform.position} | Rotation: {m_viewAngle} | Size: {m_camera.orthographicSize}");

    }

    #region Zoom
    private void Zoom(DeltaControl scrollWheelDelta)
    {
        if (!m_canZoom) { Debug.LogWarning($"Can zoom is disabled!"); return; }
        if (scrollWheelDelta == null) { Debug.LogError($"No mouse position sent!"); return; }

        // INFO: Up
        if (scrollWheelDelta.up.IsActuated())
        {
            if (m_camera.orthographicSize > m_minZoom) m_camera.orthographicSize -= m_zoomSpeed;
        }

        // INFO: Down
        if (scrollWheelDelta.down.IsActuated())
        {
            if (m_camera.orthographicSize < m_maxZoom) m_camera.orthographicSize += m_zoomSpeed;
        }

        if (m_camera.orthographicSize == m_minZoom || m_camera.orthographicSize == m_maxZoom) { Debug.LogWarning($"At max zoom! {m_camera.orthographicSize}"); return; }

    }

    #endregion

    #region Panning
    private float m_currentRotationAngle = 0f;
    private Vector2 m_lastMousePosition;
    private void Panning(Vector2 mousePosition)
    {
        if (!m_canPan) { Debug.LogWarning($"Panning is disabled!"); return; }
        if (mousePosition == Vector2.zero) { Debug.LogError($"No mouse position sent!"); return; }

        // INFO: Track mouse position
        Vector2 mouseDelta = mousePosition - m_lastMousePosition;
        m_lastMousePosition = mousePosition;

        // INFO: Get Direction
        float angleInRadians = m_currentRotationAngle * Mathf.Deg2Rad;
        Vector3 rightDirection = new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians));
        Vector3 forwardDirection = new Vector3(-Mathf.Sin(angleInRadians), 0, Mathf.Cos(angleInRadians));

        // INFO: Calculate new centre
        m_centrePoint += (rightDirection * mouseDelta.x + forwardDirection * mouseDelta.y) * m_panningSpeed * m_camera.orthographicSize * Time.deltaTime;

        // INFO: Clamp to actual board bounds
        m_centrePoint.x = Mathf.Clamp(m_centrePoint.x, 0, m_boardBounds.x);
        m_centrePoint.z = Mathf.Clamp(m_centrePoint.z, -m_boardBounds.y / 3f, m_boardBounds.y);

        // INFO: Update camera position
        Vector3 cameraPosition = m_centrePoint + new Vector3(0f, m_startingPosition.y, 0f);
        m_camera.transform.position = cameraPosition;
        m_camera.transform.rotation = Quaternion.Euler(m_viewAngle, m_currentRotationAngle, 0f);

    }
    #endregion

    #region Utility
    private void ShutdownIfError(object message)
    {
        Debug.LogError($"{message}");
        enabled = false;

    }

    #endregion


}
