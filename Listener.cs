using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Moody.WebServer
{
    public static class WebServer
    {
        /// <summary>
        /// web server
        /// </summary>
        private static HttpListener listener;
        public static int maxSimulConnections = 20;
        private static Semaphore sem = new Semaphore(maxSimulConnections, maxSimulConnections);

        /// <summary>
        /// returns lists of IP addresses
        /// </summary>
        /// <returns></returns>
        private static List<IPAddress> GetLocalHostIPS()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            List<IPAddress> ret = host.AddressList
                                      .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                                      .ToList();
            return ret;
        }

        private static HttpListener InitializeListiner(List<IPAddress> localhostIPs)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            localhostIPs.ForEach(ip =>
            {
                Console.WriteLine($"Listening on IP http://{ip}/");
                listener.Prefixes.Add($"http://{ip}/");
            });

            return listener;
        }

        private static void Start(HttpListener listener)
        {
            listener.Start();
            Task.Run(() => RunServer(listener));
        }

        /// <summary>
        /// start awaiting for connections, up to the "maxSimulConnections"
        /// value 
        /// runs on a separate thread
        /// </summary>
        /// <param name="listener"></param>
        private static void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnetionListener(listener);
            }
        }

        private static async Task StartConnetionListener(HttpListener listener)
        {
            HttpListenerContext context = await listener.GetContextAsync();
            sem.Release();

            string response = "Hello Browser!";
            byte[] encoded = Encoding.UTF8.GetBytes(response);
            context.Response.ContentLength64 = encoded.Length;
            context.Response.OutputStream.Write(encoded, 0, encoded.Length);
            context.Response.OutputStream.Close();
        }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        public static void Start()
        {
            List<IPAddress> localHostIPs = GetLocalHostIPS();
            HttpListener listener = InitializeListiner(localHostIPs);
            Start(listener);
        }
    }
}