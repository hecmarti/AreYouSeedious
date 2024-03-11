using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGJ.Inventory
{
    public interface IInventoryDefinition
    {       
        IEnumerable<IItem> Items { get; }

    }

}

