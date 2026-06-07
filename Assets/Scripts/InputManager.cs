using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Duplicate InputManager! {transform} / {Instance}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public Vector2 GetMousScreenPosition()
    {
        return Mouse.current.position.ReadValue();
    }

    public bool IsMouseButtonDown()
    {
        return Mouse.current.leftButton.wasPressedThisFrame;
    }

    public Vector2 GetCameraMoveVector()
    {
        // --- MOVEMENT ---
        // Vector3 대신 Vector2를 사용하고 있으므로 이대로 둡니다.
        Vector2 inputMoveDir = new Vector2(0, 0); 
    
        // null 체크 추가: 혹시 키보드 입력 장치가 연결되어 있지 않은 경우를 대비합니다.
        Keyboard currentKeyboard = Keyboard.current;

        if (currentKeyboard == null) 
        {
            return Vector2.zero;
        }

        // Keyboard.current를 통해 키 상태에 접근합니다.
        if (currentKeyboard.wKey.isPressed) inputMoveDir.y += 1f;
        if (currentKeyboard.sKey.isPressed) inputMoveDir.y -= 1f;
        if (currentKeyboard.aKey.isPressed) inputMoveDir.x -= 1f;
        if (currentKeyboard.dKey.isPressed) inputMoveDir.x += 1f;

        return inputMoveDir;
    }

    public float GetCameraRotateAmount()
    {
        // 1. 마우스 연결 확인
        if (Mouse.current == null) return 0f;

        // 2. 휠 버튼(Middle Button)이 눌려있는지 확인
        if (Mouse.current.middleButton.isPressed)
        {
            // 3. 마우스 델타(이동량) 중 X축 값만 읽어옴
            // (마우스를 좌우로 움직일 때 카메라를 Y축 기준으로 회전시키기 위함)
            return Mouse.current.delta.x.ReadValue();
        }

        // 눌리지 않았으면 회전 없음
        return 0f;
    }

    public float GetCameraZoomAmount()
    {
        if (Mouse.current == null) return 0f;
        
        // scroll.y는 보통 위로 휠을 굴리면 120, 아래로 굴리면 -120 등의 값이 나옵니다.
        // 하드웨어마다 값이 다를 수 있으므로 0보다 크냐 작냐로 판단하거나,
        // 필요하다면 Mathf.Clamp나 Normalize를 해서 내보낼 수도 있습니다.
        // 여기서는 Raw 값을 그대로 내보냅니다.
        return Mouse.current.scroll.y.ReadValue();
    }

}
