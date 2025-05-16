using UnityEngine;

public class BezierList : SimpleScrollRect2
{
    public Camera Camera;
    [Header("贝塞尔曲线参数")]
    public RectTransform startPoint;
    public RectTransform ctrlPoint;
    public RectTransform endPoint;

    private Vector2 startPointAnchoredPosition;
    private Vector2 ctrlPointAnchoredPosition;
    private Vector2 endPointAnchoredPosition;
    private float start2EndDistance;

    [Header("移动速度")]
    public float speed = 1;

    protected override void Start()
    {
        this.onValueChanged.AddListener(this.OnScroll);
        startPointAnchoredPosition = TransformScrennPoint(startPoint.anchoredPosition);
        ctrlPointAnchoredPosition = TransformScrennPoint(ctrlPoint.anchoredPosition);
        endPointAnchoredPosition = TransformScrennPoint(endPoint.anchoredPosition);
        start2EndDistance = Vector2.Distance(startPointAnchoredPosition,endPointAnchoredPosition);
    }

    void OnScroll(Vector2 deltaPos)
    {
        if (m_Vertical) deltaPos.x = 0;
        else if (m_Horizontal) deltaPos.y = 0;

        for (int i = 0; i < m_Content.childCount; i++)
        {
            RectTransform item = m_Content.GetChild(i) as RectTransform;
            item.anchoredPosition += deltaPos * speed;
            item.anchoredPosition = new Vector2(this.BezierScroll((item.anchoredPosition.y - startPointAnchoredPosition.y) / start2EndDistance),item.anchoredPosition.y);
        }
    }

    protected override void RenderItem(int index){}

 
    public float BezierScroll(float param)
    {
        return (Mathf.Pow(1 - param, 2) * startPointAnchoredPosition + 2 * (1 - param) * param * ctrlPointAnchoredPosition + Mathf.Pow(param, 2) * endPointAnchoredPosition).x;
    }

    Vector2 TransformScrennPoint(Vector2 screenPos)
    {
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_Content,
            screenPos,
            Camera,
            out localPosition
        );

        return localPosition;
    }
}
