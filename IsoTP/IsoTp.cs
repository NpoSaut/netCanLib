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
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="Data">Данные для передачи</param>
        /// <returns>Объект транзакции</returns>
        public static TpSendTransaction Send(ICanSocket Socket, int TransmitDescriptor, int AcknowledgmentDescriptor, Byte[] Data, TimeSpan? Timeout = null)
        {
            var tr = new TpSendTransaction(Socket, TransmitDescriptor, AcknowledgmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            tr.Send(new TpPacket(Data));
            return tr;
        }

        /// <summary>
        /// Получает данные формата ISO-TP и возвращает объект транзакции
        /// </summary>
        /// <param name="Port">CAN-порт, через который осуществляется передача</param>
        /// <param name="TransmitDescriptor">Дескриптор передающего устройства</param>
        /// <param name="AcknowledgmentDescriptor">Дескриптор принимающего устройства</param>
        /// <returns>Объект транзакции</returns>
        public static TpReceiveTransaction Receive(ICanSocket Socket, int TransmitDescriptor, int AcknowledgmentDescriptor, TimeSpan? Timeout = null)
        {
            var tr = new TpReceiveTransaction(Socket, TransmitDescriptor, AcknowledgmentDescriptor);
            if (Timeout.HasValue) tr.Timeout = Timeout.Value;
            tr.Receive();
            return tr;
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(
            ICanSocket Socket,
            int RequestTransmitDescriptor, int RequestAcknowledgmentDescriptor,
            int AnswerTransmitDescriptor, int AnswerAcknowledgmentDescriptor,
            Byte[] RequestData, TimeSpan Timeout)
        {
            Send(Socket, RequestTransmitDescriptor, RequestAcknowledgmentDescriptor, RequestData).Wait();
            return Receive(Socket, AnswerTransmitDescriptor, AnswerAcknowledgmentDescriptor, Timeout);
        }

        public static TpReceiveTransaction SendRequestAndBeginReceive(ICanSocket Socket, int MasterDescriptor, int SlaveDescriptor, Byte[] RequestData, TimeSpan Timeout)
        {
            return SendRequestAndBeginReceive(Socket, MasterDescriptor, SlaveDescriptor, SlaveDescriptor, MasterDescriptor, RequestData, Timeout);
        }
    }
}
