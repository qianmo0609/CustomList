using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEditorInternal.VersionControl;

public class TestLoopListNGUI : MonoBehaviour
{
    public UIScrollView scrollView;
    public UIGrid grid;
    public GameObject itemPrefab;
    public int itemCount;  // 数据项的总数
    public float itemHeight;  // 列表项的高度

    private List<GameObject> visibleItems = new List<GameObject>();  // 当前可见的列表项
    private Queue<GameObject> itemPool = new Queue<GameObject>();  // 列表项对象池
    private List<object> dataList = new List<object>();  // 数据列表，这里用 object 类型可存储多种数据类型

    private int visibleItemCount;  // 可见区域内可容纳的最大列表项数量

    private Vector3 topY;
    private Vector3 bottomY;

    private bool isDrag;
    float currentPosition;
    float lastPosition;
    float startPosY;

    UIButton btnTest;

    void Start()
    {
        UIPanel uiPanel = this.GetComponent<UIPanel>();
        //这是列表第一项位置
        startPosY = uiPanel.height / 2 + this.transform.position.y;
        topY = grid.transform.TransformPoint(new Vector3(0, uiPanel.height / 2 + this.transform.position.y + itemHeight, 0));
        bottomY = grid.transform.TransformPoint(new Vector3(0, this.transform.position.y - uiPanel.height / 2, 0));
        isDrag = false;
        // 初始化数据列表，这里简单示例为数字 0 到 itemCount - 1
        for (int i = 0; i < itemCount; i++)
        {
            dataList.Add(i);
        }

        // 计算可见区域内可容纳的最大列表项数量
        visibleItemCount = 5;//Mathf.FloorToInt(scrollView.panel.height / itemHeight);

        // 初始化显示的列表项
        InitializeVisibleItems();

        // 监听滚动事件
        scrollView.onDragStarted = onDragStart;
        scrollView.onDragFinished = onDragEnd;
    }

    void Test()
    {

    }

    void onDragStart()
    {
        isDrag = true;
    }

    void onDragEnd()
    {
        isDrag = false;
    }

    private void Update()
    {
        if (isDrag)
        {
            currentPosition = grid.transform.position.y;
            // 处理向上滚动
            if (currentPosition > lastPosition)
            {
                for (int i = 0;i < visibleItems.Count; i++)
                {
                    if (grid.transform.TransformPoint(visibleItems[i].transform.localPosition).y > topY.y)
                    {
                        visibleItems[i].transform.localPosition = new Vector3(0, visibleItems[i].transform.localPosition.y - visibleItemCount * 100, 0);
                        //得到最后一项的索引，取数据，然后更新Item显示
                        //GetLastVisibleIndex()
                    }
                }
            }
            // 处理向下滚动
            else if (currentPosition < lastPosition)
            {
                for (int i = 0; i < visibleItems.Count; i++)
                {
                    if (grid.transform.TransformPoint(visibleItems[i].transform.localPosition).y < bottomY.y)
                    {
                        visibleItems[i].transform.localPosition = new Vector3(0, visibleItems[i].transform.localPosition.y + visibleItemCount * 100, 0);
                        //得到第一项的索引，取数据，然后更新Item显示
                        //GetFirstVisibleIndex()
                    }
                }
            }
            lastPosition = currentPosition;
        }    
    }

    void InitializeVisibleItems()
    {
        for (int i = 0; i < visibleItemCount; i++)
        {
            GameObject item = GetItemFromPool();
            item.transform.SetParent(grid.transform, false);
            item.SetActive(true);
            item.gameObject.name = i.ToString(); 
            UpdateItem(item, i);
            visibleItems.Add(item);
        }
        grid.Reposition();
    }

    GameObject GetItemFromPool()
    {
        if (itemPool.Count > 0)
        {
            return itemPool.Dequeue();
        }
        else
        {
            return NGUITools.AddChild(grid.gameObject, itemPrefab);
        }
    }

    void UpdateItem(GameObject item, int index)
    {
        if (index < dataList.Count)
        {
            UILabel label = item.GetComponentInChildren<UILabel>();
            if (label != null)
            {
                label.text = dataList[index].ToString();
            }
        }
    }

    int GetFirstVisibleIndex()
    {
        return Mathf.FloorToInt(grid.transform.localPosition.y - startPosY / itemHeight);
    }

    int GetLastVisibleIndex()
    {
        return GetFirstVisibleIndex() + visibleItemCount - 1;
    }

    void RecycleItem(GameObject item)
    {
        item.SetActive(false);
        itemPool.Enqueue(item);
    }
}
