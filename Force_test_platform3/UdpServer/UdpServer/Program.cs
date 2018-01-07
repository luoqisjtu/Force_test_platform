using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.IO;

using AnalogIO;
using MccDaq;
using ErrorDefs;

namespace UDP
{
    class Program
    {

        static MccDaq.MccBoard DaqBoard = new MccDaq.MccBoard(0);

        private int str_temp;
        private static MccDaq.Range Range;        //定义A/D和D/A转换范围

        const int NumPoints = 5000;     //  Number of data points to collect
        const int ArraySize = 5000;       //  size of data array
        private ushort[] DataBuffer;    //  declare data array
        string FileName;                //  name of file in which data will be stored

        AnalogIO.clsAnalogIO AIOProps = new AnalogIO.clsAnalogIO();


        int Count = NumPoints;
        //  it may be necessary to add path to file name for data file to be found
        int Rate = 1000;
        int LowChan = 0;
        int HighChan = 0;
        MccDaq.ScanOptions Options = MccDaq.ScanOptions.Default;

        private int NumAIChans;
        private int ADResolution;




        static void Main(string[] args)
        {
            int recv;
            byte[] data = new byte[1024];

            //得到本机IP，设置TCP端口号         
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, 8001);
            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            //绑定网络地址
            newsock.Bind(ip);

            Console.WriteLine("This is a Server, host name is {0}", Dns.GetHostName());

            //等待客户机连接
            Console.WriteLine("Waiting for a client");

            //得到客户机IP
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);
            recv = newsock.ReceiveFrom(data, ref Remote);
            Console.WriteLine("Message received from {0}: ", Remote.ToString());
            Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

            //客户机连接成功后，发送信息
            string welcome = "你好 ! ";

            //字符串与字节数组相互转换
            data = Encoding.ASCII.GetBytes(welcome);

            //发送信息
            newsock.SendTo(data, data.Length, SocketFlags.None, Remote);

          

            

            float EngUnits;
            double HighResEngUnits;
            MccDaq.ErrorInfo ULStat;//存储和报告错误代码和消息
            System.UInt16 DataValue;
            System.UInt32 DataValue32;
            int Chan = 0;     //输入通道编号
            int Options = 0;


            while (true)
            {
                data = new byte[1024];
                //发送接收信息
                recv = newsock.ReceiveFrom(data, ref Remote);
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

                ULStat = DaqBoard.AIn(Chan, Range, out DataValue);//读取输入通道，输出16位整数值
                //  Convert raw data to Volts by calling ToEngUnits
                //  (member function of MccBoard class)
                ULStat = DaqBoard.ToEngUnits(Range, DataValue, out EngUnits);//将原始数据转换成电压

                int barHeight = (int)Math.Ceiling(EngUnits * 1000 + 150);

                newsock.SendTo(Encoding.ASCII.GetBytes(barHeight.ToString()), Remote);


            }
        }

    }
}