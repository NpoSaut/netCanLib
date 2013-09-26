using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Communications.Can;

namespace Communications.Protocols.IsoTP
{
    public static class IsoTp
    {
        /// <summary>
        /// Запускает асинхронный процесс передачи данных и возвращает объект транзакции
        /// </summary>
        /// <param name="Port">CAN-порт, через который осуществляется передача</param>
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="AcknowlegmentDescriptor">Дескриптор принимающего устройства</param>
        /// <param name="Data">Данные для передачи</param>
        /// <returns>Объект транзакции</returns>
        public static TpSendTransaction BeginSend(CanFlow Flow, int TransmitDescriptor, int AcknowlegmentDescriptor, Byte[] Data, TimeSpan? Timeout = null)
        {
            var tr = new TpSendTransaction(Flow, TransmitDescriptor, AcknowlegmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            System.Threading.Tasks.Task.Factory.StartNew(() => tr.Send(new TpPacket(Data)));
            return tr;
        }

        /// <summary>
        /// Запускает асинхронный процесс получения данных и возвращает объект транзакции
        /// </summary>
        /// <param name="Port">CAN-порт, через который осуществляется передача</param>
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="AcknowlegmentDescriptor">Дескриптор принимающего устройства</param>
        /// <returns>Объект транзакции</returns>
        public static TpReceiveTransaction BeginReceive(CanFlow Flow, int TransmitDescriptor, int AcknowlegmentDescriptor, TimeSpan? Timeout = null)
        {
            var tr = new TpReceiveTransaction(Flow, TransmitDescriptor, AcknowlegmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            System.Threading.Tasks.Task.Factory.StartNew(() => tr.Receive());
            return tr;
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(
            CanFlow Flow,
            int RequestTransmitDescriptor, int RequestAcknowlegmentDescriptor,
            int AnsverTransmitDescriptor, int AnsverAcknowlegmentDescriptor,
            Byte[] RequestData, TimeSpan Timeout)
        {
            BeginSend(Flow, RequestTransmitDescriptor, RequestAcknowlegmentDescriptor, RequestData).Wait();
            return BeginReceive(Flow, AnsverTransmitDescriptor, AnsverAcknowlegmentDescriptor, Timeout);
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(CanFlow Flow, int MasterDescriptor, int SlaveDescriptor, Byte[] RequestData, TimeSpan Timeout)
        {
            return SendRequestAndBeginReceive(Flow, MasterDescriptor, SlaveDescriptor, SlaveDescriptor, MasterDescriptor, RequestData, Timeout);
        }
    }
}
