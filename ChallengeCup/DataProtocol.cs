using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Enum;

namespace 挑战杯
{
    public class DataProtocol
    {
        public static List<Order> SendToKitchenList=new List<Order>();//发送到厨房的订单 


        /// <summary>
        /// 服务端发送至厨房
        /// orderCount代表将要发送到厨房的订单数 
        /// 队列处于动态维护状态（实时更新）
        /// </summary>
        /// <param name="orderQueue"></param>
        /// <param name="orderCount"></param>
        /// <returns></returns>
        public static char [] SendOrderToKitchen(OrderQueue orderQueue,int orderCount)
        {
            //更新订单waitSendToKitchenTime，数据库相关设置
            var connection = DataBaseOperation.CreateConnection();
            SqlDataAdapter adapter = null;
            DataSet dataset = null;
            string cmdText = null;  
                ;
            if (orderQueue == null)
                    return null;
            char[] dataChar = null;
            int auxiliaryNum = 1;  

            int numOfOrderQueue = orderCount;//设置发送的订单的数量
            dataChar = new char[numOfOrderQueue * 2];
            dataChar[0] = (char)WorkStateEnum.SendOrderToKitchen;//设置工作状态为发送订单

            for (int i = 1; i <= numOfOrderQueue; i++)
            {
                var tempOrder = orderQueue.RemoveFromOderQueue(); //将订单从等待队列中取出来
                tempOrder.SendToKitchen = DateTime.Now; //订单发送到厨房的时间
                SendToKitchenList.Add(tempOrder); //将订单放入发送到厨房的队列
                dataChar[auxiliaryNum] = (char) tempOrder.Id;
                dataChar[auxiliaryNum + 1] = (char) tempOrder.SeqId;
                auxiliaryNum = auxiliaryNum + 2;

                //判断waitSendToKitchenTime与统计时间误差,修改餐桌订单时间
                #region
                cmdText = "select * from Food where Id='" + tempOrder.Id + "'";
                adapter = DataBaseOperation.GetSqlDataAdapter(cmdText, connection);
                dataset = DataBaseOperation.GetDataSet(adapter);
                int deviation = int.Parse(dataset.Tables[0].Rows[0]["waitSendToKitchenTime"].ToString()) -
                                (tempOrder.SendToKitchen-tempOrder.ReceiveFromDbTime).Minutes;
                if (Math.Abs(deviation) >= (int) TimeDeviation.WaitSendToKitchenTime)
                {
                    DataSendAndRecv.ReviseOrderTimeToTable(tempOrder);//更新waitSendToKitchenTime
                }
                #endregion

            }

            dataChar[numOfOrderQueue * 2 + 2] = (char)'\0';//数据包结束符
            return dataChar;
                
        }

        /// <summary>
        /// 更新厨房菜品库
        /// </summary>
        /// <returns></returns>
        public static char [] UpdateOrderToKitchen(Food food)
        {
            //判断food是否为空
            if (food==null) return null;

            int foodNameLength = food.Name.ToCharArray().Length;
            var dataChar=new char[foodNameLength+4];//根据food长度确定数组长度
            //先赋值中间部分，然后再赋值剩余部分（工作状态、菜品Id、结束符、数据包结束符）
            var foodNameChar = food.Name.ToCharArray();//菜品名字char数组

            dataChar = CharArrayCopy(dataChar, foodNameChar, 2);//名字拷贝
            dataChar[0] = (char) WorkStateEnum.UpdateOrderToKitchen;//设置工作状态为更新厨房菜品库
            dataChar[1] = (char) food.Id;//设置菜品Id
            dataChar[foodNameLength + 1] = (char) 1;
            dataChar[foodNameLength + 2] = '\0';

            return dataChar;

        }

        /// <summary>
        /// 接收来自厨房的订单制作信息
        /// </summary>
        /// <param name="receiveChar"></param>
        /// <returns></returns>
        public static List<Order> ReveiveFromKitchen(char[] receiveChar)
        {
            List<Order> receuveList = new List<Order>();//辅助List
            if(receiveChar==null)
                return null;
            int orderNum = (receiveChar.Length)/2 - 1;//计算接收的订单的个数
            for (int i = 0; i < orderNum; i++)
            {
                Order tempOrder=new Order();//临时存储
                if(receiveChar[i*2+1]=='\0')
                    continue;
                tempOrder.Id = receiveChar[i*2+1];//订单id
                tempOrder.OrderState = receiveChar[i*2 + 2];//订单当前所处状态
                receuveList.Add(tempOrder);//将订单加入到辅助List
                
             }
             return receuveList;
        }

        /// <summary>
        /// 发送订单状态到餐桌
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="tabelId"></param>
        /// <returns></returns>
        public static char[] SendOrderToTabel(List<Order> orders,int tabelId)
        {
            if(orders.Count==0) return null;
            var orderCount = orders.Count;
            var dataChar = new char[orderCount*3 + 6];
            dataChar = ZigbeeAddressConvert(dataChar, tabelId);//设置餐桌Zigbee地址
            dataChar[4] = (char)WorkStateEnum.SendOrderToTable;//设置工作状态
            dataChar[orderCount*3 + 6 - 1] = '\0';//设置数据包结束符
            int auxiliaryNum = 5;//前4个字节为Zigbee地址,第5个字节为工作状态，第六个开始为订单状态
            foreach (var order  in orders)
            {
                dataChar[auxiliaryNum] = (char)order.Id;
                dataChar[auxiliaryNum + 1] = (char) order.ForcastTime;
                dataChar[auxiliaryNum + 2] = (char) order.OrderState;
                auxiliaryNum += 3;

            }
            return dataChar;

        }

        /// <summary>
        /// 修正订单时间
        /// </summary>
        /// <param name="order"></param>
        /// <param name="tabelId"></param>
        /// <returns></returns>
        public static char[] ReviseOrderTimeToTable(Order order,int tabelId)
        {
            if (order == null) return null;
            var dataChar = new char[9];
            dataChar = ZigbeeAddressConvert(dataChar, tabelId);//设置餐桌Zigbee地址
            dataChar[4] = (char)WorkStateEnum.ReviseOrderTimeToTable;//设置工作状态
            dataChar[5] = (char)order.Id;//设置订单Id
            dataChar[6] = (char) order.ReviseTime;//设置订单修正时间    
            dataChar[7] = (char) order.OrderState;//设置订单状态
            dataChar[8] = '\0';//结束符

            return dataChar;

        }

        /// <summary>
        /// 更新餐桌菜品库
        /// </summary>
        /// <param name="food"></param>
        /// <returns></returns>
        public static char[] UpdateOrderToTabel(Food food)
        {
            //判断food是否为空
            if(food==null) return null;
            int foodNameLength = food.Name.ToCharArray().Length;
            var dataChar=new char[foodNameLength+4];//根据food长度确定数组长度
            //先赋值中间部分，然后再赋值剩余部分（工作状态、菜品Id、结束符、数据包结束符）
            var foodNameChar = food.Name.ToCharArray();//菜品名字char数组

            dataChar = CharArrayCopy(dataChar, foodNameChar, 2);//名字拷贝
            dataChar[0] = (char) WorkStateEnum.UpdateOrderToTable;//设置工作状态为更新厨房菜品库
            dataChar[1] = (char) food.Id;//设置菜品Id
            dataChar[foodNameLength + 1] = (char) 1;
            dataChar[foodNameLength + 2] = '\0';

            return dataChar;
            
        }

        /// <summary>
        /// 数组拷贝，供更新终端菜品库使用
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="source"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static char [] CharArrayCopy(char[] dest, char[] source,int startIndex)
        {
            foreach (var tempchar in source)
            {
                dest[startIndex] = tempchar;
                startIndex++;
            }
            return dest;
            
        }

        /// <summary>
        /// 待完善、待完善
        /// 待完善、待完善
        /// 餐桌-Zigbee地址映射函数
        /// </summary>
        /// <param name="dataChar"></param>
        /// <param name="tableId"></param>
        /// <returns></returns>
        private static char[] ZigbeeAddressConvert(char[] dataChar, int tableId)
        {
            if (dataChar == null) return null;
            int index = 0;
            char [] addressCharArray=null;
            //带完善Zigbee地址映射表
            switch (tableId)
            {
                case 1:
                    addressCharArray = new char []{'0', '0', '0', '0'};
                    break;
            }
            if (addressCharArray == null) return dataChar;
            foreach (var tempchar in addressCharArray )
            {
                dataChar[index] = tempchar;
                index++;
            }
            return dataChar;
            
        }
    }
}