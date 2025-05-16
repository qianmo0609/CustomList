using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 简化版滚动组件，保留核心拖拽和惯性滚动功能
public class SimpleScrollRect : UIBehaviour,
    //IInitializePotentialDragHandler, // 预拖拽接口
    IBeginDragHandler,               // 开始拖拽
    IEndDragHandler,                 // 结束拖拽
    IDragHandler                    // 拖拽中
    //IScrollHandler                   // 滚轮滚动
{
    [SerializeField]
    private RectTransform m_Content; // 滚动内容区域（需在Inspector中指定）

    [SerializeField]
    [Tooltip("允许水平滚动")]
    private bool m_Horizontal = true;

    [SerializeField]
    [Tooltip("允许垂直滚动")]
    private bool m_Vertical = true;

    //[SerializeField]
    //[Range(0, 1)]
    //[Tooltip("边界弹性系数（0=无弹性，1=强弹性）")]
    //private float m_Elasticity = 0.1f;

    //[SerializeField]
    //[Tooltip("启用惯性滚动")]
    //private bool m_Inertia = true;

    [SerializeField]
    [Tooltip("惯性衰减速度（值越小停止越快）")]
    private float m_DecelerationRate = 0.135f;

    [SerializeField]
    [Tooltip("鼠标滚轮灵敏度")]
    private float m_ScrollSensitivity = 1.0f;

    // 运行时变量
    private Vector2 m_PointerStartLocalCursor; // 拖拽起始点本地坐标
    private Vector2 m_ContentStartPosition;     // 拖拽开始时内容的位置
    //private Vector2 m_Velocity;                 // 当前滚动速度
    private bool m_Dragging;                    // 是否正在拖拽
    private Bounds m_ContentBounds;            // 内容边界
    private Bounds m_ViewBounds;               // 视口边界

    /// <summary>
    /// 更新视口和内容的边界信息
    /// </summary>
    private void UpdateBounds()
    {
        // 获取当前对象的RectTransform作为视口
        var viewRect = GetComponent<RectTransform>();

        // 计算视口边界（基于视口矩形的中心点和尺寸）
        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        // 获取内容的四个角的世界坐标
        Vector3[] corners = new Vector3[4];
        m_Content.GetWorldCorners(corners);

        // 将内容边界转换到视口本地坐标系
        m_ContentBounds = GetBounds(corners, viewRect);
    }

    /// <summary>
    /// 计算内容在视口坐标系中的边界
    /// </summary>
    private Bounds GetBounds(Vector3[] corners, RectTransform viewRect)
    {
        var viewWorldToLocal = viewRect.worldToLocalMatrix;
        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // 遍历内容的四个角点，找到最小/最大坐标
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = viewWorldToLocal.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        // 创建边界并封装所有点
        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }

    // 开始拖拽事件处理
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsActive()) return;

        // 将屏幕坐标转换为视口本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out m_PointerStartLocalCursor
        );

        // 记录拖拽开始时内容的位置
        m_ContentStartPosition = m_Content.anchoredPosition;
        m_Dragging = true; // 标记拖拽状态
    }

    // 拖拽过程中事件处理
    public void OnDrag(PointerEventData eventData)
    {
        if (!m_Dragging || !IsActive()) return;

        Vector2 localCursor;
        // 获取当前鼠标位置在视口中的本地坐标
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localCursor
        )) return;

        // 计算位置偏移量
        Vector2 pointerDelta = localCursor - m_PointerStartLocalCursor;
        Vector2 position = m_ContentStartPosition + pointerDelta;

        //// 计算边界限制偏移
        Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
        position += offset;

        // 应用弹性效果
        if (offset != Vector2.zero)
        {
            position.x -= RubberDelta(offset.x, m_ViewBounds.size.x);
            position.y -= RubberDelta(offset.y, m_ViewBounds.size.y);
        }

        // 设置新位置
        SetContentPosition(position);
    }

    // 每帧更新（处理惯性）
    //private void LateUpdate()
    //{
    //    if (!m_Content) return;
    //    this.m_Velocity = Vector2.zero;
    //    // 拖拽中计算速度
    //    if (m_Dragging && m_Inertia)
    //    {
    //        Vector2 newVelocity = (m_Content.anchoredPosition - m_ContentStartPosition) / Time.deltaTime;
    //        m_Velocity = Vector2.Lerp(m_Velocity, newVelocity, Time.deltaTime * 10);
    //    }

    //    // 非拖拽状态应用惯性
    //    if (!m_Dragging && m_Velocity != Vector2.zero)
    //    {
    //        m_Velocity *= Mathf.Pow(m_DecelerationRate, Time.deltaTime);
    //        SetContentPosition(m_Content.anchoredPosition + m_Velocity * Time.deltaTime);
    //    }
    //}

    /// <summary>
    /// 计算位置偏移限制
    /// </summary>
    private Vector2 CalculateOffset(Vector2 delta)
    {
        Vector2 offset = Vector2.zero;

        // 水平方向边界检测
        if (m_Horizontal)
        {
            float projectedMax = m_ContentBounds.max.x + delta.x;
            if (projectedMax < m_ViewBounds.max.x)
                offset.x = m_ViewBounds.max.x - projectedMax;
            else if (m_ContentBounds.min.x + delta.x > m_ViewBounds.min.x)
                offset.x = m_ViewBounds.min.x - (m_ContentBounds.min.x + delta.x);
        }

        // 垂直方向边界检测
        if (m_Vertical)
        {
            float projectedMin = m_ContentBounds.min.y + delta.y;
            if (projectedMin > m_ViewBounds.min.y)
                offset.y = m_ViewBounds.min.y - projectedMin;
            else if (m_ContentBounds.max.y + delta.y < m_ViewBounds.max.y)
                offset.y = m_ViewBounds.max.y - (m_ContentBounds.max.y + delta.y);
        }

        return offset;
    }

    /// <summary>
    /// 设置内容位置（带轴向限制）
    /// </summary>
    private void SetContentPosition(Vector2 position)
    {
        // 根据设置锁定轴向
        if (!m_Horizontal) position.x = m_Content.anchoredPosition.x;
        if (!m_Vertical) position.y = m_Content.anchoredPosition.y;

        // 应用新位置
        m_Content.anchoredPosition = position;
        UpdateBounds(); // 刷新边界信息
    }

    /// <summary>
    /// 弹性效果计算（越界时产生弹性阻尼）
    /// </summary>
    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - 1 / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1)) * viewSize * Mathf.Sign(overStretching);
    }

    // 接口方法实现
    //public void OnInitializePotentialDrag(PointerEventData eventData) => m_Velocity = Vector2.zero;
    public void OnEndDrag(PointerEventData eventData) => m_Dragging = false;
    public void OnScroll(PointerEventData data)
    {
        // 滚轮滚动处理（可参考原版实现添加）
    }
}
