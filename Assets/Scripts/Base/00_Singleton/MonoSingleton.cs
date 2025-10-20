using Cysharp.Threading.Tasks;
using UnityEngine;

//============================================================//
// ½Ì±ÛÅæ ( Life Cycle )                                      
// Instance »ý¼º
// OnInitalizing
// OnInitalized
//============================================================//

public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
{
    #region Fields

    /// <summary>
    /// The instance.
    /// </summary>
    private static T instance;

    /// <summary>
    /// The initialization status of the singleton's instance.
    /// </summary>
    private SingletonStatus initializationStatus = SingletonStatus.None;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    instance = obj.AddComponent<T>();
                    instance.OnMonoSingletonCreated();
                }
            }
            return instance;
        }
    }

    /// <summary>
    /// Gets whether the singleton's instance is initialized.
    /// </summary>
    public virtual bool IsInitialized => this.initializationStatus == SingletonStatus.Initialized;

    #endregion

    #region Unity Messages

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            // Initialize existing instance
            InitializeSingleton();
        }
        else
        {

            // Destory duplicates
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// This gets called once the singleton's instance is created.
    /// </summary>
    protected virtual void OnMonoSingletonCreated()
    {

    }

    protected virtual async UniTask OnInitializing()
    {

    }

    protected virtual async UniTask OnInitialized()
    {

    }

    #endregion

    #region Public Methods

    public virtual void InitializeSingleton()
    {
        if (this.initializationStatus != SingletonStatus.None)
        {
            return;
        }

        this.initializationStatus = SingletonStatus.Initializing;
        OnInitializing();
        this.initializationStatus = SingletonStatus.Initialized;
        OnInitialized();
    }

    public virtual void ClearSingleton() { }

    public static void CreateInstance()
    {
        DestroyInstance();
        instance = Instance;
    }

    public static void DestroyInstance()
    {
        if (instance == null)
        {
            return;
        }

        instance.ClearSingleton();
        instance = default(T);
    }

    public static bool GetReference()
    {
        if (ReferenceEquals(instance, null)) return false;

        return true;
    }

    UniTask ISingleton.InitializeSingleton()
    {
        throw new System.NotImplementedException();
    }
    #endregion

}