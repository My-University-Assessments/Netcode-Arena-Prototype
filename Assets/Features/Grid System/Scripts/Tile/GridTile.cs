using System;
using System.Dynamic;
using ArenaPrototype.Feature.GridSystem.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class GridTile : MonoBehaviour, IGridTile, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Tile Settings")]
    [SerializeField] private TileType _tileType = TileType.Ground;
    public TileType tileType => _tileType;

    [field: Header("Testing")]
    [field: SerializeField] public GameObject occupiedBy { get; private set; }
    public bool isOccupied => occupiedBy != null;

    // INFO: Actions
    public static UnityAction<IGridTile> OnTileClicked { get; set; }

    // INFO: Components
    private Rigidbody rb;
    public bool interactable { get; protected set; }

    // INFO: Utility
    private bool hovered;
    private Color originalColour;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        GetComponent<Collider>().isTrigger = true;

    }

    public void Clicked()
    {
        if (tileType != TileType.Ground) return;
        OnTileClicked?.Invoke(gameObject.GetComponent<IGridTile>());

    }

    public void Hover()
    {
        if (tileType != TileType.Ground) return;
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();

        hovered = !hovered;

        if (hovered)
        {
            originalColour = meshRenderer.material.color;
            meshRenderer.material.color = Color.black;

        }
        else
        {
            meshRenderer.material.color = originalColour;

        }

    }

    #region Detection Events
    void OnTriggerEnter(Collider other) => IsOccupied(other);
    void OnTriggerStay(Collider other)
    {
        if (occupiedBy == null) IsOccupied(other);

    }
    void OnTriggerExit(Collider other) => IsOccupied(null);

    #endregion

    #region Pointer Events
    public void OnPointerClick(PointerEventData eventData) => Clicked();
    public void OnPointerEnter(PointerEventData eventData) => Hover();
    public void OnPointerExit(PointerEventData eventData) => Hover();

    #endregion

    #region Helper
    private void IsOccupied(Collider other)
    {
        // GUARD: Prevent null
        if (other == null)
        {
            occupiedBy = null;
            return;
        }

        if (other.transform.gameObject == gameObject || other.transform.gameObject.TryGetComponent(out IGridTile gridTile)) return;

        occupiedBy = other.transform.gameObject;

    }

    #endregion

}