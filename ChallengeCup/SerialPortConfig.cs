using System.IO.Ports;
namespace 挑战杯
{
    public class SerialPortConfig
    {
        public static SerialPort KitchenPort;//厨房数据交互串口
        public static SerialPort TablePort;//餐桌数据交互串口

        public SerialPortConfig(string kitchenPort, string tabelPort )
        {
            KitchenPort = new SerialPort(kitchenPort);
            TablePort=new SerialPort(tabelPort);
           
            //配置厨房串口
            KitchenPort.BaudRate = 9600;
            KitchenPort.Parity = Parity.None;
            KitchenPort.DataBits = 8;
            KitchenPort.StopBits = StopBits.One;

            //配置餐桌串口
            TablePort.BaudRate = 9600;
            TablePort.Parity = Parity.None;
            TablePort.DataBits = 8;
            TablePort.StopBits = StopBits.One;

            //打开串口
            if(!KitchenPort.IsOpen)
                KitchenPort.Open();
            if(!TablePort.IsOpen)
                TablePort.Open();

        }
    }
}