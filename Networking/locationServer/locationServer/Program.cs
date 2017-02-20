using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace locationServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Dictionary<string, string> dataStore = new Dictionary<string, string>();
            dataStore.Add("tim", "here");
            runServer(dataStore);
        }

        static public void runServer(Dictionary<string, string> dataStore)
        {
            TcpListener listener;
            //Socket connection;
            NetworkStream socketStream;
            TcpClient client;
            IPAddress localIP = Dns.Resolve("localhost").AddressList[0];

            try
            {
                listener = new TcpListener(localIP, 43);
                listener.Start();
                while (true)
                {
                    Console.WriteLine("Listening");
                    //connection = listener.AcceptSocket();
                    //Console.WriteLine(connection.Connected.ToString());
                    client = listener.AcceptTcpClient();
                    socketStream = client.GetStream();
                    doRequest(client, dataStore);
                    socketStream.Close();
                    //connection.Close();
                    //Console.WriteLine(connection.Connected.ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static public Dictionary<string, string> doRequest(TcpClient client, Dictionary<string, string> dataStore)
        {
            StreamReader streamRead = new StreamReader(client.GetStream());
            StreamWriter streamWrite = new StreamWriter(client.GetStream());
            string recieved = "";
            char[] rawIn = new char[250]; ;
            streamRead.Read(rawIn, 0, 250);
            for(int i = 0; i < 250; i++)
            {
                recieved = recieved + rawIn[i];
            }
            Console.WriteLine(recieved);
            match(recieved, dataStore, streamWrite);
            streamWrite.Flush();
            streamWrite.Close();
            return dataStore;
        }


        static public void match(string rawInput, Dictionary<string, string> dataStore, StreamWriter streamWrite)
        {
            string whoisFind = @"^\b([A-z0-9 ]+)\n";
            string whoisEdit = @"^\b([A-z0-9 ]+) ([A-z0-9 ]+)\n";
            string http09Find = @"^\bGET /([A-z0-9 ]+)\r\n";
            string http09Edit = @"^\bPUT /([A-z0-9 ]+)\r\n\r\n([A-z0-9 ]+)\r\n";
            string http10Find = @"^GET /\?([A-z0-9 ]+) HTTP/1.0[\nA-z0-9 ]*\r\n";
            string http10Edit = @"^POST /([A-z0-9 ]+) HTTP/1.0\r\nContent-Length: [0-9]+[\r\nA-z0-9 ]*\r\n([A-z0-9 ]+)";
            string http11Find = @"GET /\?name=([A-z0-9 ]+) HTTP/1.1\nHost: [A-z0-9\- ]+\n";
            string http11Edit = @"^POST / HTTP/1.1\nHost: [A-z0-9\- ]+\nContent-Length: [0-9]+[\nA-z0-9 ]*\nname=([A-z0-9 ]+)&location=([A-z0-9]+)";


            if (Regex.IsMatch(rawInput, whoisFind))
            {
                foreach(Match info in Regex.Matches(rawInput, whoisFind))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == "fail")
                    {
                        streamWrite.Write("ERROR: no entries found\r\n");
                    }
                    else
                    {
                        streamWrite.Write(record + "\r\n");
                    }
                }
            }
            else if (Regex.IsMatch(rawInput, whoisEdit))
            {
                foreach (Match info in Regex.Matches(rawInput, whoisEdit))
                {
                    editRecord(dataStore, info.Groups[1].ToString(), info.Groups[2].ToString());
                    streamWrite.Write("OK\r\n");
                }
            }
            else if (Regex.IsMatch(rawInput, http09Find))
            {
                foreach (Match info in Regex.Matches(rawInput, http09Find))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == "fail")
                    {
                        streamWrite.Write("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    }
                    else
                    {
                        streamWrite.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + record + "\r\n");
                    }
                }
            }
            else if (Regex.IsMatch(rawInput, http09Edit))
            {
                foreach (Match info in Regex.Matches(rawInput, http09Edit ))
                {
                    editRecord(dataStore, info.Groups[1].ToString(), info.Groups[2].ToString());
                    streamWrite.Write("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                }
            }
            else if (Regex.IsMatch(rawInput, http10Find))
            {
                foreach(Match info in Regex.Matches(rawInput, http10Find))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == "fail")
                    {
                        streamWrite.Write("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    }
                    else
                    {
                        streamWrite.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + record + "\r\n");
                    }
                }
            }
            else if (Regex.IsMatch(rawInput, http10Edit))
            {
                foreach (Match info in Regex.Matches(rawInput, http10Edit))
                {
                    editRecord(dataStore, info.Groups[1].ToString(), info.Groups[2].ToString());
                    streamWrite.Write("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                }
            }
            else if (Regex.IsMatch(rawInput, http11Find))
            {
                foreach (Match info in Regex.Matches(rawInput, http11Find))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == "fail")
                    {
                        streamWrite.Write("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                    }
                    else
                    {
                        streamWrite.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + record + "\r\n");
                    }
                }
            }
            else if (Regex.IsMatch(rawInput, http11Edit))
            {
                foreach (Match info in Regex.Matches(rawInput, http11Edit))
                {
                    editRecord(dataStore, info.Groups[1].ToString(), info.Groups[2].ToString());
                    streamWrite.Write("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                }
            }
        }

        static public Dictionary<string,string> editRecord(Dictionary<string,string> dataStore, string name, string location)
        {
            if (dataStore.ContainsKey(name))
            {
                dataStore[name] = location;
            }
            else
            {
                dataStore.Add(name, location);
            }
            return dataStore;
        }

        static public string findRecord(Dictionary<string, string> dataStore, string name)
        {
            if (dataStore.ContainsKey(name))
            {
                return dataStore[name];
            }
            else
            {
                return "fail";
            }
        }
    }
}
