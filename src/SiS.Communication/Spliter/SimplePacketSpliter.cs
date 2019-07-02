﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;

namespace SiS.Communication.Spliter
{
    /// <summary>
    /// Represents an object that implements interface of IPacketSpliter, 
    /// which provides methods to split data into packets using length at the begining of the packet
    /// | Message Length(4bytes) | Message |
    /// </summary>
    public class SimplePacketSpliter : IPacketSpliter
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SiS.Communication.Spliter.SimplePacketSpliter
        /// </summary>
        /// <param name="useNetworkByteOrder">true to pack length in network byte order; otherwise, int host byte order.</param>
        public SimplePacketSpliter(bool useNetworkByteOrder)
        {
            UseNetworkByteOrder = useNetworkByteOrder;
        }

        /// <summary>
        /// Initializes a new instance of the SiS.Communication.Spliter.SimplePacketSpliter
        /// </summary>
        public SimplePacketSpliter() : this(false) { }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to use network byte order
        /// </summary>
        /// <returns>true to pack length in network byte order; otherwise, in host byte order.The default is false.</returns>
        public bool UseNetworkByteOrder { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to use MakePacket method. 
        /// </summary>
        /// <returns>true if use MakePacket method; otherwise, false.The default is true.</returns>
        public bool UseMakePacket { get; set; } = true;

        /// <summary>
        /// Gets or sets the max packet length.
        /// </summary>
        /// <returns>The max packet length. The default is 20MB.</returns>
        public int MaxPacketLength { get; set; } = 20 * 1024 * 1024;

        #endregion

        #region Public Functions

        /// <summary>
        /// Convert a message to a packet using packet length
        /// </summary>
        /// <param name="messageData">The message data to convert.</param>
        /// <param name="offset">The offset of the message data.</param>
        /// <param name="count">The count of bytes to convert.</param>
        /// <returns>The converted packet with length if UseMakePacket property is true; otherwise the input message data with doing nothing.</returns>
        public byte[] MakePacket(byte[] messageData, int offset, int count)
        {
            Contract.Requires(messageData != null && count > 0);
            if (!UseMakePacket)
            {
                if (offset == 0 && count == messageData.Length)
                {
                    return messageData;
                }
                return new ArraySegment<byte>(messageData, offset, count).ToArray();
            }
            int dataLen = count;
            int packetLen = dataLen;
            if (UseNetworkByteOrder)
            {
                packetLen = IPAddress.HostToNetworkOrder(dataLen);
            }
            byte[] resData = new byte[4 + dataLen];
            Array.Copy(BitConverter.GetBytes(packetLen), 0, resData, 0, 4);
            Array.Copy(messageData, offset, resData, 4, dataLen);
            return resData;
        }

        /// <summary>
        /// Get packets from the buffer using packet length
        /// </summary>
        /// <param name="streamBuffer">The source buffer to create packets.</param>
        /// <param name="offset">The starting offset of the buffer to create packets.</param>
        /// <param name="count">The count of the data to create packets.</param>
        /// <param name="endPos">When this method returns, contains the position of the packet ending, if the buffer has 
        /// one complete packet at least, or null if the packet is not complete.</param>
        /// <returns>The packets list if has complete packet; otherwise, null.</returns>
        public List<ArraySegment<byte>> GetPackets(byte[] streamBuffer, int offset, int count, out int endPos)
        {
            int pos = offset;
            List<ArraySegment<byte>> packetList = new List<ArraySegment<byte>>();
            while (true)
            {
                int finishedCount = pos - offset;
                int remainCount = count - finishedCount;
                if (remainCount <= 4)
                {
                    break;
                }
                int contentLen = BitConverter.ToInt32(streamBuffer, pos);
                if (UseNetworkByteOrder)
                {
                    contentLen = IPAddress.NetworkToHostOrder(contentLen);
                }
                if (contentLen <= 0 || contentLen > MaxPacketLength)
                {
                    throw new Exception($"the packet size exceeds the max length:{MaxPacketLength}");
                }
                int requiredLen = 4 + contentLen;
                if (remainCount >= requiredLen)
                {
                    pos += 4;
                    ArraySegment<byte> packet = new ArraySegment<byte>(streamBuffer, pos, contentLen);
                    packetList.Add(packet);
                    pos += contentLen;
                }
                else
                {
                    break;
                }
            }
            endPos = pos;
            if (packetList.Count == 0)
            {
                return null;
            }
            return packetList;
        }

        #endregion
    }
}
