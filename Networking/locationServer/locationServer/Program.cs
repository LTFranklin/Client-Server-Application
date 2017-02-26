﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace locationServer
{
    static class data
    {
        static public Dictionary<string, string> dataStore = new Dictionary<string, string>();
        //static public ThreadStart myThreadStart;
      
    }

    class Program
    {

        static void Main(string[] args)
        {

            data.dataStore.Add("tim", "here");
            runServer();

            //runServer(dataStore);
        }

        static public void runServer()
        {
            handler requestHandler;
            TcpListener listener;
            TcpClient client;
            IPAddress localIP = Dns.Resolve("localhost").AddressList[0];
            listener = new TcpListener(localIP, 43);
            listener.Start();
            while (true)
            {
                Console.WriteLine("Listening");
                client = listener.AcceptTcpClient();
                requestHandler = new handler();
                Thread myThread = new Thread(() => requestHandler.doRequest(client, data.dataStore));
                myThread.Start();
            }
        }
    }

    class handler
    { 
        public Dictionary<string, string> doRequest(TcpClient client, Dictionary<string, string> dataStore)
        {
            Thread.Sleep(2000);
            NetworkStream socketStream;
            socketStream = client.GetStream();
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
            socketStream.Close();
            return dataStore;
        }

        //Uses regular expressions to match the recieved string to its protocol
        public void match(string rawInput, Dictionary<string, string> dataStore, StreamWriter streamWrite)
        {
            //Any number of ASCII characters (expt space) followed by LFCR
            string whoisFind = @"^([!-~]*)\r\n";
            //Any number of ASCII characters (expt space) then a space then any number of ASCII characters followed by LFCR
            string whoisEdit = @"^([!-~]*) ([ -~]*)\r\n";
            //The sequence 'GET /', then any number of ASCII characters (expt space) followed by LFCR
            string http09Find = @"^GET /([!-~]*)\r\n";
            //The sequence 'PUT /', then any number of ASCII characters(expt space) followed by 2 LFCRs. Then any number of ASCII characters followed by LFCR
            string http09Edit = @"^PUT /([!-~]*)\r\n\r\n([ -~]*)\r\n";
            //The sequence 'GET /?', then any number of ASCII characters (expt space) followed by space HTTP/1.0 then LFCR
            string http10Find = @"^GET /\?([!-~]*) HTTP/1.0[\ -~]*\r\n";
            //The sequence 'POST /', any number of ASCII scharacter (expt space) then ' HTTP/1.0' LFCR 'Content-Length: ' a number then any number of characters LFCR and any number of characters
            string http10Edit = @"^POST /([!-~]*) HTTP/1.0\r\nContent-Length: [0-9]+[\r\n -~]*\r\n([ -~]*)";
            string http11Find = @"GET /\?name=([!-~]*) HTTP/1.1\r\nHost: [A-z0-9\- ]+\r\n";
            string http11Edit = @"^POST / HTTP/1.1\r\nHost: [!-~]+\r\nContent-Length: [0-9]+[\r\n -~]*\r\nname=([!-~]*)&location=([ -~]*)";


            if (Regex.IsMatch(rawInput, http09Find))
            {
                foreach (Match info in Regex.Matches(rawInput, http09Find))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == null)
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
                    if (record == null)
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
                    if (record == null)
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
            else if (Regex.IsMatch(rawInput, whoisFind))
            {
                foreach (Match info in Regex.Matches(rawInput, whoisFind))
                {
                    string record = (findRecord(dataStore, info.Groups[1].ToString()));
                    if (record == null)
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
            else
            {
                streamWrite.Write("foo");
            }
        }

        public Dictionary<string,string> editRecord(Dictionary<string,string> dataStore, string name, string location)
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

        public string findRecord(Dictionary<string, string> dataStore, string name)
        {
            if (dataStore.ContainsKey(name))
            {
                return dataStore[name];
            }
            else
            {
                return null;
            }
        }
    }
}
