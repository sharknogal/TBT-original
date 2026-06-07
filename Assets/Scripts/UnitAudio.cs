using UnityEngine;
using System;

public class UnitAudio : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private ShootAction shootAction;
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip shootSoundClip;

    private void Awake()
    {
        // 만약 인스펙터에서 할당하지 않았다면 자동으로 찾습니다.
        if (shootAction == null) shootAction = GetComponent<ShootAction>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // ShootAction의 OnShoot 이벤트에 함수를 등록합니다.
        shootAction.OnShoot += ShootAction_OnShoot;
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 연결을 해제하는 습관을 들이는 것이 좋습니다.
        if (shootAction != null)
        {
            shootAction.OnShoot -= ShootAction_OnShoot;
        }
    }

    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        // 이벤트가 발생했을 때 실행될 코드
        PlayShootSound();
    }

    private void PlayShootSound()
    {
        if (shootSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(shootSoundClip);
        }
    }
}