namespace GGJ.Player
{

    public interface IEnergySystem
    {
        int CurrentEnergy { get; set; }

        bool CanUseEnergy(int energy);
    }

}