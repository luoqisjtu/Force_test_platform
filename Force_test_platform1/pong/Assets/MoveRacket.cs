using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.IO;


public class MoveRacket : MonoBehaviour
{

    public float speed = 30;

    byte[] data = new byte[1024];
    string input, stringData;

    Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);  //实现 Berkeley 套接字接口
    public IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);  //定义服务端

    EndPoint Remote;
    int recv;
    

    static readonly object lockObject = new object();
    string returnData = "";
    bool precessData = false;


    List<float> listToHoldData;
    List<float> listToHoldTime;

    void Start()
    {
        Console.WriteLine("This is a Client, host name is {0}", Dns.GetHostName());//获取本地计算机的主机名
        IPEndPoint ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8001);
        

        string welcome = "你好! ";
        data = Encoding.ASCII.GetBytes(welcome);  //数据类型转换
        server.SendTo(data, data.Length, SocketFlags.None, ip);  //发送给指定服务端

        Remote = (EndPoint)sender;
        recv = server.ReceiveFrom(data, ref Remote);//获取客户端，获取客户端数据，用引用给客户端赋值 
        data = new byte[1024];

        listToHoldData = new List<float>();
        listToHoldTime = new List<float>();



        //thread = new Thread(new ThreadStart(ThreadMethod));
        //thread.Start();

    }

    void FixedUpdate()
    {

            
        server.SendTo(Encoding.ASCII.GetBytes("H"), Remote);//发送信息
        data = new byte[1024];//对data清零
        recv = server.ReceiveFrom(data, ref Remote);//获取客户端，获取服务端端数据，用引用给服务端赋值，实际上服务端已经定义好并不需要赋值
        stringData = Encoding.ASCII.GetString(data, 0, recv);//字节数组转换为字符串  //输出接收到的数据 
        Console.WriteLine(stringData);


        float v = Input.GetAxisRaw("Vertical");
        float barHeight = 0.01f * Convert.ToInt32(stringData) - 0.1f;
        GetComponent<Rigidbody2D>().position = new Vector2(0, barHeight);


        listToHoldData.Add(barHeight);
           //float t = Time.time;
        listToHoldTime.Add(Time.time);

        //Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        //GetComponent<Rigidbody2D>().position = new Vector2(0, mousePosition.y);
        

    }

    private void OnApplicationQuit()
    {


        //string data = "";
        //StreamWriter writer = new StreamWriter("test3.csv", false, Encoding.UTF8);

        //foreach (float eachBarHeight in listToHoldData)
        //{
        //    data += eachBarHeight.ToString();
        //    data += "\n";
        //}

        //writer.Write(data);

        //writer.Close();


        string data = "";
        StreamWriter writer = new StreamWriter("test.csv", false, Encoding.UTF8);


        using (var e1 = listToHoldTime.GetEnumerator())
        using (var e2 = listToHoldData.GetEnumerator())
        {
            while (e1.MoveNext() && e2.MoveNext())
            {
                var item1 = e1.Current;
                var item2 = e2.Current;

                data += item1.ToString();
                data += ",";
                data += item2.ToString();
                data += "\n";
                // use item1 and item2
            }
        }


        writer.Write(data);

        writer.Close();
    }

}
