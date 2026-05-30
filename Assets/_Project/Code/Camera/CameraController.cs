using UnityEngine;
using UnityEngine.InputSystem;
using Utilities.General;

namespace _Project.CameraManagement
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Camera m_camera = null;
        [Space]
        [SerializeField] private InputActionReference m_clickActionAsset = null;
        [SerializeField] private InputActionReference m_deltaActionAsset = null;
        [SerializeField] private InputActionReference m_positionActionAsset = null;
        [Space]
        [SerializeField] private Transform m_cameraTarget = null;
        [SerializeField, Min(0)] private float m_speed = 5f;

        private Plane m_plane = new Plane(Vector3.up, Vector3.zero);
        
        private void Awake()
        {
            m_camera = Camera.main;
            m_deltaActionAsset.action.performed += ReadPosition;
            m_clickActionAsset.action.canceled += Select;
        }

        private void Select(InputAction.CallbackContext obj)
        {
            var position = m_positionActionAsset.action.ReadValue<Vector2>();
            var ray = m_camera.ScreenPointToRay(position);
            if(!m_plane.Raycast(ray, out var enter)) return;
            Debug.Log(ray.GetPoint(enter));
        }

        private void ReadPosition(InputAction.CallbackContext obj)
        {
            if(m_clickActionAsset.action.phase != InputActionPhase.Performed) return;
            var delta = obj.ReadValue<Vector2>();

            var position = m_cameraTarget.position;
            position.x += -delta.x * m_speed * Time.deltaTime;
            position.z += -delta.y * m_speed * Time.deltaTime;

            m_cameraTarget.position = position;
        }

    }
}
