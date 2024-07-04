using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
    // ネイティブ関数を使ってARPテーブルを操作するためのインポート
    [DllImport("iphlpapi.dll", ExactSpelling = true)]
    public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref int PhyAddrLen);

    public static UdpClient InitializeUdpClient()
    {
        return new UdpClient();
    }

    public static string BroadcastAndGetMacAddress(UdpClient udpClient)
    {
        string macAddress = null;

        try
        {
            // ブロードキャストメッセージ
            string message = "Discover";
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);

            // ブロードキャストアドレスにメッセージを送信
            IPEndPoint broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 50000);
            udpClient.Send(sendBytes, sendBytes.Length, broadcastEndPoint);
            Console.WriteLine("Broadcast message sent.");

            // 応答を受信
            IPEndPoint receiveEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receiveBytes = udpClient.Receive(ref receiveEndPoint);
            string response = Encoding.UTF8.GetString(receiveBytes);
            Console.WriteLine($"Received response: {response}");

            // 応答からMACアドレスを抽出（ここでは応答が単純にMACアドレスを含むと仮定）
            macAddress = response;
            Console.WriteLine($"Extracted MAC Address: {macAddress}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        return macAddress;
    }

    public static void AddMacAddressToArpTable(string ipAddress, string macAddress)
    {
        byte[] macAddrBytes = new byte[6];
        string[] macAddrHex = macAddress.Split(':');

        for (int i = 0; i < 6; i++)
        {
            macAddrBytes[i] = Convert.ToByte(macAddrHex[i], 16);
        }

        int addrLen = macAddrBytes.Length;
        int destIp = BitConverter.ToInt32(IPAddress.Parse(ipAddress).GetAddressBytes(), 0);

        int result = SendARP(destIp, 0, macAddrBytes, ref addrLen);
        if (result == 0)
        {
            Console.WriteLine("ARP table updated successfully.");
        }
        else
        {
            Console.WriteLine("Failed to update ARP table.");
        }
    }

    public static void CommunicateUsingArp(UdpClient udpClient, string ipAddress)
    {
        try
        {
            // 送信メッセージ
            string message = "Hello, Program B!";
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);

            // IPアドレスにメッセージを送信
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), 50000);
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
    }

    public static void Main()
    {
        UdpClient udpClient = InitializeUdpClient();
        string ipAddress = "127.0.0.1"; // 通信対象のIPアドレス（仮）

        // 初回ブロードキャスト通信
        string macAddress = BroadcastAndGetMacAddress(udpClient);

        if (!string.IsNullOrEmpty(macAddress))
        {
            // ARPテーブルにMACアドレスを追加
            AddMacAddressToArpTable(ipAddress, macAddress);

            // ２回目以降の通信
            CommunicateUsingArp(udpClient, ipAddress);
        }
    }
}