using NETWORKLIST;

namespace Projects_Manager.Models
{
    public class InternetConnectionChecker
    {
        private readonly INetworkListManager networkListManager;

        public InternetConnectionChecker()
        {
            networkListManager = new NetworkListManager();
        }

        public bool IsConnected()
        {
            return networkListManager.IsConnectedToInternet;
        }
    }
}
