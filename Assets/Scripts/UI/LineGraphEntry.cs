using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineGraphEntry : MaskableGraphic
{
    public string Name;
    public List<float?> Values;
    private int minimumKey;
    private int maximumKey;
    private float lowestValue;
    private float highestValue;
    private int offset;
	private StatType statType;
	private List<bool?> isSampledAt;

    private Vector3 cachedSelfPosition;

    public float Thickness;
    public bool IsGlow;
	public bool IsAltGlow = false;

    public Color glowColor = Color.blue;
	public Color altGlowColor;

    public void Init(GraphManager.GraphEntryData data)
    {
		statType = data.Subsets[0].SubType.SuperType;
		isSampledAt = GameRefs.I.isSampledAt[statType];
        this.Values = data.Subsets[0].Values;
        this.rectTransform.offsetMin = new Vector2(0, 0);
        this.rectTransform.offsetMax = new Vector2(0, 0);
        this.rectTransform.localScale = new Vector3(1, 1, 1);
        this.offset = data.Offset;
        IsGlow = data.Glow;
        if (IsGlow)
            this.color = IsAltGlow ? altGlowColor : glowColor;
        else
            this.color = data.Subsets[0].SubType.GraphColor;
    }

    private void Update()
    {
        if (this.transform.position != this.cachedSelfPosition)
        {
            this.SetVerticesDirty();
            this.cachedSelfPosition = this.transform.position;
        }
    }

    public bool HasValue()
    {
        for (int i = this.minimumKey - this.offset; i <= this.maximumKey - this.offset; i++)
        {
            if (i < 0 ||
				this.Values == null ||
				i >= this.Values.Count ||
				!isSampledAt[i + offset].HasValue ||
				!isSampledAt[i + offset].Value)
            {
                continue;
            }

            if (this.Values[i].HasValue)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsValidRange()
    {
        bool foundFirstValue = false;

        for (int i = this.minimumKey - this.offset; i <= this.maximumKey - this.offset; i++)
        {
            if (i < 0 ||
				this.Values == null ||
				i >= this.Values.Count ||
				!isSampledAt[i + offset].HasValue ||
				!isSampledAt[i + offset].Value)
            {
                continue;
            }

            if (this.Values[i].HasValue)
            {
                if (!foundFirstValue)
                {
                    foundFirstValue = true;
                }
                else
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void ChangeBounds(int min, int max)
    {
        this.minimumKey = min;
        this.maximumKey = max;

        this.SetVerticesDirty();
    }

    public void SyncScale(float lowestValue, float highestValue)
    {
        this.lowestValue = lowestValue;
        this.highestValue = highestValue;
    }

    public int GetLowestX()
    {
        for (int i = 0; i < this.Values.Count; i++)
        {
            if (this.Values[i].HasValue)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetHighestX()
    {
        for (int i = this.Values.Count - 1; i >= 0; i--)
        {
            if (this.Values[i].HasValue)
            {
                return i;
            }
        }

        return -1;
    }

    public float? GetLowestValue()
    {
        float? value = null;

        for (int i = minimumKey; i <= this.maximumKey; i++)
        {
			value = Utility.Utilities.NullableMin(value, this.Evaluate(i));
        }

        return value;
    }

    public float? GetHighestValue()
    {
        float? value = null;

        for (int i = minimumKey; i <= this.maximumKey; i++)
        {
			value = Utility.Utilities.NullableMax(value, this.Evaluate(i));
        }

        return value;
    }

    private float? Evaluate(int x)
    {
        x -= this.offset;

        if (x < 0 || x >= this.Values.Count || !isSampledAt[x].HasValue)
        {
            return null;
        }
        else
        {
            if (this.Values[x].HasValue && isSampledAt[x].HasValue && isSampledAt[x].Value)
            {
                return this.Values[x].Value;
            }
            else
            {
                int nextLowest = x;
                int nextHighest = x;

                while (nextLowest >= 0 &&
					   (!this.Values[nextLowest].HasValue ||
						(isSampledAt[nextLowest].HasValue && !isSampledAt[nextLowest].Value)))
                {
                    nextLowest--;
                }

                while (nextHighest < this.Values.Count &&
					   (!this.Values[nextHighest].HasValue ||
						(isSampledAt[nextHighest].HasValue && !isSampledAt[nextHighest].Value)))
                {
                    nextHighest++;
                }

                if (nextLowest >= 0 && nextLowest < this.Values.Count && nextHighest >= 0 && nextHighest < this.Values.Count)
                {
                    float pointRatio = 1.0f * (x - nextLowest) / (nextHighest - nextLowest);

                    return this.Values[nextLowest].Value * (1 - pointRatio) + this.Values[nextHighest].Value * pointRatio;
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public Vector3? GetPointInCanvas(int x)
    {
        float? evaluatedValue = this.Evaluate(x);

        if (evaluatedValue.HasValue)
        {
            return new Vector3(this.rectTransform.rect.width * (((float)(x - this.minimumKey) / (this.maximumKey - this.minimumKey - 1)) - this.rectTransform.pivot.x),
                this.rectTransform.rect.height * ((evaluatedValue.Value - this.lowestValue) / (this.highestValue - this.lowestValue) - this.rectTransform.pivot.y));
        }
        else
        {
            return null;
        }
    }

    protected override void OnPopulateMesh(Mesh m)
    {
        Color32 color32 = IsAltGlow ? altGlowColor : color;

        using (var vh = new VertexHelper())
        {
            if (this.IsValidRange())
            {
                int trueMinimumKey = Mathf.Max(this.minimumKey, this.offset);

                Vector3? currentPoint = this.GetPointInCanvas(trueMinimumKey);
                Vector3? nextPoint = this.GetPointInCanvas(trueMinimumKey + 1);

                while (!currentPoint.HasValue)
					   /* !isSampledAt[trueMinimumKey].HasValue || */
					   /* !isSampledAt[trueMinimumKey].Value) */
                {
                    trueMinimumKey += 1;
                    currentPoint = this.GetPointInCanvas(trueMinimumKey);
                    nextPoint = this.GetPointInCanvas(trueMinimumKey + 1);
                }

                int trailingPoint1 = -1;
                int trailingPoint2 = -1;

                /* Vector3 widthOffset = (Vector3.Cross(nextPoint.Value - currentPoint.Value, Vector3.back).normalized) * this.Thickness; */

                int vertCount = 0;

                // Makes dots?
                if(!IsGlow)
                {
                    for (int i = trueMinimumKey; i <= this.maximumKey && i < this.Values.Count + this.offset; i++)
                    {
                        if (!this.Values[i - this.offset].HasValue ||
							!isSampledAt[i].HasValue ||
							!isSampledAt[i].Value)
                        {
                            continue;
                        }

                        currentPoint = this.GetPointInCanvas(i);

                        //float uCurrent = Mathf.Lerp(uBase, uMax, (i - trueMinimumKey) / (this.maximumKey - trueMinimumKey));

                        vh.AddVert(currentPoint.Value + (Vector3.left + Vector3.up) * this.Thickness * 3, color32, new Vector2(0f, 0f));
                        vh.AddVert(currentPoint.Value + (Vector3.right + Vector3.up) * this.Thickness * 3, color32, new Vector2(1f, 0f));
                        vh.AddVert(currentPoint.Value + (Vector3.right + Vector3.down) * this.Thickness * 3, color32, new Vector2(1f, 1f));
                        vh.AddVert(currentPoint.Value + (Vector3.left + Vector3.down) * this.Thickness * 3, color32, new Vector2(0f, 1f));
                        vertCount += 4;

                        vh.AddTriangle(vertCount - 4, vertCount - 3, vertCount - 2);
                        vh.AddTriangle(vertCount - 4, vertCount - 2, vertCount - 1);
                    }
                }

                currentPoint = this.GetPointInCanvas(trueMinimumKey);

                // Makes lines
                for (int i = trueMinimumKey + 1; i <= this.maximumKey && i < this.Values.Count + this.offset; i++)
                {
                    nextPoint = this.GetPointInCanvas(i);

                    if (!nextPoint.HasValue)
                    {
						currentPoint = null;
                        continue;
                    }
					else if (!currentPoint.HasValue)
					{
						currentPoint = nextPoint;
						continue;
					}

                    Vector3 widthOffset = (Vector3.Cross(nextPoint.Value - currentPoint.Value, Vector3.back).normalized) * this.Thickness;

                    //float uCurrent = Mathf.Lerp(uBase, uMax, (i - trueMinimumKey) / (this.maximumKey - trueMinimumKey));

                    vh.AddVert(currentPoint.Value - widthOffset, color32, new Vector2(0.2f, 0f));
                    vh.AddVert(currentPoint.Value + widthOffset, color32, new Vector2(0.2f, 1f));
                    vh.AddVert(nextPoint.Value - widthOffset, color32, new Vector2(0.8f, 0f));
                    vh.AddVert(nextPoint.Value + widthOffset, color32, new Vector2(0.8f, 1f));
                    vertCount += 4;

					// What does this do?
                    /* if (i > trueMinimumKey + 1) */
                    /* { */
                        /* if (widthOffset.y < 0) */
                        /* { */
                            /* vh.AddTriangle(trailingPoint1, vertCount - 3, vertCount - 4); */
                            /* vh.AddTriangle(vertCount - 4, trailingPoint1, trailingPoint2); */
                        /* } */
                        /* else */
                        /* { */
                            /* vh.AddTriangle(trailingPoint2, vertCount - 3, vertCount - 4); */
                            /* vh.AddTriangle(vertCount - 4, trailingPoint2, trailingPoint1); */
                        /* } */
                    /* } */

                    trailingPoint1 = vertCount - 2;
                    trailingPoint2 = vertCount - 1;

                    vh.AddTriangle(vertCount - 4, vertCount - 3, vertCount - 2);
                    vh.AddTriangle(vertCount - 2, vertCount - 3, vertCount - 1);

                    currentPoint = nextPoint;
                }
            }
            else if (this.HasValue())
            {
                int trueMinimumKey = Mathf.Max(this.minimumKey, this.offset);

                Vector3? currentPoint = this.GetPointInCanvas(trueMinimumKey);

                while (!isSampledAt[trueMinimumKey].HasValue ||
					   !isSampledAt[trueMinimumKey].Value ||
					   !currentPoint.HasValue)
                {
                    trueMinimumKey += 1;
                    currentPoint = this.GetPointInCanvas(trueMinimumKey);
                }

                int vertCount = 0;

                vh.AddVert(currentPoint.Value + Vector3.left * this.Thickness * 3, color32, new Vector2(0, 0f));
                vh.AddVert(currentPoint.Value + Vector3.up * this.Thickness * 3, color32, new Vector2(0, 1f));
                vh.AddVert(currentPoint.Value + Vector3.right * this.Thickness * 3, color32, new Vector2(1, 1f));
                vh.AddVert(currentPoint.Value + Vector3.down * this.Thickness * 3, color32, new Vector2(1, 0f));
                vertCount += 4;

                vh.AddTriangle(vertCount - 4, vertCount - 3, vertCount - 2);
                vh.AddTriangle(vertCount - 4, vertCount - 2, vertCount - 1);
            }

            vh.FillMesh(m);
        }
    }
}
