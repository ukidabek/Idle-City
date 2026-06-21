using System.Collections.Generic;
using Project.Map;
using UnityEngine;

namespace Code.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrades", menuName = "Upgrades/ProducerUpgrade", order = 0)]
    public class ProducerUpgrade : Upgrade<ProducerData, ProducerLevel>
    {
        private readonly List<ProducerModifier> m_activeModifiers = new();

        protected override void ApplyLevel(ProducerLevel level)
        {
            foreach (var info in level.Modifiers)
            {
                var modifier = info.Build();
                if (!modifier.HasValue) continue;
                m_activeModifiers.Add(modifier.Value);
                m_target.Apply(modifier.Value);
            }
        }

        protected override void RevertLevel(ProducerLevel level)
        {
            foreach (var modifier in m_activeModifiers)
                m_target.Remove(modifier);
            m_activeModifiers.Clear();
        }
    }
}