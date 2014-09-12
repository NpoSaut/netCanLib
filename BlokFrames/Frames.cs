using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlokFrames
{
    [FrameDescriptor(0x0803)]
    /// <summary>
    /// Сообщение формируется субмодулем B модуля МЦО нулевого комплекта каждые 500
    /// <summary>
    public class McoMode : BlokFrame
    {
        ///<summary>Режим движения</summary>
        public enum DriveModeKind : int
        {
            [System.ComponentModel.Description("Поездной")]
            ///<summary>Поездной</summary>
            TrainMode = 0,
            [System.ComponentModel.Description("Маневровый")]
            ///<summary>Маневровый</summary>
            ShuntingMode = 1,
            [System.ComponentModel.Description("Рабочий (Для КЛУБ-УП)")]
            ///<summary>Рабочий (Для КЛУБ-УП)</summary>
            WorkingMode = 2,
            [System.ComponentModel.Description("Двойная тяга")]
            ///<summary>Двойная тяга</summary>
            DoublePowerMode = 3,
        }
        
        [System.ComponentModel.Description("Режим движения")]
        /// <summary>Режим движения</summary>
        public DriveModeKind DriveMode { get; set; }
        
        ///<summary>Автоблокировочный режим движения</summary>
        public enum AutolockModeKind : int
        {
            [System.ComponentModel.Description("Обычное движение")]
            ///<summary>Обычное движение</summary>
            Normal = 0,
            [System.ComponentModel.Description("Движение по ПАБу")]
            ///<summary>Движение по ПАБу</summary>
            HalfAutolock = 1,
            [System.ComponentModel.Description("Движение по закрытой АБ")]
            ///<summary>Движение по закрытой АБ</summary>
            Autolock = 2,
            [System.ComponentModel.Description("Запрещённая комбинация")]
            ///<summary>Запрещённая комбинация</summary>
            Forbiddern = 3,
        }
        
        [System.ComponentModel.Description("Автоблокировочный режим движения")]
        /// <summary>Автоблокировочный режим движения</summary>
        public AutolockModeKind AutolockMode { get; set; }
        
        [System.ComponentModel.Description("Система многих единиц")]
        /// <summary>Система многих единиц</summary>
        public Boolean MultipleUnit { get; set; }
        
        [System.ComponentModel.Description("Диагностика САУТа")]
        /// <summary>Диагностика САУТа</summary>
        public Boolean SautDiag { get; set; }
        
        [System.ComponentModel.Description("Скорость движения по ПАБу или ЗАБу")]
        /// <summary>Скорость движения по ПАБу или ЗАБу</summary>
        public Byte RestrictedSpeed { get; set; }
        
        [System.ComponentModel.Description("Режим движения с подвижными блок-участками")]
        /// <summary>Режим движения с подвижными блок-участками</summary>
        public Boolean SlippingSections { get; set; }
        
        
        protected override void Decode(byte[] buff)
        {
            this.DriveMode = DecodeDriveMode(buff);
            this.AutolockMode = DecodeAutolockMode(buff);
            this.MultipleUnit = DecodeMultipleUnit(buff);
            this.SautDiag = DecodeSautDiag(buff);
            this.RestrictedSpeed = DecodeRestrictedSpeed(buff);
            this.SlippingSections = DecodeSlippingSections(buff);
        }
        
        protected override byte[] Encode()
        {
            var buff = new Byte[FrameLength];
            EncodeDriveMode(buff, DriveMode);
            EncodeAutolockMode(buff, AutolockMode);
            EncodeMultipleUnit(buff, MultipleUnit);
            EncodeSautDiag(buff, SautDiag);
            EncodeRestrictedSpeed(buff, RestrictedSpeed);
            EncodeSlippingSections(buff, SlippingSections);
            return buff;
        }
        
        private DriveModeKind DecodeDriveMode(Byte[] buff)
        {
            int raw = ((buff[0] & 0x30) >> 4);
            return (DriveModeKind)raw;
        }
        
        private void EncodeDriveMode(Byte[] buff, DriveModeKind value)
        {
            buff[0] = (byte)((buff[0] & ~0x30) | ((int)value & 0x30));
        }
        
        private AutolockModeKind DecodeAutolockMode(Byte[] buff)
        {
            int raw = ((buff[0] & 0x0c) >> 2);
            return (AutolockModeKind)raw;
        }
        
        private void EncodeAutolockMode(Byte[] buff, AutolockModeKind value)
        {
            buff[0] = (byte)((buff[0] & ~0x0c) | ((int)value & 0x0c));
        }
        
        private Boolean DecodeMultipleUnit(Byte[] buff)
        {
            int raw = ((buff[0] & 0x02) >> 1);
            return raw != 0;
        }
        
        private void EncodeMultipleUnit(Byte[] buff, Boolean value)
        {
            buff[0] = (byte)((buff[0] & ~0x02) | ((value ? 1 : 0) & 0x02));
        }
        
        private Boolean DecodeSautDiag(Byte[] buff)
        {
            int raw = (buff[0] & 0x01);
            return raw != 0;
        }
        
        private void EncodeSautDiag(Byte[] buff, Boolean value)
        {
            buff[0] = (byte)((buff[0] & ~0x01) | ((value ? 1 : 0) & 0x01));
        }
        
        private Byte DecodeRestrictedSpeed(Byte[] buff)
        {
            int raw = (buff[1] & 0x3f) | ((buff[2] & 0x0e) << 5);
            return unchecked((Byte)raw);
        }
        
        private void EncodeRestrictedSpeed(Byte[] buff, Byte value)
        {
            buff[1] = (byte)((buff[1] & ~0x3f) | (value & 0x3f));
            buff[2] = (byte)((buff[2] & ~0x0e) | ((value >> 6) & 0x0e));
        }
        
        private Boolean DecodeSlippingSections(Byte[] buff)
        {
            int raw = ((buff[2] & 0x80) >> 7);
            return raw != 0;
        }
        
        private void EncodeSlippingSections(Byte[] buff, Boolean value)
        {
            buff[2] = (byte)((buff[2] & ~0x80) | ((value ? 1 : 0) & 0x80));
        }
        
    }

    /// <summary>
    /// Описание параметров движения
    /// </summary>
    [FrameDescriptor(0x1888, HalfsetKind.SetA)]
    [FrameDescriptor(0x1A88, HalfsetKind.SetB)]
    public class IpdState : BlokFrame
    {
        /// <summary>Номер выполненного теста</summary>
        [System.ComponentModel.Description("Номер выполненного теста")]
        public int TestNumber { get; set; }

        /// <summary>Результат теста</summary>
        [System.ComponentModel.Description("Результат теста")]
        public int TestResult { get; set; }
        
        ///<summary>Направление движения</summary>
        public enum DirectionKind : int
        {
            ///<summary>Вперёд</summary>
            [System.ComponentModel.Description("Вперёд")]
            Ahead = 0,
            ///<summary>Назад</summary>
            [System.ComponentModel.Description("Назад")]
            Back = 1,
        }

        /// <summary>Направление движения</summary>
        [System.ComponentModel.Description("Направление движения")]
        public DirectionKind Direction { get; set; }
        
        ///<summary>Знак ускорения</summary>
        public enum AccelerationSignKind : int
        {
            ///<summary>Положительное</summary>
            [System.ComponentModel.Description("Положительное")]
            Positive = 0,
            ///<summary>Отрицательное</summary>
            [System.ComponentModel.Description("Отрицательное")]
            Negative = 1,
        }

        /// <summary>Знак ускорения</summary>
        [System.ComponentModel.Description("Знак ускорения")]
        public AccelerationSignKind AccelerationSign { get; set; }

        /// <summary>Наличие импульсов ДПС</summary>
        [System.ComponentModel.Description("Наличие импульсов ДПС")]
        public Boolean SpeedPulsesAvailable { get; set; }

        /// <summary>Фактическая скорость</summary>
        [System.ComponentModel.Description("Фактическая скорость")]
        public int Speed { get; set; }

        /// <summary>Линейная ордината</summary>
        [System.ComponentModel.Description("Линейная ордината")]
        public int LinearOrdinate { get; set; }

        /// <summary>Признак виртуальной кабины</summary>
        [System.ComponentModel.Description("Признак виртуальной кабины")]
        public Boolean IsVirtualCabin { get; set; }
        
        ///<summary>Номер виртуальной кабины</summary>
        public enum VirtualCabinKind : int
        {
            ///<summary>1 кабина</summary>
            [System.ComponentModel.Description("1 кабина")]
            Cabin1 = 0,
            ///<summary>2 кабина</summary>
            [System.ComponentModel.Description("2 кабина")]
            Cabin2 = 1,
        }

        /// <summary>Номер виртуальной кабины</summary>
        [System.ComponentModel.Description("Номер виртуальной кабины")]
        public VirtualCabinKind VirtualCabin { get; set; }

        /// <summary>Определение местоположения в ЭК</summary>
        [System.ComponentModel.Description("Определение местоположения в ЭК")]
        public Boolean EmapPosition { get; set; }
        
        ///<summary>Тест пассивного датчика по скорости</summary>
        public enum PassiveSensorSpeedTestStateKind : int
        {
            ///<summary>�?справен</summary>
            [System.ComponentModel.Description("�?справен")]
            Correct = 0,
            ///<summary>Сбой</summary>
            [System.ComponentModel.Description("Сбой")]
            Fault = 1,
        }

        /// <summary>Тест пассивного датчика по скорости</summary>
        [System.ComponentModel.Description("Тест пассивного датчика по скорости")]
        public PassiveSensorSpeedTestStateKind PassiveSensorSpeedTestState { get; set; }
        
        ///<summary>Номер активного датчика</summary>
        public enum ActiveSpeedSensorKind : int
        {
            ///<summary>Датчик 1</summary>
            [System.ComponentModel.Description("Датчик 1")]
            Sensor1 = 0,
            ///<summary>Датчик 2</summary>
            [System.ComponentModel.Description("Датчик 2")]
            Sensor2 = 1,
        }

        /// <summary>Номер активного датчика</summary>
        [System.ComponentModel.Description("Номер активного датчика")]
        public ActiveSpeedSensorKind ActiveSpeedSensor { get; set; }
        
        ///<summary>Тест пассивного датчика по количеству импульсов</summary>
        public enum PassiveSensorImpulseTestStateKind : int
        {
            ///<summary>�?справен</summary>
            [System.ComponentModel.Description("�?справен")]
            Correct = 0,
            ///<summary>Сбой</summary>
            [System.ComponentModel.Description("Сбой")]
            Fault = 1,
        }

        /// <summary>Тест пассивного датчика по количеству импульсов</summary>
        [System.ComponentModel.Description("Тест пассивного датчика по количеству импульсов")]
        public PassiveSensorImpulseTestStateKind PassiveSensorImpulseTestState { get; set; }
        
        
        protected override void Decode(byte[] buff)
        {
            TestNumber = DecodeTestNumber(buff);
            TestResult = DecodeTestResult(buff);
            Direction = DecodeDirection(buff);
            AccelerationSign = DecodeAccelerationSign(buff);
            SpeedPulsesAvailable = DecodeSpeedPulsesAvailable(buff);
            Speed = DecodeSpeed(buff);
            LinearOrdinate = DecodeLinearOrdinate(buff);
            IsVirtualCabin = DecodeIsVirtualCabine(buff);
            VirtualCabin = DecodeVirtualCabine(buff);
            EmapPosition = DecodeEmapPosition(buff);
            PassiveSensorSpeedTestState = DecodePassiveSensorSpeedTestState(buff);
            ActiveSpeedSensor = DecodeActiveSpeedSensor(buff);
            PassiveSensorImpulseTestState = DecodePassiveSensorImpulseTestState(buff);
        }
        
        protected override byte[] Encode()
        {
            var buff = new Byte[FrameLength];
            EncodeTestNumber(buff, TestNumber);
            EncodeTestResult(buff, TestResult);
            EncodeDirection(buff, Direction);
            EncodeAccelerationSign(buff, AccelerationSign);
            EncodeSpeedPulsesAvailable(buff, SpeedPulsesAvailable);
            EncodeSpeed(buff, Speed);
            EncodeLinearOrdinate(buff, LinearOrdinate);
            EncodeIsVirtualCabine(buff, IsVirtualCabin);
            EncodeVirtualCabine(buff, VirtualCabin);
            EncodeEmapPosition(buff, EmapPosition);
            EncodePassiveSensorSpeedTestState(buff, PassiveSensorSpeedTestState);
            EncodeActiveSpeedSensor(buff, ActiveSpeedSensor);
            EncodePassiveSensorImpulseTestState(buff, PassiveSensorImpulseTestState);
            return buff;
        }
        
        private int DecodeTestNumber(Byte[] buff)
        {
            int raw = ((buff[0] & 0xf0) >> 4);
            return unchecked((int)raw);
        }
        
        private void EncodeTestNumber(Byte[] buff, int value)
        {
            buff[0] = (byte)((buff[0] & ~0xf0) | (value & 0xf0));
        }
        
        private int DecodeTestResult(Byte[] buff)
        {
            int raw = (buff[0] & 0x0f);
            return unchecked((int)raw);
        }
        
        private void EncodeTestResult(Byte[] buff, int value)
        {
            buff[0] = (byte)((buff[0] & ~0x0f) | (value & 0x0f));
        }
        
        private DirectionKind DecodeDirection(Byte[] buff)
        {
            int raw = ((buff[1] & 0x80) >> 7);
            return (DirectionKind)raw;
        }
        
        private void EncodeDirection(Byte[] buff, DirectionKind value)
        {
            buff[1] = (byte)((buff[1] & ~0x80) | ((int)value & 0x80));
        }
        
        private AccelerationSignKind DecodeAccelerationSign(Byte[] buff)
        {
            int raw = ((buff[1] & 0x20) >> 5);
            return (AccelerationSignKind)raw;
        }
        
        private void EncodeAccelerationSign(Byte[] buff, AccelerationSignKind value)
        {
            buff[1] = (byte)((buff[1] & ~0x20) | ((int)value & 0x20));
        }
        
        private Boolean DecodeSpeedPulsesAvailable(Byte[] buff)
        {
            int raw = ((buff[1] & 0x04) >> 2);
            return raw != 0;
        }
        
        private void EncodeSpeedPulsesAvailable(Byte[] buff, Boolean value)
        {
            buff[1] = (byte)((buff[1] & ~0x04) | ((value ? 1 : 0) & 0x04));
        }
        
        private int DecodeSpeed(Byte[] buff)
        {
            int raw = buff[2] | ((buff[1] & 0x01) << 8);
            return unchecked((int)raw);
        }
        
        private void EncodeSpeed(Byte[] buff, int value)
        {
            buff[2] = (byte)(value & 0xff);
            buff[1] = (byte)((buff[1] & ~0x01) | ((value >> 8) & 0x01));
        }
        
        private int DecodeLinearOrdinate(Byte[] buff)
        {
            int raw = buff[4] | (buff[3] << 8) | (buff[5] << 16);
            return unchecked((int)raw);
        }
        
        private void EncodeLinearOrdinate(Byte[] buff, int value)
        {
            buff[4] = (byte)(value & 0xff);
            buff[3] = (byte)((value >> 8) & 0xff);
            buff[5] = (byte)((value >> 16) & 0xff);
        }
        
        private Boolean DecodeIsVirtualCabine(Byte[] buff)
        {
            int raw = ((buff[6] & 0x80) >> 7);
            return raw != 0;
        }
        
        private void EncodeIsVirtualCabine(Byte[] buff, Boolean value)
        {
            buff[6] = (byte)((buff[6] & ~0x80) | ((value ? 1 : 0) & 0x80));
        }
        
        private VirtualCabinKind DecodeVirtualCabine(Byte[] buff)
        {
            int raw = ((buff[6] & 0x40) >> 6);
            return (VirtualCabinKind)raw;
        }
        
        private void EncodeVirtualCabine(Byte[] buff, VirtualCabinKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x40) | ((int)value & 0x40));
        }
        
        private Boolean DecodeEmapPosition(Byte[] buff)
        {
            int raw = ((buff[6] & 0x20) >> 5);
            return raw != 0;
        }
        
        private void EncodeEmapPosition(Byte[] buff, Boolean value)
        {
            buff[6] = (byte)((buff[6] & ~0x20) | ((value ? 1 : 0) & 0x20));
        }
        
        private PassiveSensorSpeedTestStateKind DecodePassiveSensorSpeedTestState(Byte[] buff)
        {
            int raw = ((buff[6] & 0x10) >> 4);
            return (PassiveSensorSpeedTestStateKind)raw;
        }
        
        private void EncodePassiveSensorSpeedTestState(Byte[] buff, PassiveSensorSpeedTestStateKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x10) | ((int)value & 0x10));
        }
        
        private ActiveSpeedSensorKind DecodeActiveSpeedSensor(Byte[] buff)
        {
            int raw = ((buff[6] & 0x08) >> 3);
            return (ActiveSpeedSensorKind)raw;
        }
        
        private void EncodeActiveSpeedSensor(Byte[] buff, ActiveSpeedSensorKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x08) | ((int)value & 0x08));
        }
        
        private PassiveSensorImpulseTestStateKind DecodePassiveSensorImpulseTestState(Byte[] buff)
        {
            int raw = ((buff[6] & 0x04) >> 2);
            return (PassiveSensorImpulseTestStateKind)raw;
        }
        
        private void EncodePassiveSensorImpulseTestState(Byte[] buff, PassiveSensorImpulseTestStateKind value)
        {
            buff[6] = (byte)((buff[6] & ~0x04) | ((int)value & 0x04));
        }
        
    }
    
}


