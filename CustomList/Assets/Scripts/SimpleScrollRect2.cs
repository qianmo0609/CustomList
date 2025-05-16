using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.UI.ScrollRect;

// �򻯰�������������������ק�͹��Թ�������
public class SimpleScrollRect2 : UIBehaviour,
    IBeginDragHandler,               // ��ʼ��ק
    IEndDragHandler,                 // ������ק
    IDragHandler                    // ��ק��
{
    [SerializeField]
    protected RectTransform m_Content; // ����������������Inspector��ָ����

    [SerializeField]
    [Tooltip("����ˮƽ����")]
    protected bool m_Horizontal = true;

    [SerializeField]
    [Tooltip("����ֱ����")]
    protected bool m_Vertical = true;

    // ����ʱ����
    private Vector2 m_PointerStartLocalCursor; // ��ק��ʼ�㱾������
    private bool m_Dragging;                    // �Ƿ�������ק


    // ����λ�ñ仯�¼�
    [SerializeField]
    private ScrollRectEvent m_OnValueChanged = new ScrollRectEvent();
    public ScrollRectEvent onValueChanged
    {
        get => m_OnValueChanged;
        set => m_OnValueChanged = value;
    }

    // ��ʼ��ק�¼�����
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsActive()) return;

        // ����Ļ����ת��Ϊ�ӿڱ�������
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out m_PointerStartLocalCursor
        );
        m_Dragging = true; // �����ק״̬
    }

    // ��ק�������¼�����
    public void OnDrag(PointerEventData eventData)
    {
        if (!m_Dragging || !IsActive()) return;

        Vector2 localCursor;
        // ��ȡ��ǰ���λ�����ӿ��еı�������
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localCursor
        )) return;

        // ����λ��ƫ����
        Vector2 pointerDelta = localCursor - m_PointerStartLocalCursor;
        m_OnValueChanged?.Invoke(pointerDelta.normalized);
    }

    protected virtual void RenderItem(int index){}

    public void OnEndDrag(PointerEventData eventData) => m_Dragging = false;
}
