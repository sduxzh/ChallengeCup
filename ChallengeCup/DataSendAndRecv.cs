
using System;
using System.Collections.Generic;
using System.Threading;

namespace 挑战杯
{
    public class DataSendAndRecv
    {
        ///  <summary>
        /// 将订单发送到厨房端
        /// 发送完成后，DataProtocol MakingList 中会记录发送到厨房的订单
        ///  数据发送成功，返回0
        ///  若串口位打开，返回1
        ///  </summary>
        ///  <param name="orderQueue"></param>
        /// <param name="orderCount"></param>
        public static int SendOrderToKitchen(OrderQueue orderQueue,int orderCount)
        {
            //判断串口是否打开
            if (SerialPortConfig.KitchenPort.IsOpen)
            {
                char[] data = DataProtocol.SendOrderToKitchen(orderQueue, orderCount);//将订单转换为char数组
                try
                {
                    SerialPortConfig.KitchenPort.Write(data, 0, data.Length);//发送数据
                    return 0;//数据发送成功
                }
                //捕捉异常，不知道怎么处理，哈哈
                catch (TimeoutException)
                {
                    return 1;
                }  
            }     

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig .KitchenPort.Open();//重新打开串口
            return 1;//串口未打开，提示重新发送数据
        }

        /// <summary>
        /// 接受来自厨房发来的订单的制作状态信息
        /// </summary>
        /// <returns></returns>
        public static List<Order> ReceiveFromKitchen()
        {
            char[] readBuffer=null;//接收来自厨房信息的数组
            if (SerialPortConfig.KitchenPort.IsOpen)
            {
                try
                {
                    if (SerialPortConfig.KitchenPort.BytesToRead != 0)
                    {
                        readBuffer=new char[SerialPortConfig.KitchenPort.BytesToRead+1];//根据要读取的数据量，创建数据数组
                        SerialPortConfig.KitchenPort.Read(readBuffer, 0,
                            SerialPortConfig.KitchenPort.BytesToRead);//读取数据
                    }

                    var receiveFromKitchenList = DataProtocol.ReveiveFromKitchen(readBuffer);
                    return receiveFromKitchenList;
                    
                }
                catch (Exception)
                {
                    return null;

                }
            }

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig.KitchenPort.Open();//重新打开串口
            return null;//串口未打开，提示重新发送数据
        }


        /// <summary>
        /// 更新厨房菜品库
        /// </summary>
        /// <param name="foodList"></param>
        /// <returns></returns>
        public static int UpdateOrderToKitchen(List<Food> foodList)
        {
            //判断串口是否打开
            if (SerialPortConfig.KitchenPort.IsOpen)
            {
                foreach (var food in foodList)
                {
                    var data = DataProtocol.UpdateOrderToKitchen(food);//将订单转换为char数组
                    try
                    {
                        SerialPortConfig.KitchenPort.Write(data, 0, data.Length);//发送数据
                        return 0;//数据发送成功
                    }
                    //捕捉异常，不知道怎么处理，哈哈
                    catch (TimeoutException)
                    {
                        return 1;
                    }
                }     
            }

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig.KitchenPort.Open();//重新打开串口
            return 1;//串口未打开，提示重新发送数据
                     
        }

        /// <summary>
        /// 发送订单状态到餐桌
        /// 后台订单查询函数查询到有订单，就要调用该函数
        /// orders为查询数据库获取到的订单
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="tabelId"></param>
        /// <returns></returns>
        public static int SendOrderToTabel(List<Order> orders,int tabelId)
        {
            //判断串口是否打开
            if (SerialPortConfig.TablePort.IsOpen)
            {
                char[] data = DataProtocol.SendOrderToTabel(orders, tabelId);//将订单转换为char数组
                try
                {
                    SerialPortConfig.TablePort.Write(data, 0, data.Length);//发送数据
                    return 0;//数据发送成功
                }
                //捕捉异常，不知道怎么处理，哈哈
                catch (TimeoutException)
                {
                    return 1;
                }
            }

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig.TablePort.Open();//重新打开串口
            return 1;//串口未打开，提示重新发送数据
        }
        
        /// <summary>
        /// 纠正订单剩余制作时间
        /// 订单信息更新后，调用本函数
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static int ReviseOrderTimeToTable(Order order)
        {
            //判断串口是否打开
            if (SerialPortConfig.TablePort.IsOpen)
            {
                char[] data = DataProtocol.ReviseOrderTimeToTable(order, order.TableId);//将订单转换为char数组
                try
                {
                    SerialPortConfig.TablePort.Write(data, 0, data.Length);//发送数据
                    return 0;//数据发送成功
                }
                //捕捉异常，不知道怎么处理，哈哈
                catch (TimeoutException)
                {
                    return 1;
                }
            }

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig.TablePort.Open();//重新打开串口
            return 1;//串口未打开，提示重新发送数据
        }

        /// <summary>
        /// 更新餐桌菜品库
        /// </summary>
        /// <param name="foodList"></param>
        /// <returns></returns>
        public static int UpdateOrderToTabel(List<Food> foodList)
        {
            //判断串口是否打开
            if (SerialPortConfig.TablePort.IsOpen)
            {
                foreach (var food in foodList)
                {
                    var data = DataProtocol.UpdateOrderToTabel(food);//将订单转换为char数组
                    try
                    {
                        SerialPortConfig.TablePort.Write(data, 0, data.Length);//发送数据
                        return 0;//数据发送成功
                    }
                    //捕捉异常，不知道怎么处理，哈哈
                    catch (TimeoutException)
                    {
                        return 1;
                    }
                }
            }

            //如果串口尚未打开，则自动打开串口
            TimeSpan waitTime = new TimeSpan(0, 0, 0, 0, 50);
            Thread.Sleep(waitTime);
            SerialPortConfig.TablePort.Open();//重新打开串口
            return 1;//串口未打开，提示重新发送数据
        }
    }   
}