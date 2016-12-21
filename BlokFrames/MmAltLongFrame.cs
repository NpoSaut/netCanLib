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
            BitConverter.GetBytes((Int32)(Latitude  * 1e9 * Math.PI / 180)).CopyTo(data, 0);
            BitConverter.GetBytes((Int32)(Longitude * 1e9 * Math.PI / 180)).CopyTo(data, 4);
            data[7] &= 0x7f;
            data[7] |= (Byte)((Reliable ? 0 : 1) << 7);
            return data;
        }

        protected override void Decode(byte[] Data)
        {
            int intLatitude = BitConverter.ToInt32(Data, 0);
            int intLongitude = BitConverter.ToInt32(Data, 4) & ~(1 << 31);

            Latitude = intLatitude * 180.0 / (1e9 * Math.PI);
            Longitude = intLongitude * 180.0 / (1e9 * Math.PI);
            Reliable = (Data[7] & (1 << 7)) == 0;
        }
    }
}
