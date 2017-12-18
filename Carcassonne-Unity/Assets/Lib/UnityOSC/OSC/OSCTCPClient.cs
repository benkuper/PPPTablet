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

namespace UnityOSC
{
	/// <summary>
	/// Dispatches OSC messages to the specified destination address and port.
	/// </summary>
	
	public class OSCTCPClient
	{

        public delegate void TCPClientConnectedEvent(OSCTCPClient client);
        public TCPClientConnectedEvent onConnected;
        public delegate void TCPClientDisconnectedEvent(OSCTCPClient client);
        public TCPClientDisconnectedEvent onDisconnected;
        public delegate void TCPPacketReceivedEvent(OSCPacket packet);
        public TCPPacketReceivedEvent packetReceived;

        #region Constructors
        public OSCTCPClient (IPAddress address, int port)
		{
			_ipAddress = address;
			_port = port;
		}
		#endregion
		
		#region Member Variables
		private IPAddress _ipAddress;
		private int _port;
		private TcpClient _tcpClient;
        #endregion

        bool isConnected;
        bool lastConnected;

        Thread connectThread;

        float checkTime = 2;
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
            _tcpClient.ReceiveTimeout = 1000; 
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

        public void Update()
        {
            if (_tcpClient != null)
            {

                if (isConnected != lastConnected)
                {
                    lastConnected = _tcpClient.Connected;
                    if (isConnected) onConnected(this);
                    else onDisconnected(this);
                }

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

        private void CheckStillConnected()
        {
            try
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
            }catch(Exception e)
            {
                isConnected = false;
            }
          
        }

        private void Receive()
        {
            if (!isConnected) return;

            try
            {
                if (_tcpClient.Available == 0) return;

                byte[] bytes = new byte[_tcpClient.Available];
                _tcpClient.Client.Receive(bytes);


                bool trimmed = false;
                if(bytes[0] == 0 && bytes[1] == 0 && bytes[2] == 0)
                {
                    List<byte> trimBytes = new List<byte>(bytes);
                    trimBytes.RemoveRange(0, 4);
                    bytes = trimBytes.ToArray();
                    trimmed = true;

                }
                OSCPacket packet = OSCPacket.Unpack(bytes);

                ConsoleUtil.log("Received "+bytes.Length+" "+(trimmed?"trimmed":"bytes")+", packet address : "+packet.Address);

                packetReceived(packet);
            }
            catch(Exception e)
            {
                throw new Exception(String.Format("Receive error : {0}",e.Message));
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

