using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoopScrollList4UGUI2 : MonoBehaviour
{
    // 滚动视图组件
    public ScrollRect scrollRect;
    // 内容区域，用于排列列表项
    public RectTransform content;
    // 列表项的预制体
    public GameObject itemPrefab;
    // 数据列表，假设列表项显示的数据为整数
    public List<int> dataList = new List<int>();
    // 列表项的高度
    public float itemHeight;
    // 可见区域内可容纳的最大列表项数量
    private int visibleItemCount;
    // 当前显示的列表项
    private List<GameObject> visibleItems = new List<GameObject>();
    // 列表项对象池，用于复用
    private Queue<GameObject> itemPool = new Queue<GameObject>();

    private float topY;
    private float bottomY;
    private int currentIdx;

    void Start()
    {
        // 初始化数据列表，这里简单生成一些数据
        for (int i = 0; i < 100; i++)
        {
            dataList.Add(i);
        }

        // 计算可见区域内可容纳的最大列表项数量
        visibleItemCount = Mathf.FloorToInt(scrollRect.viewport.rect.height / itemHeight) + 1;

        topY = content.anchoredPosition.y + itemHeight;

        currentIdx = 0;

        // 初始化显示的列表项
        InitializeVisibleItems();

        // 监听滚动事件
        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    // 初始化显示的列表项
    void InitializeVisibleItems()
    {
        for (int i = 0; i < visibleItemCount + 1; i++)
        {
            GameObject item = GetItemFromPool();
            item.transform.SetParent(content, false);
            item.SetActive(true);
            UpdateItem(item, i);
            visibleItems.Add(item);
        }
    }

    // 从对象池中获取列表项，若池为空则创建新的
    GameObject GetItemFromPool()
    {
        if (itemPool.Count > 0)
        {
            return itemPool.Dequeue();
        }
        else
        {
            return Instantiate(itemPrefab);
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
        float currentY = content.anchoredPosition.y;

        // 上移超出一个位置时的处理
        // 向上滚动
        if (currentY > itemHeight)
        {
            Transform firstItem = content.GetChild(0);
            firstItem.SetAsLastSibling();
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y - itemHeight);
        }
        // 下移低于一个位置时的处理
        else if (currentY < 0)
        {
            Transform lastItem = content.GetChild(content.childCount - 1);
            lastItem.SetAsFirstSibling();
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, content.anchoredPosition.y + itemHeight);
        }
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

        lastItem.transform.SetSiblingIndex(0);
        UpdateItem(lastItem, newIndex);
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

        firstItem.transform.SetSiblingIndex(visibleItems.Count - 1);
        UpdateItem(firstItem, newIndex);
    }

    // 获取第一个可见列表项的索引
    int GetFirstVisibleIndex()
    {
        return Mathf.FloorToInt(-content.anchoredPosition.y / itemHeight);
    }

    // 获取最后一个可见列表项的索引
    int GetLastVisibleIndex()
    {
        return GetFirstVisibleIndex() + visibleItemCount - 1;
    }

    // 当列表项不再可见时，将其放入对象池
    void RecycleItem(GameObject item)
    {
        item.SetActive(false);
        itemPool.Enqueue(item);
    }
}
