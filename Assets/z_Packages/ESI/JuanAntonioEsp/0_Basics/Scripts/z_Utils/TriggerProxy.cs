using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class TriggerProxy : MonoBehaviour {

	[field: Header("Settings")]
	[field: SerializeField] public bool usingTag { get; set; }
	[field: SerializeField, TagSelector] public string tagToCompare { get; set; }

	[field: Header("Trigger Events")]
	[field: SerializeField] public UnityEvent onTriggerEnter { get; set; }
	[field: SerializeField] public UnityEvent onTriggerExit { get; set; }
	[field: SerializeField] public UnityEvent onTriggerEnter2D { get; set; }
	[field: SerializeField] public UnityEvent onTriggerExit2D { get; set; }

    [field: Header("Collision Events")]
    [field: SerializeField] public UnityEvent onCollisionEnter { get; set; }
    [field: SerializeField] public UnityEvent onCollisionExit { get; set; }

    [field: Header("Coroutines Events")]
    [field: SerializeField] public UnityEvent onCoroutine { get; set; }

    #region Trigger Methods
    private void OnTriggerEnter(Collider other) {
        if (usingTag) {
            if (other.CompareTag(tagToCompare)) {
                onTriggerEnter.Invoke();
            }
        }
        else {
            onTriggerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (usingTag) {
            if (other.CompareTag(tagToCompare)) {
                onTriggerExit.Invoke();
            }
        }
        else {
            onTriggerExit.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (usingTag) {
            if (other.CompareTag(tagToCompare)) {
                onTriggerEnter2D.Invoke();
            }
        }
        else {
            onTriggerEnter2D.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (usingTag) {
            if (other.CompareTag(tagToCompare)) {
                onTriggerExit2D.Invoke();
            }
        }
        else {
            onTriggerExit2D.Invoke();
        }
    }
    #endregion

    #region Collision Methods

    private void OnCollisionEnter(Collision other) {
        //Debug.Log($"OnCollisionEnter: {other.gameObject.name}");

        if (usingTag) {
            if (other.collider.CompareTag(tagToCompare)) {
                onCollisionEnter.Invoke();
            }
        }
        else {
            onCollisionEnter.Invoke();
        }
    }

    private void OnCollisionExit(Collision other) {
        if (usingTag) {
            if (other.collider.CompareTag(tagToCompare)) {
                onCollisionExit.Invoke();
            }
        }
        else {
            onCollisionExit.Invoke();
        }
    }

    #endregion

    #region Coroutines Methods

    public void StartCoroutineEvent(float delay) {
        StartCoroutine(ExecuteAfterTime(delay));
    }

    IEnumerator ExecuteAfterTime(float delay) {
        yield return new WaitForSeconds(delay);
        onCoroutine.Invoke();
    }

    #endregion
}