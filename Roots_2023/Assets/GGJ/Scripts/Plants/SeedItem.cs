namespace GGJ.Plants
{
    public class SeedItem : ISeedItemDefinition
    {
        private readonly PlantItemDefinition definition;

        public int Cost => definition.Cost;

        public string PlantId => definition.PlantId;
        public string SeedId => definition.SeedId;

        public SeedItem(PlantItemDefinition definition)
        {
            this.definition = definition;
        }

        public void Use()
        {
            throw new System.NotImplementedException();
        }
    }
}
