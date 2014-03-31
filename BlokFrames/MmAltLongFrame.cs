using System;
using System.Runtime.InteropServices;

namespace BlokFrames
{
    [FrameDescriptor(0x4268)]
    public class MmAltLongFrame : BlokFrame
    {
        /// <summary>
        /// Достоверность данных
        /// </summary>
        public bool Reliable { get; set; }
        /// <summary>
        /// Широта (град.)
        /// </summary>
        /// <remarks>По Y</remarks>
        public Double Latitude { get; set; }
        /// <summary>
        /// Долгота (град.)
        /// </summary>
        /// <remarks>По X</remarks>
        public Double Longitude { get; set; }

        #region Перевод в радианы
        /// <summary>
        /// Широта (в радианах)
        /// </summary>
        /// <remarks>По Y</remarks>
        public Double LatitudeRad
        {
            get { return Latitude * Math.PI / 180.0; }
            set { Latitude = value * 180.0 / Math.PI; }
        }
        /// <summary>
        /// Долгота (в радианах)
        /// </summary>
        /// <remarks>По X</remarks>
        public Double LongitudeRad
        {
            get { return Longitude * Math.PI / 180.0; }
            set { Longitude = value * 180.0 / Math.PI; }
        } 
        #endregion

        public MmAltLongFrame() { }

        public MmAltLongFrame(Double Latitude, Double Longitude, Boolean IsReliable = true)
            : this()
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
            Reliable = IsReliable;
        }

        protected override byte[] Encode()
        {
            byte[] data = new byte[8];
            BitConverter.GetBytes((UInt32)(Latitude  * 10e9 * Math.PI / 180)).CopyTo(data, 0);
            BitConverter.GetBytes((UInt32)(Longitude * 10e9 * Math.PI / 180)).CopyTo(data, 4);
            data[7] |= (Byte)((Reliable ? 1 : 0) << 7);
            return data;
        }

        protected override void Decode(byte[] Data)
        {
            var ds = ByteArrayToStructure<DataStruct>(Data);
            this.Latitude = ds.Lat * 10e-9 * 180 / Math.PI;
            this.Longitude = (Int32)(ds.Lon | ((ds.Lon & (1 << 30)) << 1)) * 10e-9 * 180 / Math.PI;
            this.Reliable = (ds.Rel & (1 << 7)) != 0;
        }


        [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
        protected struct DataStruct
        {
            [MarshalAs(UnmanagedType.I4)]
            [FieldOffset(0)]
            public Int32 Lat;

            [MarshalAs(UnmanagedType.U4)]
            [FieldOffset(4)]
            public UInt32 Lon;

            [MarshalAs(UnmanagedType.U1)]
            [FieldOffset(7)]
            public byte Rel;
        }

    }
}
