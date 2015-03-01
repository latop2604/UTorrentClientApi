using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace UTorrent.Api.Tools
{
    /// <summary>
    /// Defines a MemoryStream that does not sit on the Large Object Heap, thus avoiding memory fragmentation.
    /// 
    /// </summary>
    internal sealed class ChunkedMemoryStream : Stream
    {
        private List<byte[]> _chunks = new List<byte[]>();
        /// <summary>
        /// Defines the default chunk size. Currently defined as 0x10000.
        /// 
        /// </summary>
        public const int DefaultChunkSize = 65536;
        private long _position;
        private int _chunkSize;
        private int _lastChunkPos;
        private int _lastChunkPosIndex;

        /// <summary>
        /// Gets or sets a value indicating whether to free the underlying chunks on dispose.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if free on dispose; otherwise, <c>false</c>.
        /// </value>
        public bool FreeOnDispose { get; set; }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// 
        /// </summary>
        /// 
        /// <value/>
        /// 
        /// <returns>
        /// true if the stream supports reading; otherwise, false.
        /// 
        /// </returns>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// 
        /// </summary>
        /// 
        /// <value/>
        /// 
        /// <returns>
        /// true if the stream supports seeking; otherwise, false.
        /// 
        /// </returns>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// 
        /// </summary>
        /// 
        /// <value/>
        /// 
        /// <returns>
        /// true if the stream supports writing; otherwise, false.
        /// 
        /// </returns>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// 
        /// </summary>
        /// 
        /// <value/>
        /// 
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// 
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override long Length
        {
            get
            {
                CheckDisposed();
                if (_chunks.Count == 0)
                    return 0L;
                return ((_chunks.Count - 1) * ChunkSize + _lastChunkPos);
            }
        }

        /// <summary>
        /// Gets or sets the size of the underlying chunks. Cannot be greater than or equal to 85000.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The chunks size.
        /// </value>
        public int ChunkSize
        {
            get
            {
                return _chunkSize;
            }
            set
            {
                if (value <= 0 || value >= 85000)
                    throw new ArgumentOutOfRangeException("value");
                _chunkSize = value;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// 
        /// </summary>
        /// 
        /// <value/>
        /// 
        /// <returns>
        /// The current position within the stream.
        /// 
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override long Position
        {
            get
            {
                CheckDisposed();
                return _position;
            }
            set
            {
                CheckDisposed();
                if (value < 0L)
                    throw new ArgumentOutOfRangeException("value");
                if (value > Length)
                    throw new ArgumentOutOfRangeException("value");
                _position = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kalten.Core.ChunkedMemoryStream"/> class.
        /// 
        /// </summary>
        public ChunkedMemoryStream()
            : this(DefaultChunkSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kalten.Core.ChunkedMemoryStream"/> class.
        /// 
        /// </summary>
        /// <param name="chunkSize">Size of the underlying chunks.</param>
        public ChunkedMemoryStream(int chunkSize)
            : this(chunkSize, null)
        {
            Contract.Requires(chunkSize > 0);
            Contract.Requires(chunkSize < 85000);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kalten.Core.ChunkedMemoryStream"/> class based on the specified byte array.
        /// 
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        public ChunkedMemoryStream(byte[] buffer)
            : this(DefaultChunkSize, buffer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Kalten.Core.ChunkedMemoryStream"/> class based on the specified byte array.
        /// 
        /// </summary>
        /// <param name="chunkSize">Size of the underlying chunks.</param><param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        public ChunkedMemoryStream(int chunkSize, byte[] buffer)
        {
            Contract.Requires(chunkSize > 0);
            Contract.Requires(chunkSize < 85000);

            FreeOnDispose = true;
            ChunkSize = chunkSize;
            _chunks.Add(new byte[chunkSize]);
            if (buffer == null)
                return;
            Write(buffer, 0, buffer.Length);
            Position = 0L;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
        /// 
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (FreeOnDispose && _chunks != null)
            {
                _chunks = null;
                _chunkSize = 0;
                _position = 0L;
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        ///             This implementation does nothing.
        /// 
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// 
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read from the current source.</param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current stream.</param><param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// 
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
        ///             </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.
        ///             </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.
        ///             </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException(null, "count");
            CheckDisposed();
            int index = (int)(_position / ChunkSize);
            if (index == _chunks.Count)
                return 0;
            int srcOffset = (int)(_position % ChunkSize);
            count = (int)Math.Min(count, Length - _position);
            if (count == 0)
                return 0;
            int val1 = count;
            int dstOffset = offset;
            int num = 0;
            do
            {
                int count1 = Math.Min(val1, ChunkSize - srcOffset);
                Buffer.BlockCopy(_chunks[index], srcOffset, buffer, dstOffset, count1);
                dstOffset += count1;
                val1 -= count1;
                num += count1;
                if (srcOffset + count1 == ChunkSize)
                {
                    if (index != _chunks.Count - 1)
                    {
                        srcOffset = 0;
                        ++index;
                    }
                    else
                        break;
                }
                else
                    srcOffset += count1;
            }
            while (val1 > 0);
            _position += num;
            return num;
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// The unsigned byte cast to an Int32, or -1 if at the end of the stream.
        /// 
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override int ReadByte()
        {
            CheckDisposed();
            if (_position >= Length)
                return -1;
            byte num = _chunks[(int)(_position / ChunkSize)][_position % ChunkSize];
            ++_position;
            return num;
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// 
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param><param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// 
        /// </returns>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        private void CheckDisposed()
        {
            if (_chunks == null)
                throw new ObjectDisposedException(null, "Cannot access a disposed stream");
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// 
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override void SetLength(long value)
        {
            CheckDisposed();
            if (value < 0L)
                throw new ArgumentOutOfRangeException("value");
            if (value > Length)
                throw new ArgumentOutOfRangeException("value");
            long num1 = value / ChunkSize;
            if (value % this.ChunkSize != 0L)
                ++num1;
            if (num1 > int.MaxValue)
                throw new ArgumentOutOfRangeException("value");
            if (num1 < this._chunks.Count)
            {
                int num2 = (int)(_chunks.Count - num1);
                for (int index = 0; index < num2; ++index)
                    _chunks.RemoveAt(_chunks.Count - 1);
            }
            _lastChunkPos = (int)(value % ChunkSize);
        }

        /// <summary>
        /// Converts the current stream to a byte array.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// An array of bytes
        /// </returns>
        public byte[] ToArray()
        {
            CheckDisposed();
            byte[] numArray = new byte[Length];
            int dstOffset = 0;
            for (int index = 0; index < _chunks.Count; ++index)
            {
                int count = index == _chunks.Count - 1 ? _lastChunkPos : _chunks[index].Length;
                if (count > 0)
                {
                    Buffer.BlockCopy(_chunks[index], 0, numArray, dstOffset, count);
                    dstOffset += count;
                }
            }
            return numArray;
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// 
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/> bytes from <paramref name="buffer"/> to the current stream.</param><param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin copying bytes to the current stream.</param><param name="count">The number of bytes to be written to the current stream.</param><exception cref="T:System.ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.
        ///             </exception><exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.
        ///             </exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.
        ///             </exception><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (offset < 0)
                throw new ArgumentOutOfRangeException("offset");
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            if (buffer.Length - offset < count)
                throw new ArgumentException(null, "count");
            CheckDisposed();
            int dstOffset = (int)(_position % ChunkSize);
            int index = (int)(_position / ChunkSize);
            if (index == _chunks.Count)
                _chunks.Add(new byte[ChunkSize]);
            int val1 = count;
            int srcOffset = offset;
            do
            {
                int count1 = Math.Min(val1, ChunkSize - dstOffset);
                Buffer.BlockCopy(buffer, srcOffset, _chunks[index], dstOffset, count1);
                srcOffset += count1;
                val1 -= count1;
                if (dstOffset + count1 == ChunkSize)
                {
                    ++index;
                    dstOffset = 0;
                    if (index == _chunks.Count)
                        _chunks.Add(new byte[ChunkSize]);
                }
                else
                    dstOffset += count1;
            }
            while (val1 > 0);
            _position += count;
            if (index != _chunks.Count - 1 || index <= _lastChunkPosIndex && (index != _lastChunkPosIndex || dstOffset <= _lastChunkPos))
                return;
            _lastChunkPos = dstOffset;
            _lastChunkPosIndex = index;
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// 
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param><exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.
        ///             </exception>
        public override void WriteByte(byte value)
        {
            CheckDisposed();
            int index1 = (int)(_position / ChunkSize);
            int num1 = (int)(_position % ChunkSize);
            if (num1 > ChunkSize - 1)
            {
                ++index1;
                num1 = 0;
                if (index1 == _chunks.Count)
                    _chunks.Add(new byte[ChunkSize]);
            }
            byte[] numArray = _chunks[index1];
            int index2 = num1;
            int num2 = 1;
            int num3 = index2 + num2;
            int num4 = value;
            numArray[index2] = (byte)num4;
            ++_position;
            if (index1 != _chunks.Count - 1 || index1 <= _lastChunkPosIndex && (index1 != _lastChunkPosIndex || num3 <= _lastChunkPos))
                return;
            _lastChunkPos = num3;
            _lastChunkPosIndex = index1;
        }

        /// <summary>
        /// Writes to the specified stream.
        /// 
        /// </summary>
        /// <param name="stream">The stream.</param>
        public void WriteTo(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            CheckDisposed();
            for (int index = 0; index < _chunks.Count; ++index)
            {
                int count = index == _chunks.Count - 1 ? _lastChunkPos : _chunks[index].Length;
                stream.Write(_chunks[index], 0, count);
            }
        }
    }
}
