using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace tabtoy
{
    public interface ITableSerializable
    {
        void Deserialize(TableReader reader);
    }

    public class TableReader
    {
        private readonly BinaryReader _binaryReader;
        private readonly long _boundPos;
        public bool ConvertNewLine { get; set; }

        public TableReader(Stream stream)
        {
            _binaryReader = new BinaryReader(stream);
            _boundPos = stream.Length;
        }

        private TableReader(TableReader reader, long boundPos)
        {
            ConvertNewLine = reader.ConvertNewLine;
            _binaryReader = reader._binaryReader;
            _boundPos = boundPos;
        }

        private bool IsDataEnough(uint size)
        {
            return _binaryReader.BaseStream.Position + size <= _boundPos;
        }

        private void ValidateDataBound(uint size)
        {
            if (!IsDataEnough(size))
            {
                throw new Exception("Out of struct bound");
            }
        }

        private void ConsumeData(uint size)
        {
            ValidateDataBound(size);
            _binaryReader.BaseStream.Seek(size, SeekOrigin.Current);
        }

        public void ReadHeader()
        {
            string header = string.Empty;
            ReadString(ref header);
            if (header != "TABTOY")
            {
                throw new Exception("Invalid tabtoy file");
            }

            uint version = 0;
            ReadUInt32(ref version);
            if (version != 4)
            {
                throw new Exception("Invalid tabtoy version");
            }
        }

        public bool ReadTag(ref uint value)
        {
            if (!IsDataEnough(sizeof(uint)))
            {
                return false;
            }

            value = _binaryReader.ReadUInt32();
            return true;
        }

        public void SkipFiled(uint tag)
        {
            switch (tag >> 16)
            {
                case 1:
                    short int16Value = 0;
                    ReadInt16(ref int16Value);
                    break;
                case 2:
                case 10:
                    int int32Value = 0;
                    ReadInt32(ref int32Value);
                    break;
                case 3:
                    long int64Value = 0;
                    ReadInt64(ref int64Value);
                    break;
                case 4:
                    ushort uint16Value = 0;
                    ReadUInt16(ref uint16Value);
                    break;
                case 5:
                    uint uint32Value = 0;
                    ReadUInt32(ref uint32Value);
                    break;
                case 6:
                    ulong uint64Value = 0;
                    ReadUInt64(ref uint64Value);
                    break;
                case 7:
                    float floatValue = 0f;
                    ReadFloat(ref floatValue);
                    break;
                case 8:
                    string stringValue = string.Empty;
                    ReadString(ref stringValue);
                    break;
                case 9:
                    bool boolValue = false;
                    ReadBool(ref boolValue);
                    break;
                case 12:
                    double doubleValue = 0d;
                    ReadDouble(ref doubleValue);
                    break;
                case 101:
                    var int16List = new List<short>();
                    ReadInt16(ref int16List);
                    break;
                case 102:
                case 110:
                    var int32List = new List<int>();
                    ReadInt32(ref int32List);
                    break;
                case 103:
                    var int64List = new List<long>();
                    ReadInt64(ref int64List);
                    break;
                case 104:
                    var uint16List = new List<ushort>();
                    ReadUInt16(ref uint16List);
                    break;
                case 105:
                    var uint32List = new List<uint>();
                    ReadUInt32(ref uint32List);
                    break;
                case 106:
                    var uint64List = new List<ulong>();
                    ReadUInt64(ref uint64List);
                    break;
                case 107:
                    var floatList = new List<float>();
                    ReadFloat(ref floatList);
                    break;
                case 108:
                    var stringList = new List<string>();
                    ReadString(ref stringList);
                    break;
                case 109:
                    var boolList = new List<bool>();
                    ReadBool(ref boolList);
                    break;
                case 111:
                    uint length = 0;
                    ReadUInt32(ref length);
                    for (int i = 0; i < length; i++)
                    {
                        uint bound = 0;
                        ReadUInt32(ref bound);
                        ConsumeData(bound);
                    }
                    break;
                default:
                    throw new Exception("Invalid tag type");
            }
        }

        private static readonly UTF8Encoding Encoding = new UTF8Encoding();

        public void ReadInt16(ref short value)
        {
            ValidateDataBound(sizeof(short));
            value = _binaryReader.ReadInt16();
        }

        public void ReadInt32(ref int value)
        {
            ValidateDataBound(sizeof(int));
            value = _binaryReader.ReadInt32();
        }

        public void ReadInt64(ref long value)
        {
            ValidateDataBound(sizeof(long));
            value = _binaryReader.ReadInt64();
        }

        public void ReadUInt16(ref ushort value)
        {
            ValidateDataBound(sizeof(ushort));
            value = _binaryReader.ReadUInt16();
        }

        public void ReadUInt32(ref uint value)
        {
            ValidateDataBound(sizeof(uint));
            value = _binaryReader.ReadUInt32();
        }

        public void ReadUInt64(ref ulong value)
        {
            ValidateDataBound(sizeof(ulong));
            value = _binaryReader.ReadUInt64();
        }

        public void ReadFloat(ref float value)
        {
            ValidateDataBound(sizeof(float));
            value = _binaryReader.ReadSingle();
        }

        public void ReadDouble(ref double value)
        {
            ValidateDataBound(sizeof(double));
            value = _binaryReader.ReadDouble();
        }

        public void ReadBool(ref bool value)
        {
            ValidateDataBound(sizeof(bool));
            value = _binaryReader.ReadBoolean();
        }

        public void ReadString(ref string value)
        {
            uint len = 0;
            ReadUInt32(ref len);
            ValidateDataBound(len);
            value = Encoding.GetString(_binaryReader.ReadBytes((int)len));
            if (ConvertNewLine)
            {
                value = value.Replace("\\n", "\n");
            }
        }

        public void ReadEnum<T>(ref T value)
        {
            int enumValue = 0;
            ReadInt32(ref enumValue);
            value = (T)Enum.ToObject(typeof(T), enumValue);
        }

        public void ReadInt16(ref List<short> values)
        {
            short value = 0;
            ReadInt16(ref value);
            values.Add(value);
        }

        public void ReadInt32(ref List<int> values)
        {
            int value = 0;
            ReadInt32(ref value);
            values.Add(value);
        }

        public void ReadInt64(ref List<long> values)
        {
            long value = 0;
            ReadInt64(ref value);
            values.Add(value);
        }

        public void ReadUInt16(ref List<ushort> values)
        {
            ushort value = 0;
            ReadUInt16(ref value);
            values.Add(value);
        }

        public void ReadUInt32(ref List<uint> values)
        {
            uint value = 0;
            ReadUInt32(ref value);
            values.Add(value);
        }

        public void ReadUInt64(ref List<ulong> values)
        {
            ulong value = 0;
            ReadUInt64(ref value);
            values.Add(value);
        }

        public void ReadBool(ref List<bool> values)
        {
            bool value = false;
            ReadBool(ref value);
            values.Add(value);
        }

        public void ReadString(ref List<string> values)
        {
            string value = string.Empty;
            ReadString(ref value);
            values.Add(value);
        }

        public void ReadFloat(ref List<float> values)
        {
            float value = 0f;
            ReadFloat(ref value);
            values.Add(value);
        }

        public void ReadEnum<T>(ref List<T> values)
        {
            T value = default;
            ReadEnum(ref value);
            values.Add(value);
        }

        public void ReadStruct<T>(ref T value) where T : ITableSerializable, new()
        {
            uint bound = 0;
            ReadUInt32(ref bound);
            value = new T();
            value.Deserialize(new TableReader(this, _binaryReader.BaseStream.Position + bound));
        }

        public void ReadStruct<T>(ref List<T> values) where T : ITableSerializable, new()
        {
            uint len = 0;
            ReadUInt32(ref len);
            for (int i = 0; i < len; i++)
            {
                T value = default;
                ReadStruct(ref value);
                values.Add(value);
            }
        }
    }
}
