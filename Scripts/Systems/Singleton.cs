using UnityEngine;

//public abstract class Singleton<T> where T : new()
//{
//    private static T s_instance = default(T);
//    public static T Instance
//    {
//        get
//        {
//            if (s_instance == null)
//                s_instance = new T();

//            return s_instance;
//        }
//    }

//    protected virtual void Release()
//    {
//        s_instance = default(T);
//    }
//}
public abstract class Singleton<T> where T : class, new()
{
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = new T();
            }

            return s_instance;
        }
    }

    public static bool HasInstance => s_instance != null;

    public static void ReleaseInstance()
    {
        s_instance = null;
    }
}

//public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
//{
//    private static T s_instance = null;
//    public static T Instance
//    {
//        get
//        {
//            // 인스턴스가 아직 없으면
//            if (object.ReferenceEquals(s_instance, null))
//            {
//                s_instance = FindObjectOfType<T>();
//                if (s_instance == null)
//                {
//                    string name = typeof(T).Name;

//                    GameObject gObj = GameObject.Find(name);
//                    if (gObj == null)
//                    {
//                        gObj = new GameObject();
//                        gObj.name = name;
//                    }

//                    s_instance = gObj.AddComponent<T>();
//                }
//            }
//            return s_instance;
//        }
//    }


    //protected virtual void DontDestroyOnLoad()
    //{
    //    DontDestroyOnLoad(gameObject);
    //}

    //protected virtual void OnDestroy()
    //{
    //    Debug.Log("싱글턴 파괴: OnDestroy");
    //    s_instance = null;
    //}

    //protected virtual void OnApplicationQuit()
    //{
    //    Debug.Log("싱글턴 파괴: OnApplicationQuit");
    //    s_instance = null;
    //}
//}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_instance;
    private static bool s_isQuitting;

    public static T Instance
    {
        get
        {
            if (s_isQuitting)
            {
                return null;
            }

            if (s_instance == null)
            {
#if UNITY_2023_1_OR_NEWER
                s_instance = FindFirstObjectByType<T>();
#else
                s_instance = FindObjectOfType<T>();
#endif

                if (s_instance == null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    s_instance = go.AddComponent<T>();
                }
            }

            return s_instance;
        }
    }

    public static bool HasInstance => s_instance != null;

    protected virtual bool IsDontDestroyOnLoad => false;

    protected virtual void Awake()
    {
        if (s_instance != null && s_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        s_instance = this as T;

        if (IsDontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    protected virtual void OnDestroy()
    {
        if (s_instance == this)
        {
            s_instance = null;
        }
    }

    protected virtual void OnApplicationQuit()
    {
        s_isQuitting = true;
    }
}