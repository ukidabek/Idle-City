using System;
using cookie.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utilities.General;

namespace _Project.CameraManagement
{
    public class CameraController : MonoBehaviour, ILogEnabled
    {
        public Color Color { get; } = new Color(0.4f, 0.8f, 1f);
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;
        
        [SerializeField, ReadOnly] private Camera m_camera = null;
        [SerializeField, ReadOnly] private EventSystem m_eventSystem = null;
        [Space]
        [SerializeField] private InputActionReference m_clickActionAsset = null;
        [SerializeField] private InputActionReference m_deltaActionAsset = null;
        [SerializeField] private InputActionReference m_positionActionAsset = null;
        [Space]
        [SerializeField] private Transform m_cameraTarget = null;
        [SerializeField, Min(0)] private float m_speed = 5f;
        [Space]
        [SerializeField] private UnityEvent<Vector3> OnPointSelected = new  UnityEvent<Vector3>();
        
        private Plane m_plane = new Plane(Vector3.up, Vector3.zero);
        private bool m_isOverUI = false;

        private void Awake()
        {
            m_camera = Camera.main;
            m_eventSystem = EventSystem.current;
            m_deltaActionAsset.action.performed += ReadPosition;
            m_clickActionAsset.action.canceled += Select;
        }

        private void Select(InputAction.CallbackContext obj)
        {
            if (m_isOverUI) return;
            var position = m_positionActionAsset.action.ReadValue<Vector2>();
            var ray = m_camera.ScreenPointToRay(position);
            if (!m_plane.Raycast(ray, out var enter)) return;
            var point = ray.GetPoint(enter);
            this.Log($"Point {point: 0.00} selected!", LogType.Log, this);
            OnPointSelected.Invoke(point);
        }

        private void ReadPosition(InputAction.CallbackContext obj)
        {
            if (m_isOverUI) return;
            if (m_clickActionAsset.action.phase != InputActionPhase.Performed) return;
            var delta = obj.ReadValue<Vector2>();

            var position = m_cameraTarget.position;
            position.x += -delta.x * m_speed * Time.deltaTime;
            position.z += -delta.y * m_speed * Time.deltaTime;

            m_cameraTarget.position = position;
        }

        private void Update() => m_isOverUI = m_eventSystem.IsPointerOverGameObject();
    }
}
