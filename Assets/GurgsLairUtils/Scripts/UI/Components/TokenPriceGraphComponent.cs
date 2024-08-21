using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// TODO: the names need be mutable 
/// TODO: the graph needs to be able to be updated
/// TODO: this needs to be cleaned up
/// </summary>
public class TokenPriceGraphComponent : MonoBehaviour
{
    [Header("Parents")]
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private RectTransform textContainer;

    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private float lineWidth = 2f;
    [SerializeField] private GameObject coordinateDisplayPrefab;
    [SerializeField] private Vector2 sizeOfPoint = new Vector2(10, 10);
    [SerializeField] private float xTextBuffer = 20f;
    [SerializeField] private float yTextBuffer = 20f;
    [SerializeField] private int decimalPoints = 2;
    [SerializeField] private Color divisionLineColor = Color.gray;
    [SerializeField] private Color subdivisionLineColor = Color.gray;
    [SerializeField] private float divisionLineWidth = 1f;
    [SerializeField] private float subdivisionLineWidth = 0.5f;
    [SerializeField] private int fontSize = 12;
    [SerializeField] private bool staggeredXAxis = false;

    public bool showSubdivisions = false;

    private Vector2[] corners = new Vector2[4];
    private float maxPrice;
    private float minPrice;

    private GameObject xLine;
    private GameObject yLine;
    private GameObject coordinateDisplayObject;

    [SerializeField] private float yRunaway = 1.2f;
    private DateTime endTime;

    [SerializeField] MouseInteractionComponent _mouseInteractionComponent;

    private List<TimeSpan> timeframes = new List<TimeSpan>
    {
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromHours(1),
        TimeSpan.FromHours(4),
        TimeSpan.FromHours(8),
        TimeSpan.FromHours(12),
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(3)
    };

    public struct PriceData
    {
        public List<float> prices;
        public Color lineColor;
        public Color pointColor;
        public float lineWidth;
        public Sprite dotSprite;
        public TimeSpan timeframe;
    }


    private void OnEnable()
    {
        InitializeCorners();
        DrawBTCGraph();

        InitializeCorners();
        InitializeLines();
    }

    private void Update()
    {
        HandleMouseHover();
    }

    private void InitializeLines()
    {
        if (xLine == null)
        {
            xLine = CreateLineObject(Color.yellow);
        }
        if (yLine == null)
        {
            yLine = CreateLineObject(Color.yellow);
        }
    }

    void DrawBTCGraph()
    {
        List<float> prices = new List<float>
        {
            22349f, 22475f, 24567, 22725f, 22576,
            22975f, 22456, 23225f, 26789, 23333,
            23556,21567,25678,26789, 23333,
        };

        TokenPriceGraphComponent.PriceData data = new TokenPriceGraphComponent.PriceData
        {
            prices = prices,
            lineColor = Color.green,
            pointColor = Color.yellow,
            lineWidth = 1f,
            dotSprite = null, // Will use default sprite
            timeframe = TimeSpan.FromMinutes(15)
        };

        DrawPriceGraph(data);
    }

    private void InitializeCorners()
    {
        corners[0] = Vector2.zero;
        corners[1] = new Vector2(0, graphContainer.rect.height);
        corners[2] = new Vector2(graphContainer.rect.width, graphContainer.rect.height);
        corners[3] = new Vector2(graphContainer.rect.width, 0);
    }

    public void DrawPriceGraph(PriceData priceData)
    {
        ClearGraph();
        SetupPriceRange(priceData.prices);
        SetupTimeRange(priceData.timeframe);
        ShowGraph(priceData);
        DrawDivisionLines(priceData);
        SetupAxisLabels(priceData);
    }

    private void OnDisable()
    {
        ClearGraph();
    }

    private void ClearGraph()
    {
        DeleteAllChildren(graphContainer);
        DeleteAllChildren(textContainer);
    }

    private void SetupTimeRange(TimeSpan timeframe)
    {
        endTime = RoundToTimeframe(DateTime.Now, timeframe);
    }

    private DateTime RoundToTimeframe(DateTime time, TimeSpan timeframe)
    {
        long ticks = time.Ticks / timeframe.Ticks;
        return new DateTime(ticks * timeframe.Ticks, time.Kind);
    }

    private void SetupPriceRange(List<float> prices)
    {
        float rawMax = prices.Max();
        float rawMin = prices.Min();
        float range = rawMax - rawMin;

        maxPrice = rawMax + (range * (yRunaway - 1) / 2);
        minPrice = rawMin - (range * (yRunaway - 1) / 2);
    }
    
    private void ShowGraph(PriceData priceData)
    {
        GameObject prevDot = null;
        float lerpValue = priceData.prices.Count - 1;

        for (int i = 0; i < priceData.prices.Count; i++)
        {
            if (i >= priceData.prices.Count) continue;

            float xPosition = Mathf.Lerp(corners[0].x, corners[2].x, (i / lerpValue));
            float yPosition = Mathf.Lerp(corners[0].y, corners[1].y, (priceData.prices[i] - minPrice) / (maxPrice - minPrice));

            var dot = CreateDot(new Vector2(xPosition, yPosition), priceData.pointColor, priceData.dotSprite);

            if (prevDot != null)
            {
                CreateConnection(prevDot.GetComponent<RectTransform>().anchoredPosition,
                                 dot.GetComponent<RectTransform>().anchoredPosition,
                                 priceData.lineColor, priceData.lineWidth);
            }
            prevDot = dot;
        }
    }

    private void DrawDivisionLines(PriceData priceData)
    {
        int divisions = priceData.prices.Count -1;   //here this is the numbers of input minus
        for (int i = 0; i < divisions; i++)
        {
            float normalizedValue = (float)i / divisions;
            DrawDivisionLine(normalizedValue, true);
            DrawDivisionLine(normalizedValue, false);

            if (showSubdivisions)
            {
                for (int j = 1; j < 5; j++)
                {
                    float subNormalizedValue = normalizedValue - 0.1f + (j * 0.02f);
                    DrawSubdivisionLine(subNormalizedValue, true);
                    DrawSubdivisionLine(subNormalizedValue, false);
                }
            }
        }
    }

    private void DrawDivisionLine(float normalizedValue, bool isVertical)
    {
        Vector2 startPos, endPos;
        if (isVertical)
        {
            float xPosition = Mathf.Lerp(corners[0].x, corners[2].x, normalizedValue);
            startPos = new Vector2(xPosition, corners[0].y);
            endPos = new Vector2(xPosition, corners[1].y);
        }
        else
        {
            float yPosition = Mathf.Lerp(corners[0].y, corners[1].y, normalizedValue);
            startPos = new Vector2(corners[0].x, yPosition);
            endPos = new Vector2(corners[2].x, yPosition);
        }
        CreateDivisionLine(startPos, endPos, divisionLineColor, divisionLineWidth);
    }

    private void DrawSubdivisionLine(float normalizedValue, bool isVertical)
    {
        Vector2 startPos, endPos;
        if (isVertical)
        {
            float xPosition = Mathf.Lerp(corners[0].x, corners[2].x, normalizedValue);
            startPos = new Vector2(xPosition, corners[0].y);
            endPos = new Vector2(xPosition, corners[1].y);
        }
        else
        {
            float yPosition = Mathf.Lerp(corners[0].y, corners[1].y, normalizedValue);
            startPos = new Vector2(corners[0].x, yPosition);
            endPos = new Vector2(corners[2].x, yPosition);
        }
        CreateDivisionLine(startPos, endPos, subdivisionLineColor, subdivisionLineWidth);
    }

    private void CreateDivisionLine(Vector2 startPos, Vector2 endPos, Color color, float width)
    {
        GameObject line = CreateLineObject(color);
        line.transform.SetAsFirstSibling(); // Ensure lines are drawn behind data points
        DrawLine(line, startPos, endPos, width);
    }

    private void SetupAxisLabels(PriceData priceData)
    {
        SetupYAxisLabels();
        SetupXAxisLabels(priceData);
    }

    private void SetupYAxisLabels()
    {
        int divisions = 10;
        for (int i = 0; i <= divisions; i++)
        {
            float normalizedValue = (float)i / divisions;
            float yPosition = Mathf.Lerp(corners[0].y, corners[1].y, normalizedValue);
            float price = Mathf.Lerp(minPrice, maxPrice, normalizedValue);

            string priceText = FormatPrice(price);
            CreateAxisLabel(priceText, new Vector2(-textContainer.rect.width / 2 - xTextBuffer, yPosition - textContainer.rect.height / 2));
        }
    }

    private void SetupXAxisLabels(PriceData priceData)
    {
        int divisions = priceData.prices.Count;
        DateTime startTime = endTime.AddTicks(-(priceData.timeframe.Ticks * divisions));

        for (int i = 0; i <= divisions; i++)
        {
            float normalizedValue = (float)i / divisions;
            float xPosition = Mathf.Lerp(corners[0].x, corners[2].x, normalizedValue);
            DateTime labelTime = startTime.Add(TimeSpan.FromTicks(priceData.timeframe.Ticks * i));

            string labelText = FormatTimeLabel(labelTime, priceData.timeframe);
            float yOffset = staggeredXAxis && i % 2 == 1 ? -20f : 0f;
            CreateAxisLabel(labelText, new Vector2(xPosition - textContainer.rect.width / 2, -textContainer.rect.height / 2 - yTextBuffer + yOffset));
        }
    }

    private string FormatPrice(float price)
    {
        if (decimalPoints == 0)
        {
            return Mathf.RoundToInt(price).ToString();
        }
        else
        {
            return price.ToString("F" + decimalPoints);
        }
    }

    private string FormatTimeLabel(DateTime time, TimeSpan timeframe)
    {
        if (timeframe.TotalDays >= 1)
            return time.ToString("MM/dd");
        else if (timeframe.TotalHours >= 1)
            return time.ToString("HH:mm");
        else
            return time.ToString("HH:mm");
    }

    private void CreateAxisLabel(string text, Vector2 position)
    {
        TMP_Text label = Instantiate(coordinateDisplayPrefab, textContainer).GetComponent<TMP_Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.rectTransform.anchoredPosition = position;
    }

    private GameObject CreateDot(Vector2 anchoredPos, Color pointColor, Sprite customDotSprite)
    {
        GameObject dot = new GameObject("Dot", typeof(Image));
        dot.transform.SetParent(graphContainer, false);

        Image image = dot.GetComponent<Image>();
        image.sprite = customDotSprite != null ? customDotSprite : defaultSprite;
        image.color = pointColor;

        RectTransform rectTransform = dot.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = sizeOfPoint;
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        return dot;
    }

    private void CreateConnection(Vector2 dotPosA, Vector2 dotPosB, Color lineColor, float lineWidth)
    {
        GameObject connection = new GameObject("Connection", typeof(Image));
        connection.transform.SetParent(graphContainer, false);
        connection.transform.SetAsFirstSibling(); // Ensure connections are drawn behind dots
        connection.GetComponent<Image>().color = lineColor;

        RectTransform rectTransform = connection.GetComponent<RectTransform>();
        Vector2 dir = (dotPosB - dotPosA).normalized;
        float distance = Vector2.Distance(dotPosA, dotPosB);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, lineWidth);
        rectTransform.anchoredPosition = dotPosA + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
    }

    private GameObject CreateLineObject(Color lineColor)
    {
        GameObject lineObj = new GameObject("Line", typeof(Image));
        lineObj.transform.SetParent(graphContainer, false);
        lineObj.GetComponent<Image>().color = lineColor;

        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);

        lineObj.SetActive(false);

        return lineObj;
    }

    private void DrawLine(GameObject lineObj, Vector2 startPos, Vector2 endPos, float width)
    {
        lineObj.SetActive(true);

        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();

        Vector2 dir = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);

        rectTransform.sizeDelta = new Vector2(distance, width);
        rectTransform.anchoredPosition = startPos + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, GetAngleFromVectorFloat(dir));
    }
    
    private void HandleMouseHover()
    {
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(graphContainer, Input.mousePosition, null, out localMousePos);

        if (_mouseInteractionComponent.PointerIsOver)
        {
            InitializeLines(); // Ensure lines are not null
            UpdateCoordinateDisplay(localMousePos);

            //localMousePos.x += graphContainer.rect.width / 2;
            //localMousePos.y += graphContainer.rect.height / 2;

            DrawLine(xLine, new Vector2(localMousePos.x + graphContainer.rect.width / 2, corners[0].y), new Vector2(localMousePos.x + graphContainer.rect.width / 2, corners[1].y), 1f);
            DrawLine(yLine, new Vector2(corners[0].x, localMousePos.y + graphContainer.rect.height / 2), new Vector2(corners[2].x, localMousePos.y + graphContainer.rect.height / 2), 1f);
        }
        else
        {
            HideCoordinateDisplay();
        }
    }
    
    private void UpdateCoordinateDisplay(Vector2 position)
    {
        if (coordinateDisplayObject == null)
        {
            coordinateDisplayObject = Instantiate(coordinateDisplayPrefab, graphContainer);
        }

        float xValue = Mathf.Lerp(0, maxPrice, position.x / graphContainer.rect.width);
        float yValue = Mathf.Lerp(minPrice, maxPrice, position.y / graphContainer.rect.height);

        TMP_Text coordinateText = coordinateDisplayObject.GetComponent<TMP_Text>();
        coordinateText.text = $"Price: {FormatPrice(yValue)}";
        coordinateText.fontSize = fontSize;

        Vector2 displayPos = new Vector2(position.x, position.y + 20);
        coordinateDisplayObject.GetComponent<RectTransform>().anchoredPosition = displayPos;

        coordinateDisplayObject.SetActive(true);
    }
    
    private void HideCoordinateDisplay()
    {
        xLine.SetActive(false);
        yLine.SetActive(false);

        if (coordinateDisplayObject != null)
        {
            coordinateDisplayObject.SetActive(false);
        }
    }

    private float GetAngleFromVectorFloat(Vector2 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }

    private void DeleteAllChildren(Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}