using System;

namespace InMemoryCache
{
    public class FastSlowGet
    {
        public static T Get<T>(Func<T> getFast, Func<T> getSlow)
            where T : class
        {
            T item = getFast();

            if (item != null)
            {
                return item;
            }

            {
                item = getFast();

                if (item != null)
                {
                    return item;
                }

                item = getSlow();

                return item;
            }
        }
    }
}