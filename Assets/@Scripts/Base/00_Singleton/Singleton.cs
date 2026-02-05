
//============================================================//
// ½Ì±ÛÅæ ( Life Cycle ) 
// Instance »ý¼º
// OnInitalizing
// OnInitalized
//============================================================//

using Cysharp.Threading.Tasks;

public enum SingletonStatus
{
    None,
    Initializing,
    Initialized
}
public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
{
    #region Fields

    /// <summary>
    /// The instance.
    /// </summary>
    private static T instance;

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
                //ensure that only one thread can execute
                lock (typeof(T))
                {
                    if (instance == null)
                    {
                        instance = new T();
                    }
                }
            }

            return instance;
        }
    }

    /// <summary>
    /// Gets whether the singleton's instance is initialized.
    /// </summary>
    /// 
    #endregion

    #region Public Methods

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

    #endregion

}


