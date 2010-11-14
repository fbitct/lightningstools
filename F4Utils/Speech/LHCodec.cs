using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
namespace F4Utils.Speech
{
    public class LHCodec : IDisposable, F4Utils.Speech.IAudioCodec
    {
        private IntPtr _hCoder = IntPtr.Zero;
        private IntPtr _hDecoder = IntPtr.Zero;
        private bool _disposed = false;
        private short PMSIZE = 0;
        private short CODESIZE = 0;

        public LHCodec()
        {
            Initialize();
        }

        private void Initialize()
        {
            StreamTalk80.CODECINFO CodecInfoStruct = new StreamTalk80.CODECINFO();
            StreamTalk80.CODECINFOEX CodecInfoExStruct = new StreamTalk80.CODECINFOEX();
            StreamTalk80.GetCodecInfo(ref CodecInfoStruct);
            StreamTalk80.GetCodecInfoEx(ref CodecInfoExStruct, Marshal.SizeOf(typeof(StreamTalk80.CODECINFOEX)));

            PMSIZE = CodecInfoExStruct.wInputBufferSize;
            CODESIZE = CodecInfoExStruct.wCodedBufferSize;
            if (PMSIZE == 0)
                PMSIZE = 4096;
            if (CODESIZE == 0)
                CODESIZE = 4096;
            if ((_hDecoder = StreamTalk80.OpenDecoder(StreamTalk80.OPENCODERFLAGS.LINEAR_PCM_16_BIT)) == IntPtr.Zero)
                throw new StreamTalk80Exception("Could not open decoder");
            if ((_hCoder = StreamTalk80.OpenCoder(StreamTalk80.OPENCODERFLAGS.LINEAR_PCM_16_BIT)) == IntPtr.Zero)
                throw new StreamTalk80Exception("Could not open encoder");
        }
        private void VerifyST80WDll()
        {
            string appDirectory=new FileInfo(Process.GetCurrentProcess().MainModule.FileName).DirectoryName;
            string[] filesFound = Directory.GetFiles(appDirectory, "ST80W.dll", SearchOption.TopDirectoryOnly);
            if (filesFound ==null || filesFound.Length ==0) 
            {
                throw new Exception(
                    string.Format(
                        "Could not locate ST80W.DLL, please verify that it exists " + 
                        "in the application directory ({0}) or copy it there from " + 
                        "your F4 installation folder manually.", appDirectory));
            }
        }
        ~LHCodec()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    //dispose of managed resources here
                }
                //dispose of unmanaged resources here
                CloseCodecStreams();
                _disposed = true;
            }
        }

        public int Decode(byte[] inputBuffer, int inputBufferOffset, int dataLength, ref byte[] outputBuffer, int outputBufferOffset)
        {
            StreamTalk80.LH_ERRCODE errorCode;
            short outputCodedSize;
            int loopCount, compDecodeSize = 0;
            short decodeSize;
            loopCount = dataLength;

            outputCodedSize = PMSIZE;

            GCHandle pinnedInputBufferHandle = GCHandle.Alloc(inputBuffer, GCHandleType.Pinned);
            IntPtr inputPtr = Marshal.UnsafeAddrOfPinnedArrayElement(inputBuffer, inputBufferOffset);
            GCHandle pinnedOutputBufferHandle = GCHandle.Alloc(outputBuffer, GCHandleType.Pinned);
            IntPtr outputPtr = Marshal.UnsafeAddrOfPinnedArrayElement(outputBuffer, outputBufferOffset);

            while (loopCount > 0)
            {
                decodeSize = (short)loopCount;
                if (decodeSize > CODESIZE)
                    decodeSize = CODESIZE;

                errorCode = StreamTalk80.Decode(
                    _hDecoder,
                    inputPtr,
                    ref decodeSize,
                    outputPtr,
                    ref outputCodedSize
                    );

                if (errorCode == StreamTalk80.LH_ERRCODE.LH_EBADARG)
                {
                    throw new StreamTalk80Exception("Bad argument.");
                }
                else if (errorCode == StreamTalk80.LH_ERRCODE.LH_BADHANDLE)
                {
                    throw new StreamTalk80Exception("Bad handle.");
                }
                else if (errorCode == StreamTalk80.LH_ERRCODE.LH_EFAILURE)
                {
                    throw new StreamTalk80Exception("Decompress failed.");
                }
                inputPtr = new IntPtr(inputPtr.ToInt64() + decodeSize);
                outputPtr = new IntPtr(outputPtr.ToInt64() + outputCodedSize);
                loopCount -= decodeSize;
                compDecodeSize += outputCodedSize;
            }
            pinnedInputBufferHandle.Free();
            pinnedOutputBufferHandle.Free();
            return compDecodeSize;
        }
        public int Encode(byte[] inputBuffer, int inputBufferOffset, int dataLength, byte[] outputBuffer, int outputBufferOffset)
        {
            StreamTalk80.LH_ERRCODE errorCode;
            short outputCodedSize;
            int loopCount, compDecodeSize = 0;
            short decodeSize;
            loopCount = dataLength;

            outputCodedSize = CODESIZE;

            GCHandle pinnedInputBufferHandle = GCHandle.Alloc(inputBuffer, GCHandleType.Pinned);
            IntPtr inputPtr = Marshal.UnsafeAddrOfPinnedArrayElement(inputBuffer, inputBufferOffset);
            GCHandle pinnedOutputBufferHandle = GCHandle.Alloc(outputBuffer, GCHandleType.Pinned);
            IntPtr outputPtr = Marshal.UnsafeAddrOfPinnedArrayElement(outputBuffer, outputBufferOffset);

            while (loopCount > 0)
            {
                decodeSize = (short)loopCount;
                if (decodeSize > PMSIZE)
                    decodeSize = PMSIZE;
                else if (decodeSize < PMSIZE)
                    break;

                errorCode = StreamTalk80.Encode(
                    _hCoder,
                    inputPtr,
                    ref decodeSize,
                    outputPtr,
                    ref outputCodedSize
                    );

                if (errorCode == StreamTalk80.LH_ERRCODE.LH_EBADARG)
                {
                    throw new StreamTalk80Exception("Bad argument.");
                }
                else if (errorCode == StreamTalk80.LH_ERRCODE.LH_BADHANDLE)
                {
                    throw new StreamTalk80Exception("Bad handle.");
                }
                else if (errorCode == StreamTalk80.LH_ERRCODE.LH_EFAILURE)
                {
                    throw new StreamTalk80Exception("Compress failed.");
                }

                inputPtr = new IntPtr(inputPtr.ToInt64() + decodeSize);
                outputPtr = new IntPtr(outputPtr.ToInt64() + outputCodedSize);
                loopCount -= decodeSize;
                compDecodeSize += outputCodedSize;
            }

            pinnedInputBufferHandle.Free();
            pinnedOutputBufferHandle.Free();
            return compDecodeSize;
        }
        public int Encode(Stream inputStream, int dataLength, Stream outputStream)
        {
            byte[] inputBytes = new byte[dataLength];
            int bytesRead=inputStream.Read(inputBytes, 0, dataLength);
            if (bytesRead < dataLength) throw new IOException("Unexpected end of stream encountered.");
            byte[] outputBytes = new byte[dataLength * 2];
            int encodedLength = Encode(inputBytes, 0, dataLength, outputBytes, 0);
            outputStream.Write(outputBytes, 0, encodedLength);
            outputStream.Flush();
            return encodedLength;
        }

        public int Decode(Stream inputStream, int dataLength, int uncompressedLength, Stream outputStream)
        {
            byte[] inputBytes = new byte[dataLength];
            int bytesRead=inputStream.Read(inputBytes, 0, dataLength);
            if (bytesRead < dataLength) throw new IOException("Unexpected end of stream encountered.");
            byte[] outputBytes = new byte[uncompressedLength*2];
            int decodedLength = Decode(inputBytes, 0, dataLength, ref outputBytes, 0);
            outputStream.Write(outputBytes, 0, decodedLength);
            if (decodedLength < uncompressedLength)
            {
                for (int i = 0; i < uncompressedLength - decodedLength; i++)
                {
                    outputStream.WriteByte(0); //fill rest of stream with NULLs
                }
            }
            outputStream.Flush();
            return decodedLength;
        }
        private void CloseCodecStreams()
        {
            StreamTalk80.CloseCoder(_hCoder);
            StreamTalk80.CloseDecoder(_hDecoder);
        }
    }
}
