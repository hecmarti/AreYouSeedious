using UnityEngine;
namespace GGJ.Plants
{

    [CreateAssetMenu(fileName = "PlantItemDefinition", menuName = "GGJ/PlantItemDefinition")]
    public class PlantItemDefinition : ScriptableObject, ISeedItemDefinition
    {

        [SerializeField]
        private string seedId;

        public string SeedId => seedId;

        [SerializeField]
        private string plantId;

        public string PlantId => plantId;

        [SerializeField]
        private int cost;

        public int Cost => cost;

        [SerializeField]
        private int health;

        public int Health => health;

        [Header("Placing data")]
        [SerializeField]
        private string targetTag = string.Empty;

        public string TargetTag => targetTag;
        
        [SerializeField]
        private Vector2 placingRayDirection;

        public Vector2 PlacingRayDirection => placingRayDirection;
        
        [SerializeField]
        private float placingRayDistance;

        public float PlacingRayDistance => placingRayDistance;

        [SerializeField]
        private Sprite plantSprite;
        public Sprite PlantSprite => plantSprite;

    }
}