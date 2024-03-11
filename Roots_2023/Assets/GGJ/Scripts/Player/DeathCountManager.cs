using DG.Tweening;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace GGJ.Player.Death
{
    public class DeathCountManager : MonoBehaviour, MMEventListener<CorgiEngineEvent>
    {
        [SerializeField]
        private TextMeshProUGUI deathCountText = default;

        [SerializeField]
        private float onDeathScale = default;

        [SerializeField]
        private float onDeathDuration = default;

        [SerializeField]
        private Transform parent = default;

        private int deathCount = 0;

        private void Awake() => UpdateDeathText();
        private void UpdateDeathText() => deathCountText.text = deathCount.ToString("000");

        void OnEnable()
        {
            this.MMEventStartListening<CorgiEngineEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<CorgiEngineEvent>();
        }

        public void OnMMEvent(CorgiEngineEvent e)
        {
            if (e.EventType != CorgiEngineEventTypes.PlayerDeath) return;

            parent.DOScale(onDeathScale, onDeathDuration).SetLoops(2, LoopType.Yoyo);

            deathCount++;
            deathCountText.text = deathCount.ToString();
        }
    }
}
