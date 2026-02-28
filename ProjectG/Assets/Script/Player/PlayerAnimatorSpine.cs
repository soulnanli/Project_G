using System;
using Spine.Unity;
using TarodevController;
using UnityEngine;

public class PlayerAnimatorSpine : MonoBehaviour
{
    private SkeletonAnimation _skeletonAnimation;
    private IPlayerController _player;
    private bool _isMoving = false;
    private bool _isFiring = false;

    public string targetBoneName = "target";
    void Awake()
    {
        _skeletonAnimation = GetComponent<SkeletonAnimation>();
        _player = GetComponent<IPlayerController>();

    }

    private void Start()
    {
        _skeletonAnimation.AnimationState.SetAnimation(0, "wait", true);
    }

    private void OnEnable()
    {
        _player.Fired += OnFire;
        _player.Moved += OnMove;
    }

    private void OnDisable()
    {
        _player.Fired -= OnFire;
        _player.Moved -= OnMove;
    }

    // Update is called once per frame
    void Update()
    {
        AttackAim();
        CheckStopMoving();
        CheckStopAttacking();
    }

    void OnFire()
    {
        if (_isFiring) return;
        Debug.Log("FIRE");
        _skeletonAnimation.AnimationState.SetAnimation(1, "Attack_Body", true);
        _isFiring = true;

    }

    void OnMove()
    {
        if (!_isMoving)
        {
            Debug.Log("MOVE");
            _skeletonAnimation.AnimationState.SetAnimation(0, "move", true);
            _isMoving = true;
        }
    }

    void CheckStopMoving()
    {
        // 如果正在移动，但水平输入为0，则停止移动动画
        if (_isMoving && Mathf.Abs(_player.FrameInput.x) < 0.01f)
        {
            _isMoving = false;
            _skeletonAnimation.AnimationState.SetAnimation(0, "wait", true);
            Debug.Log("STOP");
        }
    }
    
    void CheckStopAttacking()
    {
        if (!_player.IsFiring)
        {
            _isFiring= false;
            _skeletonAnimation.AnimationState.ClearTrack(1);
        }
    }
    void AttackAim()
    {
        if (_skeletonAnimation == null || _skeletonAnimation.Skeleton == null)
            return;

        // 获取鼠标屏幕位置并转换为世界坐标
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0; // 确保 Z 轴为 0

        // 角色翻转：如果鼠标在角色左边，则水平翻转
        bool isFlipped = mouseWorldPosition.x < transform.position.x;
        if (isFlipped)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // 查找目标骨骼
        var targetBone = _skeletonAnimation.Skeleton.FindBone(targetBoneName);
        if (targetBone != null)
        {
            // 计算鼠标位置相对于角色位置的偏移
            float offsetX = mouseWorldPosition.x - transform.position.x;
            float offsetY = mouseWorldPosition.y - transform.position.y;

            // 如果角色被翻转，需要反转 X 偏移量以保持正确的方向
            if (isFlipped)
            {
                offsetX = -offsetX;
            }

            // 将偏移设置为骨骼的本地坐标
            targetBone.X = offsetX;
            targetBone.Y = offsetY;

            // 更新骨骼的世界变换
            _skeletonAnimation.Skeleton.UpdateWorldTransform();
        }
    }
}
