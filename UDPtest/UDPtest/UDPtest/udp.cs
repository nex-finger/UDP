using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace UDPtest
{
    public class udp
    {
        public static int Comm()
        {
            UdpClient udpClient = new UdpClient();
            try
            {
                // 送信メッセージ
                string message = "Hello, Program B!";
                byte[] sendBytes = Encoding.UTF8.GetBytes(message);

                // 127.0.0.1、ポート50000にメッセージを送信
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 50000);
                udpClient.Send(sendBytes, sendBytes.Length, remoteEndPoint);
                Console.WriteLine($"Sent: {message}");

                // 返答を受信
                IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = udpClient.Receive(ref receiveEndPoint);
                string returnMessage = Encoding.UTF8.GetString(receiveBytes);
                Console.WriteLine($"Received: {returnMessage}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                udpClient.Close();
            }

            return 0;
        }
    }
}
