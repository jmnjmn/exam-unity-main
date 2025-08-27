using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

/**
界面上有三个输入框，分别对应 X,Y,Z 的值，请实现 {@link Q1.onGenerateBtnClick} 函数，生成一个 10 × 10 的可控随机矩阵，并显示到界面上，矩阵要求如下：
1. {@link COLORS} 中预定义了 5 种颜色
2. 每个点可选 5 种颜色中的 1 种
3. 按照从左到右，从上到下的顺序，依次为每个点生成颜色，(0, 0)为左上⻆点，(9, 9)为右下⻆点，(0, 9)为右上⻆点
4. 点(0, 0)随机在 5 种颜色中选取
5. 其他各点的颜色计算规则如下，设目标点坐标为(m, n）：
    a. (m, n - 1)所属颜色的概率为基准概率加 X%
    b. (m - 1, n)所属颜色的概率为基准概率加 Y%
    c. 如果(m, n - 1)和(m - 1, n)同色，则该颜色的概率为基准概率加 Z%
    d. 其他颜色平分剩下的概率
*/

public class Q1 : MonoBehaviour
{
    private static readonly Color[] COLORS = new Color[]
    {
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        new Color(1f, 0.5f, 0f) // Orange
    };

    // 每个格子的大小
    private const float GRID_ITEM_SIZE = 75f;

    [SerializeField]
    private InputField xInputField = null;

    [SerializeField]
    private InputField yInputField = null;

    [SerializeField]
    private InputField zInputField = null;

    [SerializeField]
    private Transform gridRootNode = null;

    [SerializeField]
    private GameObject gridItemPrefab = null;


    [SerializeField] private int rowCount = 10;
    [SerializeField] private int columnCount = 10;
    [SerializeField] [Range(0, 1f)] private float yieldTime = 0.2f;

    private ObjectPool<GameObject> _gridItemPool;
    public void OnGenerateBtnClick()
    {
        // TODO: 请在此处开始作答
        _gridItemPool = new ObjectPool<GameObject>(
            () => Instantiate(gridItemPrefab), 
            (obj)=> obj.SetActive(true),
            (obj)=> obj.SetActive(false),
            null,
            true,
            100);
        
        StopAllCoroutines();
        StartCoroutine(GenerateGrid());
    }

    private IEnumerator GenerateGrid()
    {
        WaitForSeconds wait = new WaitForSeconds(this.yieldTime);
        // 如果存在格子，先入池
        foreach (Transform child in gridRootNode)
        {
            _gridItemPool.Release(child.gameObject);
        }

        float baseProbability = 1f / COLORS.Length;
        // 读取输入框的值,进行归一化到0-1内。假设输入的数值是百分比值
        float.TryParse(xInputField.text, out var probX);
        probX = Mathf.Clamp01(probX * 0.01f);
        float.TryParse(yInputField.text, out var probY);
        probY = Mathf.Clamp01(probY * 0.01f);
        float.TryParse(zInputField.text, out var probZ);
        probZ = Mathf.Clamp01(probZ * 0.01f);

        // 初始化存储格子的颜色索引
        int[][] matrixColorIndex = new int[rowCount][];

        float[] colorProbabilities = new float[COLORS.Length];

        for (int y = 0; y < rowCount; y++)
        {
            matrixColorIndex[y] = new int[columnCount];
            for (int x = 0; x < columnCount; x++)
            {
                CalculateProbabilities(x, y, matrixColorIndex, baseProbability,
                    probX, probY, probZ, colorProbabilities);


                // 随机选择颜色
                int colorIndex = 0;
                float r = Random.Range(0, 1f);
                float sumProbability = 0f;
                for (int i = 0; i < colorProbabilities.Length; i++)
                {
                    sumProbability += colorProbabilities[i];
                    if (r <= sumProbability)
                    {
                        colorIndex = i;
                        break;
                    }
                }

                matrixColorIndex[y][x] = colorIndex;

                // 实例化格子
                GameObject gridItem = _gridItemPool.Get();
                gridItem.transform.SetParent(gridRootNode, false);
                gridItem.transform.localPosition =
                    new Vector3((x - columnCount / 2f + 0.5f) * GRID_ITEM_SIZE,
                        (rowCount / 2f - y - 0.5f) * GRID_ITEM_SIZE,
                        0f);
                gridItem.GetComponent<Image>().color = COLORS[colorIndex];
                if (yieldTime > 0)
                {
                    yield return wait;
                }
               
            }
        }
    }

    /// <summary>
    /// 计算格子(x,y)的颜色概率
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="matrixColor"></param>
    /// <param name="baseProb"></param>
    /// <param name="probX"></param>
    /// <param name="probY"></param>
    /// <param name="probZ"></param>
    /// <param name="colorProbabilities"></param>
    private void CalculateProbabilities(int x, int y, int[][] matrixColor,
        float baseProb, float probX, float probY, float probZ,
        float[] colorProbabilities)
    {
        if (colorProbabilities == null)
        {
            colorProbabilities = new float[COLORS.Length];
        }
        else
        {
            // 重置颜色概率
            for (int i = 0; i < colorProbabilities.Length; i++)
            {
                colorProbabilities[i] = 0;
            }
        }

        if (y == 0 && x == 0)
        {
            for (int i = 0; i < colorProbabilities.Length; i++)
            {
                colorProbabilities[i] = baseProb;
            }
        }
        else
        {
            int leftColor = x - 1 >= 0 ? matrixColor[y][x - 1] : -1;
            int topColor = y - 1 >= 0 ? matrixColor[y - 1][x] : -1;

            // c情况
            if (leftColor >= 0 && topColor >= 0 && leftColor == topColor)
            {
                colorProbabilities[leftColor] = baseProb + probZ;
                float remainingProb = (1 - colorProbabilities[leftColor]) / (COLORS.Length - 1);
                for (int i = 0; i < colorProbabilities.Length; i++)
                {
                    if (i != leftColor)
                    {
                        colorProbabilities[i] = remainingProb;
                    }
                }
            }
            else
            {
                int avgProbCount = COLORS.Length;
                // a情况
                if (topColor >= 0)
                {
                    colorProbabilities[topColor] = baseProb + probY;
                    avgProbCount--;
                }

                // b情况
                if (leftColor >= 0)
                {
                    colorProbabilities[leftColor] = baseProb + probX;
                    avgProbCount--;
                }

                // 计算其他颜色的平均概率
                float sum = 0;
                foreach (var p in colorProbabilities)
                {
                    sum += p;
                }

                if (sum >= 1f)
                {
                    sum = 1f;
                }

                float avg = (1 - sum) / avgProbCount;
                for (int i = 0; i < colorProbabilities.Length; i++)
                {
                    if (colorProbabilities[i] == 0)
                    {
                        colorProbabilities[i] = avg;
                    }
                }
            }
        }
    }
}