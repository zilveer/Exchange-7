using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Engine
{
    class TradeLogger : IDisposable
    {
        private Channel<byte[]> _inboundChannel;
        private ChannelWriter<byte[]> _inboundChannelWritter;
        private ChannelReader<byte[]> _inboundChannelReader;
        private readonly FileStream _filestream;
        readonly Task _logReader;
        public TradeLogger(string filePath)
        {
            _inboundChannel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(100000) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true, SingleWriter = true, AllowSynchronousContinuations = false });
            _inboundChannelReader = _inboundChannel.Reader;
            _inboundChannelWritter = _inboundChannel.Writer;

            _filestream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _logReader = new Task(async () => await LogReader());
            _logReader.Start();
        }

        public void Dispose()
        {
            _logReader.Wait();
            _filestream.Dispose();
        }

        public void Write(byte[] data)
        {
            _inboundChannelWritter.TryWrite(data);
        }

        public void Complete()
        {
            _inboundChannelWritter.Complete();
            _logReader.Wait();
            _filestream.Dispose();
        }

        private async Task LogReader()
        {
            try
            {
                while (await _inboundChannelReader.WaitToReadAsync())
                {
                    while (_inboundChannelReader.TryRead(out var bytes))
                    {
                        _filestream.Write(bytes, 0, bytes.Length);
                    }
                }
                _filestream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //TODO
                throw;
            }
        }
    }
}
