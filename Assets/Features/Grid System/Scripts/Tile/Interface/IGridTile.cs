using System;
using UnityEngine;
using UnityEngine.Events;

namespace ArenaPrototype.Feature.GridSystem.Interface
{
    public interface IGridTile
    {
        bool interactable { get; }
        TileType tileType { get; }

        static UnityAction<GameObject> OnTileClicked { get; }

        void Hovered();
        void Clicked();

    }
}

[Serializable]
public enum TileType
{
    Ground = 0,
    Water = 1,
    Hill = 2,

}