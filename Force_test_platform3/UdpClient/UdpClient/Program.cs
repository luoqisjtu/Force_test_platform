using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace UdpClient
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] data = new byte[1024];
            string input, stringData;

            //构建TCP 服务器
            Console.WriteLine("This is a Client, host name is {0}", Dns.GetHostName());//获取本地计算机的主机名

            //设置服务IP，设置TCP端口号
            IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001); //定义连接的服务器ip和端口，可以是本机ip，局域网，互联网 
                                                                                    //一台机器可以配置多个网络IP接口，每个有唯一的IP地址，以及接口名字(可能有别名alias)
                                                                                   //127.0.0.1             localhost    //自返回接口
                                                                                   //192.9.168.112         xibo         //该机器在局哉网内的接口
                                                                                   //202.96.128.68         www.test.com //该机器在互联网上的接口

            //定义网络类型，数据连接类型和网络协议UDP
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);  //实现 Berkeley 套接字接口

            string welcome = "你好! ";
            data = Encoding.ASCII.GetBytes(welcome);  //数据类型转换
            server.SendTo(data, data.Length, SocketFlags.None, ip);  //发送给指定服务端
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);  //定义服务端
            EndPoint Remote = (EndPoint)sender;//标识网络地址, EndPoint类提供了一个表示网络资源或服务的 abstract 基类,子类组合网络连接信息以构成服务的连接点。

            data = new byte[1024];
            //对于不存在的IP地址，加入此行代码后，可以在指定时间内解除阻塞模式限制
            int recv = server.ReceiveFrom(data, ref Remote);//获取客户端，获取客户端数据，用引用给客户端赋值 
            Console.WriteLine("Message received from {0}: ", Remote.ToString());//从服务端接收数据
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));// 字节数组转换成字符串,输出接收到的数据
            while (true)//进入接收循环
            {
                input = Console.ReadLine();//定义读入输入
                if (input == "exit")
                    break;
                server.SendTo(Encoding.ASCII.GetBytes(input), Remote);//发送信息
                data = new byte[1024];//对data清零
                recv = server.ReceiveFrom(data, ref Remote);//获取客户端，获取服务端端数据，用引用给服务端赋值，实际上服务端已经定义好并不需要赋值
                stringData = Encoding.ASCII.GetString(data, 0, recv);//字节数组转换为字符串  //输出接收到的数据 
                Console.WriteLine(stringData);
            }
            Console.WriteLine("Stopping Client.");
            server.Close();
        }

    }
}