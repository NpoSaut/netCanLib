using System;
using System.Collections.Generic;

namespace Communications
{
    /// <summary>
    /// ����, ��������� � ����������� �������� ����������� ����
    /// </summary>
    /// <typeparam name="TSocket">��� ������</typeparam>
    /// <typeparam name="TDatagram">��� �����������</typeparam>
    public abstract class PipedPortBase<TSocket, TDatagram> : PortBase<TSocket, TDatagram>
        where TSocket : ISocket<TDatagram>
    {
        /// <summary>����� ��� �������� ����������</summary>
        public ISendPipe<TDatagram> SendPipe { get; private set; }
        /// <summary>����� ��� ����� ����������</summary>
        public IReceivePipe<TDatagram> ReceivePipe { get; private set; }

        /// <summary>
        /// ������ ��������� �����, ����������� � ������ ������� ����������� ����
        /// </summary>
        /// <param name="Name">��� �����</param>
        /// <param name="SendPipe">�����, � ������� ����� �������� ������������ ��������� �� �������</param>
        /// <param name="ReceivePipe">�����, �� ������� ��������� �������� ���������</param>
        public PipedPortBase(string Name, ISendPipe<TDatagram> SendPipe, IReceivePipe<TDatagram> ReceivePipe) : base(Name)
        {
            this.SendPipe = SendPipe;
            this.ReceivePipe = ReceivePipe;
            ReceivePipe.DatagramsReceived += ReceivePipeOnDatagramsReceived;
        }

        private void ReceivePipeOnDatagramsReceived(object Sender, DatagramsReceivedEventArgs<TDatagram> e)
        {
            OnDatagramsReceived(e.Datagrams);
        }

        /// <summary>��������� �������� �������� �� ������ ����������.</summary>
        protected override void SendingImplementation(ISocketBackend<TDatagram> Source, IList<TDatagram> Data)
        {
            SendPipe.Send(Data);
        }
    }
}