namespace GGJ.Plants
{
    public interface ISeedItemDefinition
    {
        int Cost { get; }
        string SeedId { get; }
        string PlantId { get; }
    }
}