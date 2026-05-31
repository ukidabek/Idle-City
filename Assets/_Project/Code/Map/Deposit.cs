using UnityEngine;

namespace Project.Map
{
    public class Deposit : MonoBehaviour, ITielComponent, IDeposit
    {
        [SerializeField] private DepositData m_depositData;
    }

    public interface IDeposit
    {
    }

    [CreateAssetMenu(menuName = "Map/Tiles/DepositData", fileName = "DepositData")]
    public class DepositData : ScriptableObject, IDeposit
    {
        
    }
}