using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

public static class Footilities {
  [System.Serializable]
  public class JsonArrayWrapper<T> {
    public T[] array;
  }

  public static T DeepClone<T>(T root) {
    // This is not the faster way to clone, but it's fast enough and only
    // requires Serializable.
    MemoryStream stream = new MemoryStream();
    BinaryFormatter formatter = new BinaryFormatter();
    formatter.Serialize(stream, root);
    stream.Position = 0;
    return (T) formatter.Deserialize(stream);
  }

  public static List<GameObject> FindAllObjectsWithTag(string tag) {
    List<GameObject> matches = new List<GameObject>();
    foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects()) {
      FindAllObjectsWithTag(tag, matches, go.transform);
    }
    return matches;
  }

  private static void FindAllObjectsWithTag(string tag, List<GameObject> soFar, Transform root) {
    if (root.gameObject.CompareTag(tag)) {
      soFar.Add(root.gameObject);
    }

    for (int i = 0; i < root.childCount; ++i) {
      FindAllObjectsWithTag(tag, soFar, root.GetChild(i));
    }
  }

  public static T[] GetJsonArray<T>(string json) {
    string objectJson = "{\"array\": " + json + "}";
    JsonArrayWrapper<T> wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>(objectJson);
    return wrapper.array;
  }

  public static void SetFirstChild(this GameObject parent, GameObject child) {
    child.transform.SetParent(parent.transform);
    child.transform.SetSiblingIndex(0);
  }

  public static string ToHex(this Color color) {
    return ColorUtility.ToHtmlStringRGBA(color);
  }

  public static void Push<T>(this List<T> list, T item) {
    list.Add(item);
  }

  public static T Pop<T>(this List<T> list) {
    int i = list.Count - 1;
    T item = list[i];
    list.RemoveAt(i);
    return item;
  }

  public static void SetLastChild(this GameObject parent, GameObject child) {
    child.transform.SetParent(parent.transform);
    child.transform.SetSiblingIndex(parent.transform.childCount - 1);
  }

  public static void SetSize(this RectTransform trans, Vector2 newSize) {
    Vector2 oldSize = trans.rect.size;
    Vector2 deltaSize = newSize - oldSize;
    trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x, deltaSize.y * trans.pivot.y);
    trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1.0f - trans.pivot.x), deltaSize.y * (1.0f - trans.pivot.y));
  }

  public static int Random100() {
    return Random.Range(0, 100);
  }

  public static T RandomElement<T>(this T[] list) {
    return list[Random.Range(0, list.Length)];
  }

  public static T RandomElement<T>(this List<T> list) {
    return list[Random.Range(0, list.Count)];
  }

  public static T RemoveRandomElement<T>(this List<T> list) {
	int i = Random.Range(0, list.Count);
	T element = list[i];
	list.RemoveAt(i);
    return element;
  }

  public static T RandomEnum<T>() {
	System.Array values = System.Enum.GetValues(typeof(T));
    return (T) values.GetValue(Random.Range(0, values.Length));
  }

  public static string Pluralize(string noun, int quantity, bool prefixQuantity = true) {
    string text = string.Format("{1}{2}", quantity, noun, quantity == 1 ? "" : "s");
    if (prefixQuantity) {
      text = quantity + " " + text;
    }
    return text;
  }

  public static void DestroyChildren(GameObject gameObject) {
    for (int i = gameObject.transform.childCount - 1; i >= 0; --i) {
      GameObject.Destroy(gameObject.transform.GetChild(i).gameObject);
    }
  }

  public static IEnumerator CoLoadSessionManager() {
	  if (GameObject.FindObjectOfType<SessionController>() == null) {
		  AsyncOperation load = SceneManager.LoadSceneAsync("SessionManager", LoadSceneMode.Additive);
		  yield return new WaitUntil(() => load.isDone);
	  }
  }

  public static bool ContainsPixel(GameObject obj, Vector2 point) {
    return RectTransformUtility.RectangleContainsScreenPoint(obj.GetComponent<RectTransform>(), point);
  }

  public static void Shuffle<T>(this T[] list) {
    for (int i = list.Length - 1; i > 0; --i) {
      int j = Random.Range(0, i + 1);
      T tmp = list[i];
      list[i] = list[j];
      list[j] = tmp;
    }
  }

  public static void Shuffle<T>(this List<T> list) {
    for (int i = list.Count - 1; i > 0; --i) {
      int j = Random.Range(0, i + 1);
      T tmp = list[i];
      list[i] = list[j];
      list[j] = tmp;
    }
  }

  public static IEnumerator CoReplaceScene(string oldScene, string newScene, AudioListener oldEar = null) {
	if (oldEar != null) {
	  oldEar.enabled = false;
	}
    AsyncOperation load = SceneManager.LoadSceneAsync(newScene, LoadSceneMode.Additive);
    yield return new WaitUntil(() => load.isDone);
    SceneManager.UnloadSceneAsync(oldScene);
  }

  public static int RandomThat(int lo, int hi, System.Predicate<int> criteria) {
    int random;
    do {
      random = Random.Range(lo, hi);
    } while (!criteria(random));
    return random;
  }

  public static Coroutine Schedule(this MonoBehaviour invoker, float delay, System.Action job) {
	return invoker.StartCoroutine(CoSchedule(delay, job));
  }

  public static IEnumerator CoSchedule(float delay, System.Action job) {
	  yield return new WaitForSeconds(delay);
	  job();
  }

  public static IEnumerator CoEaseInBack(float targetTime, float startValue, float endValue, System.Action<float> onStep, System.Action onEnd = null) {
    float elapsedTime = 0;
    float startTime = Time.time;

    while (elapsedTime < targetTime) {
      float s = 1.70158f;
      float proportion = elapsedTime / targetTime;
      float intermediate = (endValue - startValue) * proportion * proportion * ((s + 1) * proportion - s) + startValue;
      onStep(intermediate);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    onStep(endValue);

    if (onEnd != null) {
      onEnd();
    }
  }

  public static IEnumerator CoEaseOutBack(float targetTime, float startValue, float endValue, System.Action<float> onStep, System.Action onEnd = null) {
    float elapsedTime = 0;
    float startTime = Time.time;

    while (elapsedTime < targetTime) {
      float s = 1.70158f;
      float proportion = elapsedTime / targetTime - 1;
      float intermediate = (endValue - startValue) * (proportion * proportion * ((s + 1) * proportion + s) + 1) + startValue;
      onStep(intermediate);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    onStep(endValue);

    if (onEnd != null) {
      onEnd();
    }
  }

  public static IEnumerator CoEaseBackInOut(float targetTime, float startValue, float endValue, System.Action<float> onStep, System.Action onEnd = null) {
    float elapsedTime = 0;
    float startTime = Time.time;
	float s = 1.70158f;
	float u = s * 1.525f;

    while (elapsedTime < targetTime) {
	  float t = elapsedTime / (0.5f * targetTime);
	  float interpolated;
	  if (t < 1) {
	    interpolated = (endValue - startValue) * 0.5f * t * t * ((u + 1) * t - u) + startValue;
	  } else {
		t -= 2.0f;
	    interpolated = (endValue - startValue) * 0.5f * (t * t * ((u + 1) * t + u) + 2) + startValue;
	  }
      onStep(interpolated);
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    onStep(endValue);

    if (onEnd != null) {
      onEnd();
    }
  }

  public static IEnumerator CoLerp(float targetTime, float startValue, float endValue, System.Action<float> onStep, System.Action onEnd = null) {
    float elapsedTime = 0;
    float startTime = Time.time;

    while (elapsedTime < targetTime) {
      float proportion = elapsedTime / targetTime;
      onStep(Mathf.Lerp(startValue, endValue, proportion));
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    onStep(endValue);

    if (onEnd != null) {
      onEnd();
    }
  }

  public static IEnumerator CoLerp(float targetTime, Vector2 startValue, Vector2 endValue, System.Action<Vector2> onStep, System.Action onEnd = null) {
    float elapsedTime = 0;
    float startTime = Time.time;

    while (elapsedTime < targetTime) {
      float proportion = elapsedTime / targetTime;
      onStep(Vector2.Lerp(startValue, endValue, proportion));
      yield return null;
      elapsedTime = Time.time - startTime;
    }

    onStep(endValue);

    if (onEnd != null) {
      onEnd();
    }
  }

  public static void Register(this EventTrigger eventTrigger, EventTriggerType eventType, UnityAction<BaseEventData> action) {
	EventTrigger.Entry entry = new EventTrigger.Entry();
	entry.eventID = eventType;
	entry.callback.AddListener(action);
	eventTrigger.triggers.Add(entry);
  }

  public static void Each<T>(this IEnumerable<T> collection, System.Action<T, int> action) {
    int i = 0;
    foreach (T t in collection) {
      action(t, i);
	  i += 1;
    }
  }

  public static string GetAbsolutePath(Transform node)
  {
	  string path = "/" + node.transform.name;
	  while (node.parent != null)
	  {
		  node = node.parent;
		  path = "/" + node.name + path;
	  }
	  return path;
  }

  public static Transform FindInactiveParent(Transform node)
  {
	  if (node == null)
	  {
		  return null;
	  }
	  else if (!node.gameObject.activeSelf)
	  {
		  return node;
	  }
	  else
	  {
		  return FindInactiveParent(node.parent);
	  }
  }

  public static List<int> ToIntList(this List<bool?> list) {
	return list.Select(x => x.HasValue ? System.Convert.ToInt32(x.Value) : -1).ToList();
  }

  public static List<bool?> ToNullableBoolList(this List<int> list) {
	return list.Select(x => {
		bool? val;
		if (x == 0)
		{
			val = false;
		}
		else if (x == 1)
		{
			val = true;
		}
		else
		{
			val = null;
		}
		return val;
	}).ToList();
  }
}
