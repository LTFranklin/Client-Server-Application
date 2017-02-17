using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Net;

namespace location
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient();
            client.Connect("localhost", 43);
            //client.Connect("whois.networksolutions.com", 43);
            //client.Connect("whois.net.dcs.hull.ac.uk", 43);
            StreamWriter streamWrite = new StreamWriter(client.GetStream());
            StreamReader streamRead = new StreamReader(client.GetStream());
            NetworkStream netStream = client.GetStream();

            try
            {
                if (args[args.Length - 1] == "/h9")
                {
                    h09Pro(args, streamWrite);
                }
                else if (args[args.Length - 1] == "/h0")
                {
                    h10Pro(args, streamWrite);
                }
                else if (args[args.Length - 1] == "/h1")
                {
                    h11Pro(args, streamWrite);
                }
                else
                {
                    whoisPro(args, streamWrite);
                }

                streamWrite.Flush();
                Console.WriteLine(streamRead.ReadToEnd());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static public void whoisPro(string[] args, StreamWriter streamWrite)
        {
            if (args.Length == 2)
            {
                streamWrite.Write(args[0] + " " + args[1] + "\n");
            }
            else
            {
                streamWrite.Write(args[0] + "\n");
            }
        }

        static public void h09Pro(string[] args, StreamWriter streamWrite)
        {
            if (args.Length == 3)
            {
                streamWrite.Write("PUT /" + args[0] + "\n\n" + args[1] +"\n");
            }
            else
            {
                streamWrite.Write("GET /" + args[0] + "\n");
            }
        }

        static public void h10Pro(string[] args, StreamWriter streamWrite)
        {
            if (args.Length == 3)
            {
                streamWrite.Write("POST /" + args[0] + " HTTP/1.0\nContent-Length: " + args[1].Length + "\n" + args[1]);
            }
            else
            {
                streamWrite.Write("GET /?" + args[0] + " HTTP/1.0\n");
            }
        }

        static public void h11Pro(string[] args, StreamWriter streamWrite)
        {
            if (args.Length == 3)
            {
                streamWrite.Write("POST / HTTP/1.1\nHost: " + Dns.GetHostName() + "\nContent-Length: " + (args[0].Length+args[1].Length) + "\nname=" + args[0] + "&location=" + args[1]);
            }
            else
            {
                streamWrite.Write("GET /?name=" + args[0] + " HTTP/1.1\nHost: " + Dns.GetHostName() + "\n");
            }
        }
    }
}
