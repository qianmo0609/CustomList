using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 循环列表组件类
public class LoopScrollList4UGUI : MonoBehaviour
{
    // 滚动视图组件
    public ScrollRect scrollRect;
    // 内容区域，用于排列列表项
    public RectTransform content;
    // 列表项的预制体
    public GameObject itemPrefab;
    // 列表项的数量
    public int itemCount;
    // 列表项的高度
    public float itemHeight;
    // 可见区域内可容纳的最大列表项数量
    private int visibleItemCount;
    // 当前显示的列表项
    private List<GameObject> visibleItems = new List<GameObject>();
    // 数据列表
    private List<int> dataList = new List<int>();
    // 上一次滚动位置
    private float lastScrollPosition;

    void Start()
    {
        // 初始化数据列表
        for (int i = 0; i < itemCount; i++)
        {
            dataList.Add(i);
        }

        // 计算可见区域内可容纳的最大列表项数量
        visibleItemCount = Mathf.FloorToInt(scrollRect.viewport.rect.height / itemHeight);

        // 初始化显示的列表项
        InitializeVisibleItems();

        // 监听滚动事件
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    // 初始化显示的列表项
    void InitializeVisibleItems()
    {
        for (int i = 0; i < visibleItemCount; i++)
        {
            GameObject item = Instantiate(itemPrefab, content);
            item.transform.localPosition = new Vector3(0, -i * itemHeight, 0);
            UpdateItem(item, i);
            visibleItems.Add(item);
        }
    }

    // 更新列表项的显示内容
    void UpdateItem(GameObject item, int index)
    {
        if (index < dataList.Count)
        {
            Text text = item.GetComponentInChildren<Text>();
            if (text != null)
            {
                text.text = "Item " + dataList[index].ToString();
            }
        }
    }

    // 滚动事件处理函数
    void OnScroll(Vector2 scrollPosition)
    {
        Debug.Log(content.anchoredPosition.y);
        //float currentScrollPosition = scrollRect.verticalNormalizedPosition;
        //if (currentScrollPosition > lastScrollPosition)
        //{
        //    // 向上滚动
        //    while (content.anchoredPosition.y > itemHeight)
        //    {
        //        MoveLastItemToTop();
        //        content.anchoredPosition -= new Vector2(0, itemHeight);
        //    }
        //}
        //else if (currentScrollPosition < lastScrollPosition)
        //{
        //    // 向下滚动
        //    while (content.anchoredPosition.y < -itemHeight)
        //    {
        //        MoveFirstItemToBottom();
        //        content.anchoredPosition += new Vector2(0, itemHeight);
        //    }
        //}
        //lastScrollPosition = currentScrollPosition;
    }

    // 将最后一个列表项移动到顶部
    void MoveLastItemToTop()
    {
        GameObject lastItem = visibleItems[visibleItems.Count - 1];
        visibleItems.RemoveAt(visibleItems.Count - 1);
        visibleItems.Insert(0, lastItem);
        int newIndex = GetFirstVisibleIndex() - 1;
        if (newIndex < 0)
        {
            newIndex = dataList.Count - 1;
        }
        lastItem.transform.localPosition = new Vector3(0, 0, 0);
        UpdateItem(lastItem, newIndex);
        for (int i = 1; i < visibleItems.Count; i++)
        {
            visibleItems[i].transform.localPosition -= new Vector3(0, itemHeight, 0);
        }
    }

    // 将第一个列表项移动到底部
    void MoveFirstItemToBottom()
    {
        GameObject firstItem = visibleItems[0];
        visibleItems.RemoveAt(0);
        visibleItems.Add(firstItem);
        int newIndex = GetLastVisibleIndex() + 1;
        if (newIndex >= dataList.Count)
        {
            newIndex = 0;
        }
        firstItem.transform.localPosition = new Vector3(0, -visibleItemCount * itemHeight, 0);
        UpdateItem(firstItem, newIndex);
        for (int i = 0; i < visibleItems.Count - 1; i++)
        {
            visibleItems[i].transform.localPosition += new Vector3(0, itemHeight, 0);
        }
    }

    // 获取第一个可见列表项的索引
    int GetFirstVisibleIndex()
    {
        float offset = content.anchoredPosition.y / itemHeight;
        int index = Mathf.FloorToInt(offset);
        return index;
    }

    // 获取最后一个可见列表项的索引
    int GetLastVisibleIndex()
    {
        return GetFirstVisibleIndex() + visibleItemCount - 1;
    }
}
