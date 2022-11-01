using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding;

namespace FileDrop.Helpers.WiFiDirect
{
    public class SocketReaderWriter : IDisposable
    {
        DataReader _dataReader;
        DataWriter _dataWriter;
        StreamSocket _streamSocket;

        public SocketReaderWriter(StreamSocket socket)
        {
            _dataReader = new DataReader(socket.InputStream);
            _dataReader.UnicodeEncoding = UnicodeEncoding.Utf8;
            _dataReader.ByteOrder = ByteOrder.LittleEndian;

            _dataWriter = new DataWriter(socket.OutputStream);
            _dataWriter.UnicodeEncoding = UnicodeEncoding.Utf8;
            _dataWriter.ByteOrder = ByteOrder.LittleEndian;

            _streamSocket = socket;
        }

        public void Dispose()
        {
            _dataReader.Dispose();
            _dataWriter.Dispose();
            _streamSocket.Dispose();
        }

        public async Task WriteAsync(object obj, IBuffer payload = null)
        {
            try
            {
                var info = JsonConvert.SerializeObject(obj);
                _dataWriter.WriteUInt32(_dataWriter.MeasureString(info));
                if (payload == null) _dataWriter.WriteUInt32(0);
                else _dataWriter.WriteUInt32(payload.Length);
                _dataWriter.WriteString(info);
                if (payload != null)
                    _dataWriter.WriteBuffer(payload);
                await _dataWriter.StoreAsync();
            }
            catch (Exception)
            { }
        }

        public async Task WriteAsync(string info, IBuffer payload = null)
        {
            try
            {
                _dataWriter.WriteUInt32(_dataWriter.MeasureString(info));
                if (payload == null) _dataWriter.WriteUInt32(0);
                else _dataWriter.WriteUInt32(payload.Length);
                _dataWriter.WriteString(info);
                if (payload != null)
                    _dataWriter.WriteBuffer(payload);
                await _dataWriter.StoreAsync();
            }
            catch (Exception)
            { }
        }

        public async Task<SocketRead> ReadAsync()
        {
            try
            {
                uint bytesRead = await _dataReader.LoadAsync(sizeof(uint));
                if (bytesRead > 0)
                {
                    uint infoLength = _dataReader.ReadUInt32();
                    bytesRead = await _dataReader.LoadAsync(sizeof(uint));
                    if (bytesRead > 0)
                    {
                        uint payloadLength = _dataReader.ReadUInt32();
                        bytesRead = await _dataReader.LoadAsync(infoLength);
                        if (bytesRead > 0)
                        {
                            string info = _dataReader.ReadString(infoLength);
                            if (payloadLength == 0)
                            {
                                return new SocketRead()
                                {
                                    payload = null,
                                    info = info
                                };
                            }
                            else
                            {
                                bytesRead = await _dataReader.LoadAsync(payloadLength);
                                if (bytesRead > 0)
                                {
                                    var payload = _dataReader.ReadBuffer(payloadLength);
                                    return new SocketRead()
                                    {
                                        payload = payload,
                                        info = info
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            { }
            return null;
        }

        public async void StartRead(Action<SocketRead> action)
        {
            SocketRead read;
            do
            {
                read = await ReadAsync();
                action(read);
            }
            while (read != null);
        }

        public class SocketRead
        {
            public string info;
            public IBuffer payload;
        }
    }
}
