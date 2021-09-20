using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TCPLibrary.Abstracts
{
    /// <summary>
    /// 接收数据时用到的数据缓冲区
    /// </summary>
    public abstract class ZDataBuffer
    {
        /// <summary>
        /// 存放数据区
        /// </summary>
        protected byte[] _buffer;
        /// <summary>
        /// 当前数据长度
        /// </summary>
        protected int _length;
        /// <summary>
        /// 当前缓冲区容量
        /// </summary>
        protected int _capacity;

        /// <summary>
        /// 剩余数据
        /// </summary>
        public byte[] UnCompelete
        {
            get
            {
                byte[] b = new byte[_length];
                Buffer.BlockCopy(_buffer, 0, b, 0, _length);
                return b;
            }
        }
        /// <summary>
        /// 移除缓冲区中前面length个字节。派生类中在TryReadMessage方法末尾必须调用该方法，移除已经处理过的数据
        /// </summary>
        /// <param name="length"></param>
        protected void Remove(int length)
        {
            if (_length <= length)
            {
                _length = 0;
            }
            else
            {
                byte[] buffer = new byte[_buffer.Length];
                Buffer.BlockCopy(_buffer, length, buffer, 0, _length - length);
                _length = _length - length;
                _buffer = buffer;
            }
        }
        /// <summary>
        /// 确保容量足够大
        /// </summary>
        /// <param name="count"></param>
        private void EnsureCapacity(int count)
        {
            if (count <= _capacity)  //如果容量够  直接返回
            {
                return;
            }
            if (count < 2 * _capacity)  //如果扩大2倍够用 则扩大2倍
            {
                count = 2 * _capacity;
            }

            byte[] buffer = new byte[count];
            _capacity = count;
            Buffer.BlockCopy(_buffer, 0, buffer, 0, _length);   //复制原数据
            _buffer = buffer;
        }
        /// <summary>
        /// 向缓冲区中写入字节
        /// </summary>
        /// <param name="source">原数据存放区</param>
        /// <param name="offset">原数据偏移</param>
        /// <param name="count">写入字节数</param>
        public void WriteBytes(byte[] source, int offset, int count)
        {
            if (_buffer == null)
            {
                _buffer = new byte[count];
                _length = 0;
                _capacity = count;
            }
            EnsureCapacity(_length + count);  //保证容量
            Buffer.BlockCopy(source, offset, _buffer, _length, count);  //将数据写入数据存储区
            _length += count;
        }
        /// <summary>
        /// 尝试从缓冲区中读取一条完整的消息，派生类必须重写该方法。
        /// 该方法必须遵守“应用层协议”去解析接收到的数据
        /// </summary>
        /// <returns></returns>
        internal abstract ZMessage TryReadMessage();
    }
}
