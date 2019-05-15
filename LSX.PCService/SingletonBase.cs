using System.Reflection;

namespace System
{
    public abstract class SingletonBase<T> where T : class
    {
        private static class NestetContainer
        {
            static NestetContainer()
            {
                try
                {
                    INSTANCE =
                        Activator.CreateInstance(typeof(T), BindingFlags.Instance | BindingFlags.NonPublic, null,
                                                    new object[] { }, null) as T;
                }
                catch (MissingMethodException)
                {
                    INSTANCE = Activator.CreateInstance(typeof(T)) as T;
                }
            }

            public static readonly T INSTANCE;
        }

        public static T Instance
        {
            get
            {
                return NestetContainer.INSTANCE;
            }
        }
    }
}
