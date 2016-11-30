using UnlimitedOutsideConnections.Redirection.Extensions;

namespace UnlimitedOutsideConnections.Redirection
{
    public class Redirector<T>
    {
        public static void Deploy()
        {
            typeof(T).Redirect();
        }

        public static void Revert()
        {
            typeof(T).Revert();
        }

        public static bool IsDeployed()
        {
            return typeof(T).IsRedirected();
        }
    }
}