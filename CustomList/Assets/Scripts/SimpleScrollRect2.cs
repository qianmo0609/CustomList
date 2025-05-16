using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.ScrollRect;

// 简化版滚动组件，保留核心拖拽和惯性滚动功能
public class SimpleScrollRect2 : UIBehaviour,
    IBeginDragHandler,               // 开始拖拽
    IEndDragHandler,                 // 结束拖拽
    IDragHandler                    // 拖拽中
{
    [SerializeField]
    protected RectTransform m_Content; // 滚动内容区域（需在Inspector中指定）

    [SerializeField]
    [Tooltip("允许水平滚动")]
    protected bool m_Horizontal = true;

    [SerializeField]
    [Tooltip("允许垂直滚动")]
    protected bool m_Vertical = true;

    // 运行时变量
    private Vector2 m_PointerStartLocalCursor; // 拖拽起始点本地坐标
    private bool m_Dragging;                    // 是否正在拖拽


    // 滚动位置变化事件
    [SerializeField]
    private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
    public ScrollRectEvent onValueChanged
    {
        get => m_OnValueChanged;
        set => m_OnValueChanged = value;
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
        m_OnValueChanged?.Invoke(pointerDelta.normalized);
    }

    protected virtual void RenderItem(int index){}

    public void OnEndDrag(PointerEventData eventData) => m_Dragging = false;
}
