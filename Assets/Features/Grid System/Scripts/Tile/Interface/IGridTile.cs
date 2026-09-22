using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ArenaPrototype.Feature.GridSystem.Interface
{
    public interface IGridTile
    {
        bool interactable { get; }
        TileType tileType { get; }
        GameObject occupiedBy { get; }
        bool isOccupied { get; }
        GameObject gameObject { get; }

        static UnityAction OnTileClicked { get; }

        void Hover();
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