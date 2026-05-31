using UnityEngine;

namespace Project.Map
{
    public abstract class DataTile<DataType> : MonoBehaviour, ITielComponent where DataType : ScriptableObject
    {
        [SerializeField] protected DataType m_data = null;
    }
}