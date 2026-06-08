using Project.Resources;

namespace Project.Map
{
    public class Consumer : Client
    {
        public override ClientType Type => ClientType.Consumer;

        private bool m_satisfied = false;

        public bool Satisfied
        {
            get
            {
                var value = m_satisfied;
                m_satisfied = false;
                return value;
            }
        }

        public override float Amount
        {
            get
            {
                m_satisfied = Data.Resource.Value >= base.Amount;
                return base.Amount;
            }
        }
    }
}