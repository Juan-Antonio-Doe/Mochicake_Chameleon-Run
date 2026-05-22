using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnityBugs : MonoBehaviour {

    [field: Header("Test Unity Bugs")]
    [field: Header("Bug: Inspector Error when working with Dinamyc Arrays")]
    [field: SerializeField] private bool testArrayListBug { get; set; } = false;
    [field: SerializeField] private float waitTime { get; set; } = 0.75f;
    [field: SerializeField] private List<GameObject> list { get; set; }
    [field: SerializeField] private GameObject[] array { get; set; }

    IEnumerator Start() {

        if (testArrayListBug) {
            list = new List<GameObject>();
            array = new GameObject[0];

            // Bucle infinito que rellena una lista con un elemento vacio cada 0.75 segundos.
            yield return new WaitForSeconds(1f);

            while (true) {
                list.Add(null);
                array = list.ToArray();
                yield return new WaitForSeconds(waitTime);
            }
        }
    }
}