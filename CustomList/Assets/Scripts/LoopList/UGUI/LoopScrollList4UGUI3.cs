using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LoopScrollList4UGUI3 : MonoBehaviour
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

    Transform topItem = null;  //最顶上的一个物体
    Transform bottomItem = null; //最底的一个物体

    int topIdx = -1;

    int currentDataoffSet = 0; //代表列表显示第一项的实际索引也就是与数据真实第一项的有效偏移

    float contentAnchoredPositionYMax = 0;
    float contentAnchoredPositionYMin = 0;

    void Start()
    {
        // 初始化数据列表，这里简单生成一些数据
        for (int i = 0; i < 100; i++)
        {
            dataList.Add(i);
        }
        // 计算可见区域内可容纳的最大列表项数量
        visibleItemCount = Mathf.FloorToInt(scrollRect.viewport.rect.height / itemHeight) + 1;
        // 初始化显示的列表项
        InitializeVisibleItems();
        // 监听滚动事件
        scrollRect.onValueChanged.AddListener(OnScroll);
        topY = content.position.y + itemHeight / 2;//809.77f;
        bottomY = content.position.y - visibleItemCount * itemHeight - itemHeight / 2;//265.77f;
        content.sizeDelta = new Vector2(content.rect.width,dataList.Count * itemHeight);
        topItem = content.GetChild(0);
        bottomItem = content.GetChild(visibleItemCount);
        topIdx = 0;
        contentAnchoredPositionYMin = content.anchoredPosition.y;
        contentAnchoredPositionYMax = content.anchoredPosition.y + (dataList.Count - visibleItemCount)* itemHeight + itemHeight - scrollRect.viewport.rect.height % itemHeight;
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
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Min(contentAnchoredPositionYMax, Mathf.Max(contentAnchoredPositionYMin, content.anchoredPosition.y)));
        
        // 上移超出一个位置时的处理
        //向上滚动
        if (topItem.position.y >= topY)
        {
            //超出顶部的距离可以有几个itemHeight的距离
            int times = Mathf.CeilToInt((topItem.position.y - topY) / itemHeight);
            //超出的距离中不满一个itemHeight的部分，需要将这部分在后续计算中处理
            float offset = (topItem.position.y - topY) % itemHeight;
            for (int i = 0; i < times; i++)
            {
                topItem = content.GetChild(topIdx);
                topItem.position = new Vector3(topItem.position.x, bottomY + (times - i - 1) * itemHeight + offset, topItem.position.z);
                topIdx++;
                topIdx %= (visibleItemCount + 1);
                topItem = content.GetChild(topIdx);
                bottomItem = content.GetChild((topIdx + visibleItemCount) % (visibleItemCount + 1));
                currentDataoffSet++;
                //根据currentDataoffSet来更新Item的具体信息
                UpdateItem(bottomItem.gameObject, currentDataoffSet + visibleItemCount);
            }
        }
        // 下移低于一个位置时的处理
        else if (bottomItem.position.y <= bottomY)
        {
            int times = Mathf.CeilToInt(Mathf.Abs(bottomItem.position.y - bottomY) / itemHeight);
            float offset = Mathf.Abs(bottomItem.position.y - bottomY) % itemHeight;
            for (int i = 0; i < times; i++)
            {
                bottomItem = content.GetChild((topIdx + visibleItemCount) % (visibleItemCount + 1));
                bottomItem.position = new Vector3(bottomItem.position.x, topY - (times - i - 1) * itemHeight - offset, bottomItem.position.z);
                topIdx--;
                if (topIdx < 0)
                {
                    topIdx = visibleItemCount;
                }
                topItem = content.GetChild(topIdx);
                bottomItem = content.GetChild((topIdx + visibleItemCount) % (visibleItemCount + 1));
                currentDataoffSet--;
                //根据currentDataoffSet来更新Item的具体信息
                UpdateItem(topItem.gameObject, currentDataoffSet);
            }
        }
    }

    // 在 Scene 视图中绘制辅助线
    private void OnDrawGizmos()
    {
        if (content == null) return;

        // 获取 Content 的局部坐标范围
        Vector3[] contentCorners = new Vector3[4];
        content.GetLocalCorners(contentCorners);

        // 将局部坐标转换为世界坐标
        Matrix4x4 matrix = content.transform.localToWorldMatrix;
        for (int i = 0; i < contentCorners.Length; i++)
        {
            contentCorners[i] = matrix.MultiplyPoint(contentCorners[i]);
        }

        // 绘制 Content 的边界框
        Gizmos.color = Color.green;
        Gizmos.DrawLine(contentCorners[0], contentCorners[1]); // 上边
        Gizmos.DrawLine(contentCorners[1], contentCorners[2]); // 右边
        Gizmos.DrawLine(contentCorners[2], contentCorners[3]); // 下边
        Gizmos.DrawLine(contentCorners[3], contentCorners[0]); // 左边

        // 绘制滚动阈值线（上边界和下边界）
        Vector3 upperThresholdPos = content.position + Vector3.up * itemHeight;
        Vector3 lowerThresholdPos = content.position + Vector3.down * itemHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(upperThresholdPos - Vector3.right * 50, upperThresholdPos + Vector3.right * 50); // 上阈值线
        Gizmos.DrawLine(lowerThresholdPos - Vector3.right * 50, lowerThresholdPos + Vector3.right * 50); // 下阈值线
    }
}
