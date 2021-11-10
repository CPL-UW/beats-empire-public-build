using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class BarGraphEntry : MaskableGraphic
{
    public string Name;
    public List<float?> Values;
    public List<Color> Colors;
	public Color GlowColor;
	public Color AltGlowColor;

    private float positionInGraph;
    private float highestValue;

	public bool isGlow;
	public bool altGlow = false;
	public bool isUnlocked;
	private float buffer = -10f;

	private Vector3[] myVertexes;

    private Vector3 cachedSelfPosition;

    public float Thickness;

    public void Init(GraphManager.GraphEntryData data, bool glow=false)
    {
		myVertexes = new Vector3[4];
        this.Name = data.Name;

        this.Values = new List<float?>();
        this.Colors = new List<Color>();
		isGlow = glow;
		isUnlocked = data.EntryUnlocked;

        for (int i = 0; i < data.Subsets.Count; i++)
        {
            this.Values.Add(data.Subsets[i].Values[0]);

			// faded color if not unlocked
            this.Colors.Add(isUnlocked ? data.Subsets[i].SubType.GraphColor : 
				new Color(data.Subsets[i].SubType.GraphColor.r,
				data.Subsets[i].SubType.GraphColor.g,
				data.Subsets[i].SubType.GraphColor.b,
				0.2f));
        }

        this.rectTransform.offsetMin = new Vector2(0, 0);
        this.rectTransform.offsetMax = new Vector2(0, 0);
        this.rectTransform.localScale = new Vector3(1, 1, 1);
    }

    private void Update()
    {
        if (this.transform.position != this.cachedSelfPosition)
        {
            this.SetVerticesDirty();
            this.cachedSelfPosition = this.transform.position;
        }
    }

    public bool IsValidKey()
    {
        return this.Evaluate().HasValue;
    }

    public void ChangeKey(int key)
    {
        this.SetVerticesDirty();
    }

    public void SyncTransform(float highestValue, float positionInGraph)
    {
        this.highestValue = highestValue;
        this.positionInGraph = positionInGraph;
    }

    public float? GetValue()
    {
        return this.Evaluate();
    }

    private float? Evaluate()
    {
        float? sum = null;

        for (int i = 0; i < this.Values.Count; i++)
        {
            float? subValue = this.SubEvaluate(i);

            if (subValue.HasValue)
            {
                if (sum.HasValue)
                {
                    sum += subValue.Value;
                }
                else
                {
                    sum = subValue.Value;
                }
            }
        }

        return sum;
    }

    private float? SubEvaluate(int index)
    {
        return this.Values[index];
    }

	public void SetAltGlow(bool altGlow)
	{
		this.altGlow = altGlow;
	}

	public bool CheckBounds(Vector3 point)
	{
		if (isGlow)
			return false;

		// Check if it's inside our bar plus some buffer
		if (point.x > myVertexes[1].x && point.x < myVertexes[0].x &&
			point.y > myVertexes[0].y && point.y < myVertexes[2].y)
			return true;
		else
			return false;
	}

    public Vector3? GetPointInCanvas()
    {
        float? evaluatedValue = this.Evaluate();

        if (evaluatedValue.HasValue)
        {
            return new Vector3(this.rectTransform.rect.width * (this.positionInGraph - this.rectTransform.pivot.x), 
                this.rectTransform.rect.height * ((evaluatedValue.Value / this.highestValue) - this.rectTransform.pivot.y));
        }
        else
        {
            return null;
        }
    }

    private Vector3? GetSubPointInCanvas(int index)
    {
        float? evaluatedValue = this.SubEvaluate(index);

        if (evaluatedValue.HasValue)
        {
            return new Vector3(this.rectTransform.rect.width * (this.positionInGraph - this.rectTransform.pivot.x),
                this.rectTransform.rect.height * ((evaluatedValue.Value / this.highestValue) - this.rectTransform.pivot.y));
        }
        else
        {
            return null;
        }
    }

    protected override void OnPopulateMesh(Mesh m)
    {
        using (var vh = new VertexHelper())
        {
            if (this.IsValidKey())
            {
                float summedHeight = 0;
                int vertexCount = 0;

                for (int i = 0; i < this.Values.Count; i++)
                {
                    Vector3? possibleTop = this.GetSubPointInCanvas(i);
					Color32 color32;
					if (isGlow)
						color32 = altGlow ? AltGlowColor : GlowColor;
					else
						color32 = this.Colors[i];

					if (possibleTop.HasValue)
                    {
                        Vector3 top = possibleTop.Value;
                        Vector3 bottom = possibleTop.Value;
                        bottom.y = this.rectTransform.rect.height * (-this.rectTransform.pivot.y);

                        top.y += summedHeight;
                        bottom.y += summedHeight;

                        summedHeight += top.y - bottom.y;

                        Vector3 widthOffset = (Vector3.Cross(top - bottom, Vector3.back).normalized) * this.Thickness;

                        vh.AddVert(bottom - widthOffset + new Vector3(widthOffset.x * 0.25f, 0f, 0f) - new Vector3(isGlow ? buffer : 0f, 0f, 0f), color32, new Vector2(0, 0f));
                        vh.AddVert(bottom + widthOffset + new Vector3(isGlow ? buffer : 0f, 0f, 0f), color32, new Vector2(1, 0f));
                        vh.AddVert(top - widthOffset - new Vector3(isGlow ? buffer : 0f, isGlow ? buffer : 0f, 0f), color32, new Vector2(0, 1f));
                        vh.AddVert(top + widthOffset + new Vector3(isGlow ? buffer : 0f, isGlow ? -buffer : 0f, 0f), color32, new Vector2(1, 1f));

						myVertexes[0] = bottom - widthOffset - new Vector3(isGlow ? buffer : 0f, 0f, 0f);
						myVertexes[1] = bottom + widthOffset + new Vector3(isGlow ? buffer : 0f, 0f, 0f);
						myVertexes[2] = top - widthOffset - new Vector3(0f, isGlow ? buffer : 0f, 0f);
						myVertexes[3] = top + widthOffset + new Vector3(0f, isGlow ? buffer : 0f, 0f);

                        vertexCount += 4;

                        vh.AddTriangle(vertexCount - 4, vertexCount - 3, vertexCount - 2);
                        vh.AddTriangle(vertexCount - 2, vertexCount - 3, vertexCount - 1);
                    }
                }
            }

            vh.FillMesh(m);
        }
    }
}
