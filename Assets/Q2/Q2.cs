using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
录屏路径：`Assets/Q2/CandyCrush.MP4`

请仔细观察根目录中提供的知名消除游戏 Candy Crush 录屏中，选关界面对话框 Play 按钮的动画效果，请复刻这一效果，使用代码实现或者 Animation 均可，动画包括：
1. 按钮出现
2. 按钮按下
3. 按钮弹起
*/

public class Q2 : MonoBehaviour
{
    [SerializeField]
    private Button button = null;

    private Animator _animator;
    private static readonly int Show = Animator.StringToHash("Show");
    private static readonly int TouchDown = Animator.StringToHash("TouchDown");
    private static readonly int TouchUp = Animator.StringToHash("TouchUp");
    
    private void Start()
    {
        _animator = button.GetComponent<Animator>();
    }

    public void OnShowBtnClick()
    {
        // TODO: 请在此处开始作答
        if (_animator!=null)
        {
            _animator.Play(Show);
        }
    }

    public void OnTouchDownBtnClick()
    {
        // TODO: 请在此处开始作答
        if (_animator!=null)
        {
            _animator.Play(TouchDown);
        }
    }

    public void OnTouchUpBtnClick()
    {
        // TODO: 请在此处开始作答
        if (_animator!=null)
        {
            _animator.Play(TouchUp);
        }
    }
}
