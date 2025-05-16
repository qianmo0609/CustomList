using UnityEngine;

public class SimpleList : SimpleScrollRect2
{
    public float speed = 1;

    protected override void Start()
    {
        this.onValueChanged.AddListener(this.OnScroll);
    }

    void OnScroll(Vector2 deltaPos)
    {
        if (m_Vertical) deltaPos.x = 0;
        else if (m_Horizontal) deltaPos.y = 0;

        for (int i = 0; i < m_Content.childCount; i++)
        {
            RectTransform item = m_Content.GetChild(i) as RectTransform;
            item.anchoredPosition += deltaPos * speed; 
        }
    }

    protected override void RenderItem(int index)
    {

    }
}
