using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PersisterTest : MonoBehaviour {
  public InputField jsonBox;

  public void PushJSON() {
    Persister.Persist(jsonBox.text);
  }
}
