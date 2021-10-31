using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roman015API.Utils
{
    public class DummyTextFileStream : Stream
    {
        private readonly char[] loreumIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.\nCurabitur pretium tincidunt lacus. Nulla gravida orci a odio. Nullam varius, turpis et commodo pharetra, est eros bibendum elit, nec luctus magna felis sollicitudin mauris. Integer in mauris eu nibh euismod gravida. Duis ac tellus et risus vulputate vehicula. Donec lobortis risus a elit. Etiam tempor. Ut ullamcorper, ligula eu tempor congue, eros est euismod turpis, id tincidunt sapien risus a quam. Maecenas fermentum consequat mi. Donec fermentum. Pellentesque malesuada nulla a mi. Duis sapien sem, aliquet nec, commodo eget, consequat quis, neque. Aliquam faucibus, elit ut dictum aliquet, felis nisl adipiscing sapien, sed malesuada diam lacus eget erat. Cras mollis scelerisque nunc. Nullam arcu. Aliquam consequat. Curabitur augue lorem, dapibus quis, laoreet et, pretium ac, nisi. Aenean magna nisl, mollis quis, molestie eu, feugiat in, orci. In hac habitasse platea dictumst.".ToCharArray();
        private readonly byte[] loreumIpsumBytes;
        private long _Length = 0;
        private long _Position = 0;

        public DummyTextFileStream(long length)
        {
            _Length = length;
            loreumIpsumBytes = loreumIpsumBytes ?? Encoding.ASCII.GetBytes(loreumIpsum);
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => _Length;

        public override long Position { get => _Position; set => _Position = value; }

        public override void Flush()
        {
            // No Actual Flushing Needed
            return;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return FastRead(buffer, offset, count);
        }

        private int FastRead(byte[] buffer, int offset, int count)
        {
            // This Function Gives Download Speeds of ~70MB/s on Developper Machine
            int copySize;
            int charsRead = 0;
            int loreumIpsumIndex = (int)(_Position % loreumIpsumBytes.Length);

            while (_Position < _Length && offset < buffer.Length && count > 0)
            {
                copySize = count;

                if (copySize + loreumIpsumIndex > loreumIpsumBytes.Length)
                {
                    copySize = loreumIpsumBytes.Length - loreumIpsumIndex;
                }

                if (copySize + offset > buffer.Length)
                {
                    copySize = buffer.Length - offset;
                }

                if (copySize + _Position > _Length)
                {
                    copySize = (int)(_Length - _Position);
                }

                Buffer.BlockCopy(loreumIpsumBytes, loreumIpsumIndex, buffer, offset, copySize);

                loreumIpsumIndex = (loreumIpsumIndex + copySize) % loreumIpsumBytes.Length;
                _Position += copySize;
                charsRead += copySize;
                count -= copySize;
                offset += copySize;
            }

            return charsRead;
        }

        private int SimpleRead(byte[] buffer, int offset, int count)
        {
            // This Function Gives Download Speeds of ~35MB/s on Developper Machine
            int charsRead = 0;
            int loreumIpsumIndex = (int)(_Position % loreumIpsum.Length);

            while (_Position < _Length && offset + charsRead < buffer.Length)
            {
                buffer[offset + charsRead] = (byte)(loreumIpsum[loreumIpsumIndex]);
                loreumIpsumIndex = (loreumIpsumIndex + 1) % loreumIpsum.Length;
                _Position++;
                charsRead++;
            }

            return charsRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _Position = offset;
                    break;

                case SeekOrigin.Current:
                    _Position += offset;
                    break;

                case SeekOrigin.End:
                    _Position = _Length - offset - 1;
                    break;

                default:
                    throw new System.NotImplementedException();
            }

            return _Position;
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
    }
}
