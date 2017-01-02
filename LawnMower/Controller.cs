using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LawnMower {

    public class Controller {
        const byte FLG_ESC = 0xFD;
        const byte FLG_STX = 0xFF;
        const byte FLG_ETX = 0xFE;

        const byte HDR_FUEL = 0x00;
        const byte HDR_GETDATA = 0x01;
        const byte HDR_PULSE = 0x02;

        public static Controller cont;

        private MainWindow win;
        private SerialPort serial;

        private byte[] rxBuf = new byte[32];
        private int rxNew = 0;
        private bool inFrame = false;
        private bool inEsc = false;

        private Dispatcher curDispatcher;

        public bool pollData;

       
        public Controller() {
            cont = this;
            win = new MainWindow(this);
            win.Show();
            startSerial();
            curDispatcher = Dispatcher.CurrentDispatcher;
        }

        public void startSerial() {
            serial = new SerialPort("COM3", 38400);
            try {
                serial.Open();
            } catch (Exception e) {
                win.errorQuit(e.ToString());
            }
            serial.DataReceived += readInst;
            
        }

        public void escapeAndSend(byte[] data) {
            byte[] outData = new byte[(data.Length * 2) + 2];
            int pos = 0;
            outData[pos++] = FLG_STX;
            foreach (byte b in data) {
                if (b == FLG_ESC || b == FLG_STX || b == FLG_ETX) {
                    outData[pos++] = FLG_ESC;
                }
                outData[pos++] = b;
            }
            outData[pos++] = FLG_ETX;
            serial.Write(outData, 0, pos);
        }

        public void sendFuel(byte x) {
            byte[] data = { HDR_FUEL, x };
            escapeAndSend(data);
        }

        public void sendGetData() {
            byte[] data = { HDR_GETDATA };
            escapeAndSend(data);
        }

        public void sendPulse() {
            byte[] data = { HDR_PULSE };
            escapeAndSend(data);
        }

        public void readInst(object sender, SerialDataReceivedEventArgs e) {
            while (serial.BytesToRead > 0) {
                byte b = (byte) serial.ReadByte();
                if (rxNew == rxBuf.Length) rxNew = 0;
                if (!inFrame) inEsc = false;
                if (!inEsc) {
                    if (b == FLG_ESC && inFrame) {
                        inEsc = true;
                    } else if (b == FLG_STX) {
                        clrBuf();
                        inFrame = true;
                    } else if (b == FLG_ETX) {
                        if (inFrame) processFrame();
                        inFrame = false;
                    } else {
                        if (inFrame) rxBuf[rxNew++] = b;
                    }
                } else {
                    inEsc = false;
                    if (inFrame) rxBuf[rxNew++] = b;
                }
            }
        }

        public void clrBuf() {
            rxNew = 0;
            inEsc = false;
            inFrame = false;
        }

        public void processFrame() {
            byte hdr = rxBuf[0];
            switch (hdr) {
                case HDR_GETDATA:
                    if (rxNew != 5) return;
                    int pulseWidth = rxBuf[1];
                    bool running = Convert.ToBoolean(rxBuf[2]);
                    double rpm = running ? 60 / (((int)((rxBuf[3] << 8) | rxBuf[4])) * 0.000016) : 0;
                    try {
                        curDispatcher.Invoke(() => {
                            win.showData(pulseWidth, running, rpm);
                        });
                    } catch (TaskCanceledException e) {
                        Console.WriteLine(e.ToString());
                    }
                    if (pollData) sendGetData();
                    break;
            }
        }


    }
}
