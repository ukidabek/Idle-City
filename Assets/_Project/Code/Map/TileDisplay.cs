using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows;
using cookie.Logging;
using UnityEngine;
using Utilities.General;

namespace Project.Map
{
    public class TileDisplay : Window, ILogEnabled
    {
        public Color Color { get; } = new Color(0.6f, 1f, 0.5f);
        [field: SerializeField] public LogMode Mode { get; private set; } = LogMode.All;
        
        private TileStack m_selectedStack = null;
        [SerializeField] private TileOptionView[] m_optionsViews = null;
        [SerializeField, ReadOnly] private Camera m_camera = null;
        [SerializeField] private float m_radius = 0.5f;
        [SerializeField, Range(-360f, 360f)] private float m_angleOffset = 0f;
        private List<TileOptionView> m_activeOptionViews = new List<TileOptionView>(10);
        
        protected override IEnumerator Start()
        {
            yield return base.Start();
            m_camera = Camera.main;
        }

        public void Select(TileStack tileStack)
        {
            if (tileStack == null)
            {
                Hide();
                return;
            }
            
            m_selectedStack = tileStack;
            UpdatePosition();

            m_activeOptionViews.Clear();
            m_activeOptionViews.AddRange(m_optionsViews.Where(view =>
            {
                view.Initialize(m_selectedStack);
                return view.gameObject.activeSelf;
            }));

            var count = m_activeOptionViews.Count;
            var anglePerSegment = 360f / count;
            for (var i = 0; i < count; i++)
            {
                var angle = (m_angleOffset + anglePerSegment * i) * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * m_radius;
                m_activeOptionViews[i].transform.localPosition = position;
            }
            
            Show();
        }

        private void UpdatePosition()
        {
            if (m_selectedStack == null) return;
            var screenPosition = m_camera.WorldToScreenPoint(m_selectedStack.Peek().transform.position);
            m_canvasHolder.transform.position = screenPosition;
        }

        private void LateUpdate() => UpdatePosition();

        public override void Show()
        {
            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
    }
}