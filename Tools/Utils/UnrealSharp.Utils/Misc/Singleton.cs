namespace UnrealSharp.Utils.Misc
{
    /// <summary>
    /// Class Singleton.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// The s instance
        /// </summary>
        private static T? s_instance;

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
            if (s_instance == null)
            {
                s_instance = new T();

                (s_instance as Singleton<T>)!.Init();
            }
        }

        /// <summary>
        /// Destroys the instance.
        /// </summary>
        public static void DestroyInstance()
        {
            if (s_instance != null)
            {
                (s_instance as Singleton<T>)!.UnInit();
                s_instance = null;
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
                if (s_instance == null)
                {
                    CreateInstance();
                }

                return s_instance!;
            }
        }


        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns>T.</returns>
        public static T GetInstance()
        {
            if (s_instance == null)
            {
                CreateInstance();
            }

            return s_instance!;
        }

        /// <summary>
        /// Determines whether this instance has instance.
        /// </summary>
        /// <returns><c>true</c> if this instance has instance; otherwise, <c>false</c>.</returns>
        public static bool HasInstance()
        {
            return (s_instance != null);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Uns the initialize.
        /// </summary>
        public virtual void UnInit()
        {

        }
    };


    /// <summary>
    /// Class SingletonExtend.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TParamType">The type of the t parameter type.</typeparam>
    public class SingletonExtend<T, TParamType>
        where T : class, new()
    {
        /// <summary>
        /// The s instance
        /// </summary>
        private static T? s_instance;

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
            if (s_instance == null)
            {
                s_instance = new T();

                (s_instance as SingletonExtend<T, TParamType>)!.Init(param);
            }
        }

        /// <summary>
        /// Destroys the instance.
        /// </summary>
        public static void DestroyInstance()
        {
            if (s_instance != null)
            {
                (s_instance as SingletonExtend<T, TParamType>)!.UnInit();
                s_instance = null;
            }
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <returns>T.</returns>
        public static T GetInstance(TParamType param)
        {
            if (s_instance == null)
            {
                CreateInstance(param);
            }

            return s_instance!;
        }

        /// <summary>
        /// Determines whether this instance has instance.
        /// </summary>
        /// <returns><c>true</c> if this instance has instance; otherwise, <c>false</c>.</returns>
        public static bool HasInstance()
        {
            return (s_instance != null);
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public virtual void Init(TParamType param)
        {

        }

        /// <summary>
        /// Uns the initialize.
        /// </summary>
        public virtual void UnInit()
        {

        }
    };

}
