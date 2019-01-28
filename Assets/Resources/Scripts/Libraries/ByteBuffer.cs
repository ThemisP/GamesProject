using System;
using System.Collections.Generic;
using System.Text;

namespace ByteBuffer
{
    public class ByteBuffer : IDisposable
    {
        List<byte> Buff;
        byte[] readBuff;
        int readpos;
        bool buffUpdate = false;

        public ByteBuffer() {
            Buff = new List<byte>();
            readpos = 0;
        }
        public int GetReadPos() {
            return readpos;
        }
        public byte[] BuffToArray() {
            return Buff.ToArray();
        }
        public int Count() {
            return Buff.Count;
        }
        public int Length() {
            return Count() - readpos;
        }
        public void Clear() {
            Buff.Clear();
            readpos = 0;
        }

        #region "Write Data"
        public void WriteByte(byte Input) {
            Buff.Add(Input);
            buffUpdate = true;
        }
        
        public void WriteBytes(byte[] Input) {
            Buff.AddRange(Input);
            buffUpdate = true;
        }

        public void WriteShort(short Input) {
            Buff.AddRange(BitConverter.GetBytes(Input));
            buffUpdate = true;
        }

        public void WriteInt(int Input) {
            Buff.AddRange(BitConverter.GetBytes(Input));
            buffUpdate = true;
        }

        public void WriteFloat(float Input) {
            Buff.AddRange(BitConverter.GetBytes(Input));
            buffUpdate = true;
        }

        public void WriteString(string Input) {
            Buff.AddRange(BitConverter.GetBytes(Input.Length));
            Buff.AddRange(Encoding.ASCII.GetBytes(Input));
            buffUpdate = true;
        }
        #endregion

        #region "Read Data"
        public byte ReadByte(bool Peek = true) {
            if (Buff.Count > readpos) {
                if (buffUpdate) {
                    readBuff = Buff.ToArray();
                    buffUpdate = false;
                }
                byte ret = readBuff[readpos];
                if (Peek && Buff.Count > readpos) {
                    readpos += 1;
                }
                return ret;
            } else {
                throw new Exception("Byte buffer is past its limit");
            }
        }
        public byte[] ReadBytes(int Length, bool Peek = true) {            
            if (buffUpdate) {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }
            byte[] ret = Buff.GetRange(readpos, Length).ToArray();
            if (Peek) {
                readpos += Length;
            }
            return ret;           
        }
        public float ReadFloat(bool Peek = true) {
            if (Buff.Count > readpos) {
                if (buffUpdate) {
                    readBuff = Buff.ToArray();
                    buffUpdate = false;
                }
                float ret = BitConverter.ToSingle(readBuff, readpos);
                if (Peek && Buff.Count > readpos) {
                    readpos += 4;
                }
                return ret;
            } else {
                throw new Exception("Byte buffer is past its limit");
            }
        }
        public int ReadInt(bool Peek = true) {
            if(Buff.Count > readpos) {
                if (buffUpdate) {
                    readBuff = Buff.ToArray();
                    buffUpdate = false;
                }
                int ret = BitConverter.ToInt32(readBuff, readpos);
                if(Peek && Buff.Count > readpos) {
                    readpos += 4;
                }
                return ret;
            } else {
                throw new Exception("Byte buffer is past its limit");
            }
        }
        public string ReadString(bool Peek = true) {
            int length = ReadInt();
            if (buffUpdate) {
                readBuff = Buff.ToArray();
                buffUpdate = false;
            }
            string ret = Encoding.ASCII.GetString(readBuff, readpos, length);
            if(Peek && Buff.Count > readpos) {
                if (ret.Length>0) {
                    readpos += length;
                }
            }

            return ret;
        }
        #endregion

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!this.disposedValue) {
                if (disposing) {
                    Buff.Clear();
                    
                }
                readpos = 0;
            }
            this.disposedValue = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
