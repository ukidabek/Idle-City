using System.Linq;
using Project.Resources;
using UnityEngine;

namespace Project.Map
{
    public class Producer : Client
    {
        [SerializeField] private Consumer[] m_consumers;
        public override ClientType Type  => ClientType.Producer;

        public override float Amount
        {
            get
            {
                if (m_consumers == null || m_consumers.Length == 0)
                    return base.Amount;
                
                return m_consumers.All(c => c.Satisfied) ? base.Amount : 0;
            }
        }
    }
}