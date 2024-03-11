using UnityEngine;

namespace GGJ.Plants
{
    public abstract class PlantBase : MonoBehaviour
    {
        [SerializeField]
        private PlantItemDefinition itemDefinition = default;
        private IGrowEffect[] growEffects;
        private AudioSource soundEffect;

        public int Health => itemDefinition.Health;

        public int Cost => itemDefinition.Cost;

        public string PlantId => itemDefinition.PlantId;

        public Vector2 PlacingRayDirection => itemDefinition.PlacingRayDirection;
        public float PlacingRayDistance => itemDefinition.PlacingRayDistance;
        public string TargetTag => itemDefinition.TargetTag;

        protected virtual void Awake()
        {
            growEffects = GetComponentsInChildren<IGrowEffect>();
            soundEffect = GetComponent<AudioSource>();
        }
        
        public virtual void Grow()
        {
            PlaySoundEffect();

            foreach (var growEffect in growEffects)
            {
                growEffect.Play();
            }
        }

        protected abstract void Die();

        public void PlaySoundEffect()
        {
            soundEffect.Play();
        }
    }
}