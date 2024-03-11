using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Levels
{

    public interface ILevelDefinition
    {
        int Order { get; }

        int StartingEnergy { get; }
    }

}