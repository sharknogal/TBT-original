using UnityEngine;
using UnityEngine.InputSystem; 
using Cinemachine;

public class CameraController : MonoBehaviour
{
    private const float MIN_FOLLOW_OFFSET_Y = 2f;
    private const float MAX_FOLLOW_OFFSET_Y = 12f;
    
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
    
    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;

    private void Start()
    {
        if (cinemachineVirtualCamera == null)
        {
            Debug.LogError("CinemachineVirtualCamera is not assigned.");
            return;
        }
        
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        
        if (cinemachineTransposer != null)
        {
             targetFollowOffset = cinemachineTransposer.m_FollowOffset;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation(); // Mouse 객체만 전달
        HandleZoom();
    }

    private void HandleMovement()
    {
        // --- MOVEMENT ---
        Vector2 inputMoveDir = InputManager.Instance.GetCameraMoveVector();

        float moveSpeed = 10f;

        Vector3 moveVector = transform.forward * inputMoveDir.y + transform.right * inputMoveDir.x;
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }

    private void HandleRotation()
    {
        Vector3 rotationVector = new Vector3(0, 0, 0);

        // InputManager에서 입력값만 받아옵니다.
        rotationVector.y = InputManager.Instance.GetCameraRotateAmount();

        float rotationSpeed = 75f;
        transform.eulerAngles += rotationVector * rotationSpeed * Time.deltaTime;
    }

    private void HandleZoom()
    {
        if (cinemachineTransposer == null) return; 
        
        // [주의] 스크롤 값은 보통 120 단위로 큽니다. ZoomSpeed를 너무 크게 잡으면 확 튈 수 있습니다.
        // 여기서는 값을 좀 줄였습니다 (100f -> 2f). 테스트 해보고 조절하세요.
        float zoomSpeed = 1f; 
        float zoomLerpSpeed = 5f;
        
        // [수정] InputManager를 통해 스크롤 값을 가져옵니다.
        float scrollInput = InputManager.Instance.GetCameraZoomAmount();

        // 스크롤 값이 너무 클 경우를 대비해 정규화(0보다 크면 1, 작으면 -1)하여 사용할 수도 있습니다.
        if (scrollInput > 0) scrollInput = 1f;
        else if (scrollInput < 0) scrollInput = -1f;
        else scrollInput = 0f;

        // 줌 인(값이 양수)일 때 거리가 가까워져야 하므로(Y값 감소) 뺄셈 연산
        // 스크롤은 '이벤트'성이므로 Time.deltaTime을 곱하지 않는 것이 일반적이나,
        // 부드러운 Lerp를 위해 Target값을 변경하는 방식이므로 괜찮습니다.
        targetFollowOffset.y -= scrollInput * zoomSpeed; 
        
        // 최소/최대 높이 제한
        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_OFFSET_Y, MAX_FOLLOW_OFFSET_Y);
        
        // 실제 카메라 위치를 부드럽게 이동 (Lerp)
        cinemachineTransposer.m_FollowOffset = 
            Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * zoomLerpSpeed);
    }
}