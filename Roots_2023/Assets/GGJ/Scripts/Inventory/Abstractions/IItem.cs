namespace GGJ.Inventory
{
    public interface IItem
    {
        string PlantId { get; }

        int Cost { get; }

        void Use();
    }
}