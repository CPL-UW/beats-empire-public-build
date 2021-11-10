using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AspectRatioLock : MonoBehaviour {
	public int relativeWidth;
	public int relativeHeight;
	public List<CanvasScaler> canvasScalers;

	private int oldWidth;
	private int oldHeight;
	private float targetAspect;
	private Vector2 oldResolution;
	private new Camera camera;
	private Camera voidCamera;

	void Start()
	{
		targetAspect = relativeWidth / (float) relativeHeight;
		camera = GetComponent<Camera>();
		LockAspectRatio();
	}

	void Update()
	{
		if (oldWidth != Screen.width || oldHeight != Screen.height)
		{
			LockAspectRatio();
		}
	}

	private void AddVoidCamera()
	{
		voidCamera = new GameObject("Void Camera", typeof(Camera)).GetComponent<Camera>();
		voidCamera.depth = int.MinValue;
		voidCamera.clearFlags = CameraClearFlags.SolidColor;
		voidCamera.backgroundColor = Color.black;
		voidCamera.cullingMask = 0;
	}

	private void LockAspectRatio()
	{
		oldWidth = Screen.width;
		oldHeight = Screen.height;
		float currentAspect = Screen.width / (float) Screen.height;

		Rect viewport = camera.rect;
		if (Mathf.Abs(currentAspect - targetAspect) < 0.01f)
		{
			viewport = new Rect(0, 0, 1, 1);
			if (voidCamera)
			{
				Destroy(voidCamera.gameObject);
				voidCamera = null;
			}
		}
		else
		{
			if (!voidCamera)
			{
				AddVoidCamera();
			}
			if (currentAspect < targetAspect)
			{
				viewport.width = 1.0f;
				viewport.height = currentAspect / targetAspect;
				viewport.x = 0;
				viewport.y = (1.0f - viewport.height) * 0.5f;

				foreach (CanvasScaler scaler in canvasScalers)
				{
					scaler.matchWidthOrHeight = 0.0f;
				}
			}
			else
			{
				viewport.width = targetAspect / currentAspect;
				viewport.height = 1.0f;
				viewport.x = (1.0f - viewport.width) * 0.5f;
				viewport.y = 0;

				foreach (CanvasScaler scaler in canvasScalers)
				{
					scaler.matchWidthOrHeight = 1.0f;
				}
			}
		}

		camera.rect = viewport;
	}
}
