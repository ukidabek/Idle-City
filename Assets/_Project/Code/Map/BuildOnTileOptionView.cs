using System.Linq;

namespace Project.Map
{
    public class BuildOnTileOptionView : TileOptionView
    {
        private IDeposit m_deposit = null;
        private IGround m_ground = null;
        
        protected override bool Validate(TileStack data)
        {
            var components = data.Components;
            return components.Any(component => component is IGround);
        }

        protected override void InitializeInternal(TileStack data)
        {
            var components = data.Components;
            foreach (var component in components)
            {
                switch (component)
                {
                    case IGround ground:
                        m_ground = ground;
                        break;
                    case IDeposit deposit:
                        m_deposit = deposit;
                        break;
                }
            }
        }
    }
}