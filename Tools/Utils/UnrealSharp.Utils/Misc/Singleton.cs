namespace UnrealSharp.Utils.Misc;

/// <summary>
/// Class Singleton.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Singleton<T> where T : class, new()
{
    /// <summary>
    /// The s instance
    /// </summary>
    private static T? _sInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="Singleton{T}"/> class.
    /// </summary>
    protected Singleton()
    {
    }

    /// <summary>
    /// Creates the instance.
    /// </summary>
    public static void CreateInstance()
    {
        if (_sInstance == null)
        {
            _sInstance = new T();

            (_sInstance as Singleton<T>)!.Init();
        }
    }

    /// <summary>
    /// Destroys the instance.
    /// </summary>
    public static void DestroyInstance()
    {
        if (_sInstance != null)
        {
            (_sInstance as Singleton<T>)!.UnInit();
            _sInstance = null;
        }
    }

    /// <summary>
    /// Gets the Instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (_sInstance == null)
            {
                CreateInstance();
            }

            return _sInstance!;
        }
    }


    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <returns>T.</returns>
    public static T GetInstance()
    {
        if (_sInstance == null)
        {
            CreateInstance();
        }

        return _sInstance!;
    }

    /// <summary>
    /// Determines whether this instance has instance.
    /// </summary>
    /// <returns><c>true</c> if this instance has instance; otherwise, <c>false</c>.</returns>
    public static bool HasInstance()
    {
        return _sInstance != null;
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    protected virtual void Init()
    {

    }

    /// <summary>
    /// Uns the initialize.
    /// </summary>
    protected virtual void UnInit()
    {

    }
}


/// <summary>
/// Class SingletonExtend.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TParamType">The type of the t parameter type.</typeparam>
public abstract class SingletonExtend<T, TParamType>
    where T : class, new()
{
    /// <summary>
    /// The s instance
    /// </summary>
    private static T? _sInstance;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingletonExtend{T, TParamType}"/> class.
    /// </summary>
    protected SingletonExtend()
    {

    }

    /// <summary>
    /// Creates the instance.
    /// </summary>
    /// <param name="param">The parameter.</param>
    public static void CreateInstance(TParamType param)
    {
        if (_sInstance == null)
        {
            _sInstance = new T();

            (_sInstance as SingletonExtend<T, TParamType>)!.Init(param);
        }
    }

    /// <summary>
    /// Destroys the instance.
    /// </summary>
    public static void DestroyInstance()
    {
        if (_sInstance != null)
        {
            (_sInstance as SingletonExtend<T, TParamType>)!.UnInit();
            _sInstance = null;
        }
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <returns>T.</returns>
    public static T GetInstance(TParamType param)
    {
        if (_sInstance == null)
        {
            CreateInstance(param);
        }

        return _sInstance!;
    }

    /// <summary>
    /// Determines whether this instance has instance.
    /// </summary>
    /// <returns><c>true</c> if this instance has instance; otherwise, <c>false</c>.</returns>
    public static bool HasInstance()
    {
        return _sInstance != null;
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    protected abstract void Init(TParamType param);

    /// <summary>
    /// Uns the initialize.
    /// </summary>
    protected virtual void UnInit()
    {

    }
}