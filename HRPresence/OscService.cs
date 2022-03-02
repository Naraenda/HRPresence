using System.Net;
using System.Net.Sockets;

namespace HRPresence
{
    class OscService
    {
        public UdpClient udp;

        public void Initialize(IPAddress ip, int port) {
            udp = new UdpClient();
            udp.Connect(ip, port);
        }

        public bool Update(int heartrate) {
            // Maps the heart rate from [0;255] to [-1;+1]
            var floatHR = (heartrate * 0.0078125f) - 1.0f;
            var data = new (string, object)[] {
                ("HR"        , heartrate),
                ("onesHR"    , (heartrate      ) % 10),
                ("tensHR"    , (heartrate / 10 ) % 10),
                ("hundredsHR", (heartrate / 100) % 10),
                ("floatHR"   , floatHR)
            };

            try {
                foreach (var (path, value) in data) {
                    var bytes = new OscCore.OscMessage($"/avatar/parameters/{path}", value).ToByteArray();
                    udp.Send(bytes, bytes.Length);
                }
            } catch {
                return false;
            }
            return true;
        }
    }
}
