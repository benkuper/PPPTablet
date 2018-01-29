//
//	  UnityOSC - Open Sound Control interface for the Unity3d game engine
//
//	  Copyright (c) 2012 Jorge Garcia Martin
//
// 	  Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// 	  documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// 	  the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
// 	  and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// 	  The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// 	  of the Software.
//
// 	  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// 	  TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// 	  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// 	  CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// 	  IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Threading;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace UnityOSC
{
	/// <summary>
	/// Dispatches OSC messages to the specified destination address and port.
	/// </summary>
	
	public class OSCTCPClient
	{

        public delegate void TCPClientEvent(OSCTCPClient client);
        public TCPClientEvent onConnected;
        public TCPClientEvent onRegistered;
        public TCPClientEvent onDisconnected;
        public delegate void TCPPacketReceivedEvent(OSCPacket packet);
        public TCPPacketReceivedEvent packetReceived;

        List<byte> buffer;

        #region Constructors
        public OSCTCPClient (IPAddress address, int port)
		{
			_ipAddress = address;
			_port = port;
            buffer = new List<byte>();

            Connect();
		}
		#endregion
		
		#region Member Variables
		private IPAddress _ipAddress;
		private int _port;
		private TcpClient _tcpClient;
        #endregion

        bool isConnected;
        bool lastConnected;
        public bool isRegistered;
        bool lastRegistered;

        Thread connectThread;

        float checkTime = 5;
        float lastCheck;

		#region Properties
		public IPAddress ClientIPAddress
		{
			get
			{
				return _ipAddress;
			}
		}
		
		public int Port
		{
			get
			{
				return _port;
			}
		}
		#endregion
	
		#region Methods
		/// <summary>
		/// Connects the client to a given remote address and port.
		/// </summary>
		public void Connect()
		{
            UnityEngine.Debug.Log("Connecting to "+_ipAddress+":"+_port+"...");

			if(_tcpClient != null && _tcpClient.Connected) Close();

			_tcpClient = new TcpClient();
                   // Disable the Nagle Algorithm for this tcp socket.
            _tcpClient.NoDelay = true;

            // Set the receive buffer size to 8k
            _tcpClient.ReceiveBufferSize = 8192;

            // Set the timeout for synchronous receive methods to 
            // 1 second (1000 milliseconds.)
            _tcpClient.ReceiveTimeout = 2000; 

			try
			{
				_tcpClient.Connect(_ipAddress, _port);
                isConnected = true;

                
			}
			catch(Exception e)
			{
                isConnected = false;
                Debug.LogWarning(String.Format("Can't create client at IP address {0} and port {1} : {2}", _ipAddress,_port,e.Message));
			}

		}

        public String getLocalIP()
        {
            String result = "";
            /*
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    Console.WriteLine(ni.Name);
                    foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            Console.WriteLine(ip.Address.ToString());
                            if (ip.Address.ToString().StartsWith("192.168.")) return ip.Address.ToString();
                        }
                    }
                }
            }
            */
            if (result == "") result = Network.player.ipAddress;
            return result;
        }

        public void Update()
        {
            if (_tcpClient != null)
            {

                if (isConnected != lastConnected)
                {
                    lastConnected = _tcpClient.Connected;
                    if (isConnected)
                    {
                        onConnected(this);
                        register();
                    }
                    else
                    {
                        isRegistered = false;
                        onDisconnected(this);
                    }
                }
            }
            else
            {
                isRegistered = false;
            }

            if(isRegistered != lastRegistered)
            {
                lastRegistered = isRegistered;
                if (isRegistered) onRegistered(this);
            }

            if (_tcpClient != null && isConnected)
            {
                if (Time.time > lastCheck + checkTime)
                {
                    CheckStillConnected();
                    lastCheck = Time.time;
                }
                Receive();
            }
            else
            {
                if (Time.time > lastCheck + checkTime)
                {
                    connectThread = new Thread(new ThreadStart(this.Connect));
                    connectThread.Start();
                    lastCheck = Time.time;
                }
            }

        }

        private void register()
        {
            Debug.Log("Register : " + TabletIDManager.getTabletID() + " > " + getLocalIP());
            OSCMessage m = new OSCMessage("/register");
            m.Append(TabletIDManager.getTabletID());
            m.Append(getLocalIP());
            OSCMaster.sendMessageToRouter(m);
        }

        private void CheckStillConnected()
        {
           // Debug.Log("Check Still connected");
            try
            {
                if(_tcpClient != null)
                {
                    if (_tcpClient.Client.Poll(0, SelectMode.SelectRead))
                    {
                        byte[] buff = new byte[1];
                        if (_tcpClient.Client.Receive(buff, SocketFlags.Peek) == 0)
                        {
                            // Client disconnected
                            isConnected = false;
                        }
                    }
                }
               
            }catch(Exception e)
            {
                isConnected = false;
            }
          
            if(isConnected && !isRegistered) register();
        }

        private void Receive()
        {
            if (!isConnected) return;

            try
            {
                if (_tcpClient.Available == 0) return;

                byte[] bytes = new byte[_tcpClient.Available];
                _tcpClient.Client.Receive(bytes);

                buffer.AddRange(bytes);

                unpackAll();

            }
            catch(Exception e)
            {
                throw new Exception(String.Format("Receive error : {0}\n{1}",e.Message,e.StackTrace));
            }
        }

        void unpackAll()
        {
            while(buffer.Count > 0)
            {
                byte[] bytes = buffer.ToArray();

                int start = 0;
                OSCMessage packet = OSCMessage.Unpack(bytes, ref start);
                //Debug.Log("First pack start = " + start + " / " + packet != null);

                bool trimmed = false;
                if (packet == null)
                {
                    String s = BitConverter.ToString(bytes, 0, 4);
                    //Debug.Log("Trim : " + s);
                    start = 4; // 4 bytes
                    packet = OSCMessage.Unpack(bytes, ref start);
                    trimmed = true;
                }

                buffer.RemoveRange(0, start);

                if (packet != null)
                {

                    //Debug.Log("First pack start = " + start + " / " + packet.Address + " / " + bytes.Length);
                    ConsoleUtil.log("Received " + bytes.Length + " " + (trimmed ? "trimmed" : "bytes") + ", packet address : " + packet.Address);

                    packetReceived(packet);
                }
                else
                {
                    Debug.Log("Packet null after trim test, adding to buffer");
                }

            }

        }

        /// <summary>
        /// Closes the client.
        /// </summary>
        public void Close()
		{
            if(_tcpClient != null) _tcpClient.Close();
			_tcpClient = null;
		}
		
		/// <summary>
		/// Sends an OSC packet to the defined destination and address of the client.
		/// </summary>
		/// <param name="packet">
		/// A <see cref="OSCPacket"/>
		/// </param>
		public void Send(OSCPacket packet)
		{
            Debug.Log("Sending message " + packet.BinaryData.Length);
			byte[] data = packet.BinaryData;
			try 
			{
                int sent = _tcpClient.Client.Send(data,data.Length,SocketFlags.None);
                Debug.Log("Sent : " + sent);
			}
			catch
			{
				throw new Exception(String.Format("Can't send OSC packet to client {0} : {1}", _ipAddress, _port));
			}
		}
		#endregion
	}
}

