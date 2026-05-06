using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

public class HexGridNavigator : MonoBehaviour
{
    public HexGrid grid;
    public float moveSpeed = 8f;
    public float rotationSpeed = 720f;
    public float heightOffset = 2.0f;
    public Material highlightMaterial;
    public LayerMask obstacleLayer;
    public bool IsMoving => _isMoving;
    public bool IgnoreCancelAction { get; set; }

    private Vector2Int _currentCoord;
    private bool _isMoving = false;
    private bool _isSelecting = false;
    private HexViewManager _viewManager;
    private Action<HexCell> _onCellSelectedCallback;

    private List<GameObject> _highlightedCells = new List<GameObject>();
    private Dictionary<GameObject, Material> _originalMaterials = new Dictionary<GameObject, Material>();
    private DebuffSystem _debuffs;

    public Vector2Int CurrentCoordinates => _currentCoord;
    private void OnEnable() => GameEvents.OnCancelCurrentAction += TryCancel;
    private void OnDisable() => GameEvents.OnCancelCurrentAction -= TryCancel;

    public void Initialize(HexGrid grid, Vector2Int startCoord, HexViewManager viewManager)
    {
        this.grid = grid;
        _currentCoord = startCoord;
        _viewManager = viewManager;
        _debuffs = GetComponent<DebuffSystem>();
    }

    private void Update()
    {
        if (_isSelecting && !_isMoving && Mouse.current.leftButton.wasPressedThisFrame)
        {
            HandleSelectionClick();
        }
    }

    public void BeginSelection(int range, Action<HexCell> callback)
    {
        if (_isMoving || grid == null) return;

        ClearSelectionState();
        _onCellSelectedCallback = callback;
        _isSelecting = true;

        HexCell startCell = grid.GetCell(_currentCoord);
        if (startCell == null) return;

        Queue<HexCell> frontier = new Queue<HexCell>();
        frontier.Enqueue(startCell);
        Dictionary<HexCell, int> distance = new Dictionary<HexCell, int> { { startCell, 0 } };

        while (frontier.Count > 0)
        {
            HexCell current = frontier.Dequeue();
            if (distance[current] >= range) continue;

            foreach (HexCell neighbor in current.neighbors)
            {
                if (!neighbor.canWalkOn || distance.ContainsKey(neighbor)) continue;

                Vector3 sPos = current.transform.position + Vector3.up * 0.5f;
                Vector3 ePos = neighbor.transform.position + Vector3.up * 0.5f;

                if (Physics.Linecast(sPos, ePos, obstacleLayer)) continue;

                Vector3 cellTop = neighbor.transform.position + Vector3.up * 1.0f;
                LayerMask excludeHex = ~LayerMask.GetMask("Interactable", "Special");
                Collider[] hits = Physics.OverlapSphere(cellTop, 0.3f, excludeHex, QueryTriggerInteraction.Collide);

                bool blocked = false;
                foreach (Collider hit in hits)
                {
                    if (!hit.CompareTag("Trap"))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                distance[neighbor] = distance[current] + 1;
                HighlightCell(neighbor.gameObject);
                frontier.Enqueue(neighbor);
            }
        }
    }

    private void HandleSelectionClick()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Camera activeCam = (_viewManager.fpCamera != null && _viewManager.fpCamera.enabled) ? _viewManager.fpCamera : _viewManager.worldCamera;
        Ray ray = activeCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, 500f))
        {
            HexCell cell = hit.collider.GetComponent<HexCell>() ?? hit.collider.GetComponentInParent<HexCell>();
            if (cell != null && _highlightedCells.Contains(cell.gameObject))
            {
                _onCellSelectedCallback?.Invoke(cell);
            }
        }
    }

    public void MoveTo(HexCell target)
    {
        List<HexCell> path = FindPath(_currentCoord, target.coordinates);
        if (path.Count > 0)
        {
            ClearSelectionState();
            StartCoroutine(FollowPath(path));
        }
    }

    private List<HexCell> FindPath(Vector2Int start, Vector2Int end)
    {
        HexCell startCell = grid.GetCell(start);
        HexCell endCell = grid.GetCell(end);
        if (startCell == null || endCell == null) return new List<HexCell>();

        Queue<HexCell> frontier = new Queue<HexCell>();
        frontier.Enqueue(startCell);
        Dictionary<HexCell, HexCell> cameFrom = new Dictionary<HexCell, HexCell> { { startCell, null } };

        while (frontier.Count > 0)
        {
            HexCell current = frontier.Dequeue();
            if (current.coordinates == end) break;

            foreach (HexCell next in current.neighbors)
            {
                if (!next.canWalkOn || cameFrom.ContainsKey(next)) continue;

                Vector3 sPos = current.transform.position + Vector3.up * 0.5f;
                Vector3 ePos = next.transform.position + Vector3.up * 0.5f;
                if (Physics.Linecast(sPos, ePos, obstacleLayer)) continue;

                Vector3 cellTop = next.transform.position + Vector3.up * 1.0f;
                LayerMask excludeHex = ~LayerMask.GetMask("Interactable", "Special");
                Collider[] hits = Physics.OverlapSphere(cellTop, 0.3f, excludeHex, QueryTriggerInteraction.Collide);
                bool blocked = false;
                foreach (Collider hit in hits)
                {
                    if (!hit.CompareTag("Trap"))
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                frontier.Enqueue(next);
                cameFrom[next] = current;
            }
        }

        List<HexCell> path = new List<HexCell>();
        if (!cameFrom.ContainsKey(endCell)) return path;
        for (HexCell curr = endCell; curr != null; curr = cameFrom[curr]) path.Add(curr);
        path.Reverse();
        return path;
    }

    private IEnumerator FollowPath(List<HexCell> path)
    {
        _isMoving = true;
        for (int i = 1; i < path.Count; i++)
        {
            if (_debuffs != null && _debuffs.IsStunned)
            {
                _isMoving = false;
                yield break;
            }

            Vector3 targetPos = path[i].transform.position + Vector3.up * heightOffset;
            Vector3 targetDir = (targetPos - transform.position);
            targetDir.y = 0;
            if (targetDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                while (Vector3.Distance(transform.position, targetPos) > 0.01f)
                {
                    if (_debuffs != null && _debuffs.IsStunned)
                    {
                        _isMoving = false;
                        yield break;
                    }
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                    yield return null;
                }
            }
            _currentCoord = path[i].coordinates;
        }
        _isMoving = false;
    }

    private void HighlightCell(GameObject cell)
    {
        if (_highlightedCells.Contains(cell)) return;
        Renderer rend = cell.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            if (!_originalMaterials.ContainsKey(cell)) _originalMaterials[cell] = rend.sharedMaterial;
            rend.material = highlightMaterial;
            cell.layer = LayerMask.NameToLayer("Special");
            _highlightedCells.Add(cell);
        }
    }

    private void TryCancel()
    {
        if (!IgnoreCancelAction) ClearSelectionState();

    }
    public void ClearSelectionState()
    {
        _isSelecting = false;
        _onCellSelectedCallback = null;
        foreach (var entry in _originalMaterials)
        {
            if (entry.Key != null)
            {
                Renderer rend = entry.Key.GetComponentInChildren<Renderer>();
                if (rend != null) rend.material = entry.Value;
                entry.Key.layer = LayerMask.NameToLayer("Interactable");
            }
        }
        _highlightedCells.Clear();
        _originalMaterials.Clear();
    }
}