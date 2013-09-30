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
        /// Отправляет данные в формате ISO-TP и возвращает объект транзакции
        /// </summary>
        /// <param name="Port">CAN-порт, через который осуществляется передача</param>
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="AcknowlegmentDescriptor">Дескриптор принимающего устройства</param>
        /// <param name="Data">Данные для передачи</param>
        /// <returns>Объект транзакции</returns>
        public static TpSendTransaction Send(CanFlow Flow, int TransmitDescriptor, int AcknowlegmentDescriptor, Byte[] Data, TimeSpan? Timeout = null)
        {
            var tr = new TpSendTransaction(Flow, TransmitDescriptor, AcknowlegmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            tr.Send(new TpPacket(Data));
            return tr;
        }

        /// <summary>
        /// Получает данные формата ISO-TP и возвращает объект транзакции
        /// </summary>
        /// <param name="Port">CAN-порт, через который осуществляется передача</param>
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="AcknowlegmentDescriptor">Дескриптор принимающего устройства</param>
        /// <returns>Объект транзакции</returns>
        public static TpReceiveTransaction Receive(CanFlow Flow, int TransmitDescriptor, int AcknowlegmentDescriptor, TimeSpan? Timeout = null)
        {
            var tr = new TpReceiveTransaction(Flow, TransmitDescriptor, AcknowlegmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            tr.Receive();
            return tr;
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(
            CanFlow Flow,
            int RequestTransmitDescriptor, int RequestAcknowlegmentDescriptor,
            int AnsverTransmitDescriptor, int AnsverAcknowlegmentDescriptor,
            Byte[] RequestData, TimeSpan Timeout)
        {
            Send(Flow, RequestTransmitDescriptor, RequestAcknowlegmentDescriptor, RequestData).Wait();
            return Receive(Flow, AnsverTransmitDescriptor, AnsverAcknowlegmentDescriptor, Timeout);
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(CanFlow Flow, int MasterDescriptor, int SlaveDescriptor, Byte[] RequestData, TimeSpan Timeout)
        {
            return SendRequestAndBeginReceive(Flow, MasterDescriptor, SlaveDescriptor, SlaveDescriptor, MasterDescriptor, RequestData, Timeout);
        }
    }
}
