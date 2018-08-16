using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CondenserDotNet.StatsD
{
    public abstract class StatsDClient
    {
        protected readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);
        protected readonly byte[] _buffer;
        //protected readonly Memory<byte> _remainingBuffer;
        protected readonly UdpClient _udpClient;
        
        private const byte _seperator = 0x3A; // :

        public StatsDClient(int bufferSize) => _buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(bufferSize);//_remainingBuffer = _buffer;

        public void SendMetric(MetricEntry metricEntry)
        {
            //var initialLength = _remainingBuffer.Length;
           // var length = Encoding.UTF8.GetByteCount(metricEntry.MetricName) + 1 + ;
            //var span = _remainingBuffer.Span;

            //if(span.Length <= length)
            //{
            //    //Need to send the current buffer and retry
            //    _udpClient.Send()
            //}


            //UTF8Encoding.UTF8.GetBytes(metricEntry.MetricName, )

            //_waitingEntries.Enqueue(metricEntry);
            //_semaphoreSlim.Release(1);
        }
    }
}
