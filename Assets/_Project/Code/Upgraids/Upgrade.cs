using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Project.Map;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Upgrades
{
    [Serializable]
    public abstract class Level
    {
        [SerializeField] private Cost[] m_const = null;
        public Cost[] Const => m_const;
    }

    [Serializable]
    public class Dependency
    {
        [SerializeField] private Upgrade m_upgrade = null;
        [SerializeField, Min(0)] private int m_minimalLevel = 0;

        public Upgrade Upgrade => m_upgrade;

        public int MinimalLevel => m_minimalLevel;

        public bool IsSatisfied() => m_upgrade != null && m_upgrade.CurrentLevel >= m_minimalLevel;
    }

    public abstract class Upgrade : ScriptableObject, IReadOnlyList<Level>
    {
        [SerializeField] private Dependency[] m_dependencies = null;
        public IReadOnlyList<Dependency> Dependencies => m_dependencies;
        [Space]
        [SerializeField, Min(0)] private int m_level = 0;
        protected abstract Level[] Levels { get; }
        
        public bool IsUnlocked => m_level > 0 || m_dependencies == null || m_dependencies.Length == 0;
        public int CurrentLevel => m_level;
        public bool CanLevelUp => Levels != null && m_level < Levels.Length;

        public Cost[] Cost => Levels[m_level].Const;

        public void LevelUp()
        {
            if (!CanLevelUp) 
                return;
            if (m_level > 0)
                OnRevertLevel();
            m_level++;
            OnApplyLevel();
        }

        protected abstract void OnApplyLevel();
        protected abstract void OnRevertLevel();

        public IEnumerator<Level> GetEnumerator() => Levels.AsEnumerable().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => Levels?.Length ?? 0;

        public Level this[int index] => Levels[index];
    }

    public abstract class Upgrade<TargetT, LevelT> : Upgrade where TargetT : Object where LevelT : Level
    {
        [SerializeField] protected TargetT m_target = null;
        [SerializeField] private LevelT[] m_levels = null;
        protected override Level[] Levels => m_levels;

        protected override void OnApplyLevel() => ApplyLevel((LevelT)Levels[CurrentLevel - 1]);
        protected override void OnRevertLevel() => RevertLevel((LevelT)Levels[CurrentLevel - 1]);

        protected abstract void ApplyLevel(LevelT level);
        protected abstract void RevertLevel(LevelT level);
    }
}