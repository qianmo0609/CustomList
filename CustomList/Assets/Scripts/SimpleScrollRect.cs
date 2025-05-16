using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// �򻯰�������������������ק�͹��Թ�������
public class SimpleScrollRect : UIBehaviour,
    //IInitializePotentialDragHandler, // Ԥ��ק�ӿ�
    IBeginDragHandler,               // ��ʼ��ק
    IEndDragHandler,                 // ������ק
    IDragHandler                    // ��ק��
    //IScrollHandler                   // ���ֹ���
{
    [SerializeField]
    private RectTransform m_Content; // ����������������Inspector��ָ����

    [SerializeField]
    [Tooltip("����ˮƽ����")]
    private bool m_Horizontal = true;

    [SerializeField]
    [Tooltip("����ֱ����")]
    private bool m_Vertical = true;

    //[SerializeField]
    //[Range(0, 1)]
    //[Tooltip("�߽絯��ϵ����0=�޵��ԣ�1=ǿ���ԣ�")]
    //private float m_Elasticity = 0.1f;

    //[SerializeField]
    //[Tooltip("���ù��Թ���")]
    //private bool m_Inertia = true;

    [SerializeField]
    [Tooltip("����˥���ٶȣ�ֵԽСֹͣԽ�죩")]
    private float m_DecelerationRate = 0.135f;

    [SerializeField]
    [Tooltip("������������")]
    private float m_ScrollSensitivity = 1.0f;

    // ����ʱ����
    private Vector2 m_PointerStartLocalCursor; // ��ק��ʼ�㱾������
    private Vector2 m_ContentStartPosition;     // ��ק��ʼʱ���ݵ�λ��
    //private Vector2 m_Velocity;                 // ��ǰ�����ٶ�
    private bool m_Dragging;                    // �Ƿ�������ק
    private Bounds m_ContentBounds;            // ���ݱ߽�
    private Bounds m_ViewBounds;               // �ӿڱ߽�

    /// <summary>
    /// �����ӿں����ݵı߽���Ϣ
    /// </summary>
    private void UpdateBounds()
    {
        // ��ȡ��ǰ�����RectTransform��Ϊ�ӿ�
        var viewRect = GetComponent<RectTransform>();

        // �����ӿڱ߽磨�����ӿھ��ε����ĵ�ͳߴ磩
        m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);

        // ��ȡ���ݵ��ĸ��ǵ���������
        Vector3[] corners = new Vector3[4];
        m_Content.GetWorldCorners(corners);

        // �����ݱ߽�ת�����ӿڱ�������ϵ
        m_ContentBounds = GetBounds(corners, viewRect);
    }

    /// <summary>
    /// �����������ӿ�����ϵ�еı߽�
    /// </summary>
    private Bounds GetBounds(Vector3[] corners, RectTransform viewRect)
    {
        var viewWorldToLocal = viewRect.worldToLocalMatrix;
        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        // �������ݵ��ĸ��ǵ㣬�ҵ���С/�������
        for (int j = 0; j < 4; j++)
        {
            Vector3 v = viewWorldToLocal.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        // �����߽粢��װ���е�
        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
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

        // ��¼��ק��ʼʱ���ݵ�λ��
        m_ContentStartPosition = m_Content.anchoredPosition;
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
        Vector2 position = m_ContentStartPosition + pointerDelta;

        //// ����߽�����ƫ��
        Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
        position += offset;

        // Ӧ�õ���Ч��
        if (offset != Vector2.zero)
        {
            position.x -= RubberDelta(offset.x, m_ViewBounds.size.x);
            position.y -= RubberDelta(offset.y, m_ViewBounds.size.y);
        }

        // ������λ��
        SetContentPosition(position);
    }

    // ÿ֡���£�������ԣ�
    //private void LateUpdate()
    //{
    //    if (!m_Content) return;
    //    this.m_Velocity = Vector2.zero;
    //    // ��ק�м����ٶ�
    //    if (m_Dragging && m_Inertia)
    //    {
    //        Vector2 newVelocity = (m_Content.anchoredPosition - m_ContentStartPosition) / Time.deltaTime;
    //        m_Velocity = Vector2.Lerp(m_Velocity, newVelocity, Time.deltaTime * 10);
    //    }

    //    // ����ק״̬Ӧ�ù���
    //    if (!m_Dragging && m_Velocity != Vector2.zero)
    //    {
    //        m_Velocity *= Mathf.Pow(m_DecelerationRate, Time.deltaTime);
    //        SetContentPosition(m_Content.anchoredPosition + m_Velocity * Time.deltaTime);
    //    }
    //}

    /// <summary>
    /// ����λ��ƫ������
    /// </summary>
    private Vector2 CalculateOffset(Vector2 delta)
    {
        Vector2 offset = Vector2.zero;

        // ˮƽ����߽���
        if (m_Horizontal)
        {
            float projectedMax = m_ContentBounds.max.x + delta.x;
            if (projectedMax < m_ViewBounds.max.x)
                offset.x = m_ViewBounds.max.x - projectedMax;
            else if (m_ContentBounds.min.x + delta.x > m_ViewBounds.min.x)
                offset.x = m_ViewBounds.min.x - (m_ContentBounds.min.x + delta.x);
        }

        // ��ֱ����߽���
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
    /// ��������λ�ã����������ƣ�
    /// </summary>
    private void SetContentPosition(Vector2 position)
    {
        // ����������������
        if (!m_Horizontal) position.x = m_Content.anchoredPosition.x;
        if (!m_Vertical) position.y = m_Content.anchoredPosition.y;

        // Ӧ����λ��
        m_Content.anchoredPosition = position;
        UpdateBounds(); // ˢ�±߽���Ϣ
    }

    /// <summary>
    /// ����Ч�����㣨Խ��ʱ�����������ᣩ
    /// </summary>
    private static float RubberDelta(float overStretching, float viewSize)
    {
        return (1 - 1 / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1)) * viewSize * Mathf.Sign(overStretching);
    }

    // �ӿڷ���ʵ��
    //public void OnInitializePotentialDrag(PointerEventData eventData) => m_Velocity = Vector2.zero;
    public void OnEndDrag(PointerEventData eventData) => m_Dragging = false;
    public void OnScroll(PointerEventData data)
    {
        // ���ֹ��������ɲο�ԭ��ʵ����ӣ�
    }
}
