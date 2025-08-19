using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteButton : MonoBehaviour
{
    private AudioManager sound;
    
    // 클릭할 때 호출할 이벤트
    public UnityEngine.Events.UnityEvent onClick;

    private void Start()
    {
        sound = AudioManager.Instance;
    }

    void Update()
    {
        // 마우스 왼쪽 버튼을 클릭했을 때
        if (Input.GetMouseButtonDown(0))
        {
            // 마우스 클릭 위치를 World 좌표로 변환
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            // 클릭한 위치에 Collider가 있는지 체크
            Collider2D collider = Physics2D.OverlapPoint(mousePosition);
            if (collider != null && collider.gameObject == gameObject)
            {
                sound.PlayButtonSound();
                // 클릭한 스프라이트가 자기 자신일 경우 onClick 이벤트 발생
                onClick.Invoke();
            }
        }
    }
}
