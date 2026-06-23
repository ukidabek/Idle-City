using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI.Upgrades
{
    public class UpgradeConnector : UIBehaviour
    {
        [SerializeField] private float m_lineThickness = 2f;

        public void Connect(RectTransform fromAnchor, RectTransform toAnchor)
        {
            var fromPosition = fromAnchor.position;
            var toPosition = toAnchor.position;
            var direction = toPosition - fromPosition;
            var distance = direction.magnitude;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            var connectorTransform = (RectTransform) transform;
            connectorTransform.position = (fromPosition + toPosition) * 0.5f;
            connectorTransform.sizeDelta = new Vector2(distance, m_lineThickness);
            connectorTransform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void Clear()
        {
            ((RectTransform) transform).sizeDelta = Vector2.zero;
        }
    }
}
