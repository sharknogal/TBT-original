using System;
using System.Collections.Generic; // 리스트 사용을 위해 추가
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }

    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<bool> OnBusyChanged;
    public event EventHandler OnActionStarted;

    [SerializeField] private Unit selectedUnit;
    [SerializeField] private LayerMask unitLayerMask;

    private BaseAction selectedAction;
    private bool isBusy;
    
    // [추가됨] 살아있는 아군 유닛들을 관리할 리스트
    private List<Unit> selectedUnitList; 

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Duplicate UnitActionSystem! {transform} / {Instance}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 리스트 초기화
        selectedUnitList = new List<Unit>();
    }

    private void Start()
    {
        Unit.OnAnyUnitSpawned += Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;

        SetSelectedUnit(selectedUnit);
    }

    // [추가됨] 유닛이 스폰될 때 아군이라면 리스트에 추가
    private void Unit_OnAnyUnitSpawned(object sender, EventArgs e)
    {
        Unit unit = sender as Unit;
        if (unit != null && !unit.IsEnemy())
        {
            selectedUnitList.Add(unit);
        }
    }

    // [추가됨] 유닛이 죽었을 때 처리 로직
    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        Unit deadUnit = sender as Unit;
        
        // 죽은 유닛을 리스트에서 제거
        if (selectedUnitList.Contains(deadUnit))
        {
            selectedUnitList.Remove(deadUnit);
        }

        // 죽은 유닛이 현재 선택된 유닛이라면
        if (deadUnit == selectedUnit)
        {
            // 잠시 선택을 해제하여 안전하게 처리 (선택적)
            selectedUnit = null; 
            
            // 가장 가까운 유닛 찾아서 선택
            TrySelectNearestUnit(deadUnit.transform.position);
        }
    }

    // [추가됨] 가장 가까운 아군 유닛을 찾는 함수
    private void TrySelectNearestUnit(Vector3 deadUnitPosition)
    {
        // 남은 아군이 없다면 리턴
        if (selectedUnitList.Count == 0) return;

        Unit closestUnit = null;
        float shortestDistance = float.MaxValue;

        foreach (Unit unit in selectedUnitList)
        {
            // 거리 계산
            float distance = Vector3.Distance(deadUnitPosition, unit.transform.position);
            
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestUnit = unit;
            }
        }

        // 찾은 유닛이 있다면 선택
        if (closestUnit != null)
        {
            SetSelectedUnit(closestUnit);
        }
    }

    private void Update()
    {
        if (isBusy) return;

        if (!TurnSystem.Instance.IsPlayerTurn()) return;

        if (EventSystem.current.IsPointerOverGameObject()) return;

        if(TryHandleUnitSelection()) return;

        HandleSelectedAction();
    }

    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDown())
        {
            // 선택된 유닛이 없으면 실행하지 않음 (안전장치)
            if (selectedUnit == null) return;

            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPosition());

            if (!selectedAction.IsValidActionGridPosition(mouseGridPosition))
            {
                return;
            }

            if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
                return;
            }
            
            SetBusy();
            selectedAction.TakeAction(mouseGridPosition, ClearBusy);

            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SetBusy()
    {
        isBusy = true;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged?.Invoke(this, isBusy);
    }

    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMousScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayerMask) )
            {
                if (hit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        return false;
                    }
                    if (unit.IsEnemy())
                    {
                        return false;
                    }

                    SetSelectedUnit(unit);
                    return true;
                }
            }
        }

        return false;
    }

    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        
        if (selectedUnit != null)
        {
            SetSelectedAction(unit.GetAction<MoveAction>());

            OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetSelectedUnit() => selectedUnit;

    public BaseAction GetSelectedAction() => selectedAction;
    
    // 메모리 누수 방지를 위해 이벤트 구독 해제 권장
    private void OnDestroy()
    {
        Unit.OnAnyUnitSpawned -= Unit_OnAnyUnitSpawned;
        Unit.OnAnyUnitDead -= Unit_OnAnyUnitDead;
    }
}