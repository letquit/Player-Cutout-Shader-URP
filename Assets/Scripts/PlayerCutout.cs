using System;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 玩家遮挡镂空控制器
/// 在 URP 相机渲染前更新全局 Shader 参数，为 Cutout Shader 提供屏幕空间镂空中心与深度阈值
/// </summary>
public class PlayerCutout : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera m_camera;   // 执行镂空的主相机
    [SerializeField] private Transform m_target; // 需要保持可见的目标（通常为玩家根节点）

    // ===== Shader 全局属性 ID 缓存 =====
    // PropertyToID 比字符串查找快数个数量级，且结果可跨帧复用
    private static readonly int CenterId = Shader.PropertyToID("_CutoutCenter");  // 镂空中心 (viewport xy)
    private static readonly int DepthId = Shader.PropertyToID("_CutoutDepth");    // 深度阈值 (viewport z)
    private static readonly int EnabledId = Shader.PropertyToID("_CutoutEnabled"); // 镂空开关 (0/1)

    /// <summary>
    /// 启用时订阅 URP 相机渲染开始事件
    /// beginCameraRendering 在 Culling 之前触发，确保 Shader 参数在本帧绘制前就绪
    /// </summary>
    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += UpdateCutout;
    }

    /// <summary>
    /// 禁用时取消订阅并关闭全局镂空效果
    /// 防止组件禁用后残留参数导致其他物体持续被镂空
    /// </summary>
    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= UpdateCutout;
        Shader.SetGlobalFloat(EnabledId, 0f);
    }
    
    /// <summary>
    /// URP 相机渲染前回调：计算并上传本帧镂空参数
    /// </summary>
    /// <param name="context">URP 渲染上下文（此处未使用，但为委托签名必需）</param>
    /// <param name="camera">当前正在准备渲染的相机</param>
    private void UpdateCutout(ScriptableRenderContext context, Camera camera)
    { 
        // 【安全默认值】先关闭镂空，仅在通过所有校验后才重新开启
        // 避免多相机场景下非目标相机继承上一帧的镂空参数
        Shader.SetGlobalFloat(EnabledId, 0f);

        // 引用缺失或非目标相机直接跳过
        if (m_camera == null || m_target == null || camera != m_camera)
            return;

        // 将目标世界坐标转换为视口坐标 (x,y ∈ [0,1], z = 距相机距离)
        Vector3 targetViewportPosition = m_camera.WorldToViewportPoint(m_target.position);

        // 目标在相机近裁面之后才执行镂空
        // z <= nearClipPlane 表示目标在相机背后或贴脸，此时无需也不应镂空
        if (targetViewportPosition.z <= m_camera.nearClipPlane)
            return;
        
        // 上传镂空中心：仅使用 viewport xy，zw 置零
        // Shader 端用此坐标与片段 UV 做距离比较，确定镂空区域
        Shader.SetGlobalVector(CenterId, new Vector4(targetViewportPosition.x, targetViewportPosition.y, 0f, 0f));
        
        // 【深度基准校正】将目标 y 坐标归零后重新投影
        // 目的：以玩家"脚底平面"作为深度比较基准，而非角色模型中心
        // 这样当玩家站在墙后时，墙体从脚底高度开始镂空，避免头顶上方不必要的挖孔
        Vector3 targetPositionWithoutHeight = m_target.position;
        targetPositionWithoutHeight.y = 0f;

        float targetDepthWithoutHeight = m_camera.WorldToViewportPoint(targetPositionWithoutHeight).z;
        
        // 上传深度阈值：Shader 端仅对深度 > 此值的片段执行镂空
        // 即只有位于玩家"后方"（更远）的墙壁才会被挖孔，玩家前方的物体不受影响
        Shader.SetGlobalFloat(DepthId, targetDepthWithoutHeight);
        
        // 所有校验通过，启用本帧镂空
        Shader.SetGlobalFloat(EnabledId, 1f);
    }
}