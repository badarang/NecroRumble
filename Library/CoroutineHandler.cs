using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CoroutineHandler : MonoBehaviour
{
    private static MonoBehaviour _instance;

    //private IEnumerator _enumerator = null;

    public static void Init()
    {
        _instance = new GameObject($"[{nameof(CoroutineHandler)}]").AddComponent<CoroutineHandler>();
        DontDestroyOnLoad(_instance.gameObject);
    }

    public new static Coroutine StartCoroutine(IEnumerator coroutine_)
    {
        return _instance.StartCoroutine(coroutine_);
    }

    public new static void StopCoroutine(Coroutine coroutine_)
    {
        _instance?.StopCoroutine(coroutine_);
    }

    public new static void StopAllCoroutines()
    {
        _instance?.StopAllCoroutines(); // null in TitleScene
    }
}
