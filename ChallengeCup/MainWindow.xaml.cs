using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Threading;
using Enum;

namespace 挑战杯
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// 变量
        /// </summary>
        #region
        private OrderQueue _orderQueue=new OrderQueue();//全局订单队列

        private bool _isFirst = true;//作用于后台获取数据库新订单，如果是第一次获取，则获取延迟为0
        private int _terminalNum = 12;//设置终端的数量
        private int _perfectSendToKicthenNum = 6;//发送到厨房端最佳订单数目
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitWindow();//初始化框体中的相关控件
            BackGroundQueryNewOrder();//后台获取新订单，将其添加到订单队列中
            UpdateSendToKitchenOrderState();//全局数据更新函数（更新发送到厨房订单状态、数据库来自点餐端订单信息、数据库菜品制作所需时间、维持厨房可用终端）
                                            //每更新一次，尝试发送订单到厨房
        }

        /// <summary>
        /// 始化函数
        /// </summary>
        private void InitWindow()
        {
            //配置UI元素
            #region
            TestLabel.Opacity = 0;
            #endregion

            //配置串口状态
            #region

            #endregion


        }


        /// <summary>
        /// 查询订单状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            BeginInvokeQureyNewOrder();
        }


        /// <summary>
        /// 异步查询数据库订单状态
        /// </summary>
        private  void BeginInvokeQureyNewOrder()
        {
            try
            {
                Func<DataSet> func =new Func<DataSet>(QureyOrderState);
                IAsyncResult iar = func.BeginInvoke(new AsyncCallback(EndInvokeWeather), func);

                /////////////////////////////////////////////////////////////////////
                /////////////////////////////////////////////////////////////////////
                //待拓展，后期添加加载图片
                //待拓展，后期添加加载图片
                //待拓展，后期添加加载图片
                this.TestLabel.Dispatcher.Invoke(new Action(delegate()
                {
                    TestLabel.Opacity = 1;
                    TestLabel.Content = "查询中....";
                }));
            }
            catch (Exception ex)
            {
                throw ex;

            }   
        }

        /// <summary>
        /// 异步订单查询回调函数
        /// </summary>
        /// <param name="iar"></param>
        private void EndInvokeWeather(IAsyncResult iar)
        {
            Func<DataSet> func = (Func<DataSet>) iar.AsyncState;//还原状态
            var dataset = func.EndInvoke(iar);//获取DataSet用于绑定DataGrid
            
            if (dataset != null)
            {
                var OrderNum = dataset.Tables[0].Rows.Count;//订单数量
                for (int i = 0; i < OrderNum; i++)
                {
                   
                    dataset.Tables[0].Rows[i]["OrderState"] = ConverOrderState(dataset.Tables[0].Rows[i]["OrderState"].ToString());
                }
                DataGrid.Dispatcher.Invoke(new Action(delegate()
                {
                    DataGrid.ItemsSource = dataset.Tables[0].DefaultView;//将数据库返回的DataSet绑定到DataGrid
                    ///////////////////////////////////////////////////////////////////////////
                    //待拓展，待拓展，与BeginInvokeQureyNewOrder对应，共同完成加载图片的动态显示
                    //待拓展，待拓展，与BeginInvokeQureyNewOrder对应，共同完成加载图片的动态显示
                    TestLabel.Content = "查询完成";
                    TestLabel.Opacity = 0;
                    ///////////////////////////////////////////////////////////////////////////
                }));
            }
            else
            {
                MessageBox.Show("系统异常，请稍后......");
            }
        }

        /// <summary>
        /// 查询数据库新订单，供BeginInvokeQureyNewOrder调用
        /// </summary>
        /// <returns></returns>
        private DataSet QureyOrderState()
        {
            string cmdText = "select * from FoodOrder";
            var cononection = DataBaseOperation.CreateConnection();//获取数据库连接
            cononection.Open();//打开连接
            var dataset = DataBaseOperation.GetDataSet(cmdText, cononection);
            cononection.Close();//关闭数据库连接，释放系统资源
            return dataset;
        }

        /// <summary>
        /// 将订单状态转化为中文
        /// </summary>
        /// <param name="orderstate"></param>
        /// <returns></returns>
        private string ConverOrderState(string orderstate)
        {
            string orderState=null;
            switch (orderstate)
            {
                case "1":
                    orderState= "等待制作";
                    break;
                case "2":
                    orderState = "制作中";
                    break;
                case "3":
                    orderState = "制作完成";
                    break;
            }
            return orderState;
        }

        /// <summary>
        /// 后台定时查询来自点餐端的新订单信息
        /// 将新订单加入到全局 _orderQueue中
        /// </summary>
        private void BackGroundQueryNewOrder()
        {
            List<Order> sendToTabelList=new List<Order>();//发送到餐桌的订单
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(10);//设定timer间隔，每隔10s查询一次
            timer.Tick+=new EventHandler(delegate(object s, EventArgs a)
            {
                var connection = DataBaseOperation.CreateConnection();//获取数据库连接
                connection.Open();//打开连接
                string cmdText = "select * from FoodOrder where OrderState=0";//查询新订单
                var adapter = DataBaseOperation.GetSqlDataAdapter(cmdText, connection);//获取adapter
                var dataSet = DataBaseOperation.GetDataSet(adapter);//获取dataSet
                SqlCommandBuilder commandBuilder=new SqlCommandBuilder(adapter);
                if (dataSet != null)
                {
                    var num = dataSet.Tables[0].Rows.Count;//获取新订单的数量
                    //将数据库订单存储到订单队列中，并更新数据库OrderStae状态
                    for (int i = 0; i < num; i++)
                    {
                        Order tempOrder = new Order
                        {
                            Id = int.Parse((dataSet.Tables[0].Rows[i]["Id"]).ToString()),//设置订单菜品号
                            SeqId = int.Parse((dataSet.Tables[0].Rows[i]["SeqId"]).ToString()),//设置订单队列序号
                            TableId = int.Parse((dataSet.Tables[0].Rows[i]["TableId"]).ToString()),//设置订单餐桌号
                            OrderState = (char)OrderState.Waiting,//设置订单状态
                            ReceiveFromDbTime = DateTime.Now, //设置从数据库读取到的时间
                        };

                        dataSet.Tables[0].Rows [i]["OrderState"] = (int)OrderState.Waiting;//将工作状态设置为“等待”，下次不再读取
                        //加锁（不解释了....）
                        lock (_orderQueue)
                        {
                            _orderQueue.AddToOderQueue(tempOrder);//将订单添加到订单队列
                            sendToTabelList.Add(tempOrder);//将订单添加到sendToTabelList队列，缺少预测时间
                        }
                    }
                    //将修改后的信息重新写入数据库
                    if (dataSet.HasChanges())
                    {
                        adapter.Update(dataSet);
                    }
                    connection.Close();//关闭连接                  
                }

                connection.Open();//打开数据连接
                
                //完善sendToTabelList队列中订单的预测时间
                for (int i=0;i<sendToTabelList.Count;i++)
                {
                    cmdText = "select * from Food where Id='" + sendToTabelList[i].Id + "' ";
                    adapter = DataBaseOperation.GetSqlDataAdapter(cmdText, connection);//获取adapter
                    dataSet = DataBaseOperation.GetDataSet(adapter);//获取dataSet
                    //设置order的预测时间ForcastTime
                    sendToTabelList[i].ForcastTime =
                        int.Parse((dataSet.Tables[0].Rows[0]["waitSendToKitchenTime"]).ToString()) +
                        int.Parse((dataSet.Tables[0].Rows[0]["waitToMakeTime"]).ToString()) +
                        int.Parse((dataSet.Tables[0].Rows[0]["makingTime"]).ToString());

                } 

                //---------------------------------------------------------------//
                //这里方法有问题，不保所有的订单都是同一个餐桌的，后期需要进行改进
                //DataSendAndRecv.SendOrderToTabel(sendToTabelList,sendToTabelList[0].TableId);
                //---------------------------------------------------------------//
                //---------------------------------------------------------------//
            });
            timer.Start();//启动定时器
        }

        /// <summary>
        /// 将订单发送到厨房
        /// </summary>
        private void SendOrderToKicthen()
        {
            //锁定_orderQueue(不解释....)
            lock (_orderQueue)
            {
                //_orderQueue != null可有可无
                if (_orderQueue != null)
                {
                    //第一次向厨房中发送订单
                    if (_isFirst)
                    {
                        //如果订单数大于12单，那么向厨房中发送12单
                        if (_orderQueue.NumOfOrderQueue() >= 12)
                        {
                            DataSendAndRecv.SendOrderToKitchen(_orderQueue, 12);//向厨房中发送12个订单
                            _terminalNum -= 12;
                            _isFirst = false;//设置标志位（是否为第一次向厨房发送订单）
                        }
                        //如果订单小于12单，那么将所有订单发送到厨房
                        else
                        {
                            var tempNum = _orderQueue.NumOfOrderQueue();//获取发送的订单数量，供更新_terminalNum
                            DataSendAndRecv.SendOrderToKitchen(_orderQueue, _orderQueue.NumOfOrderQueue());//向厨房发送所有订单
                            _terminalNum -= tempNum;
                            _isFirst = false;//设置标志位（是否为第一次向厨房发送订单）
                        }
                    }
                    //如果不是第一次向厨房发送订单,那么DataProtocol中的MakingList不为空
                    else
                    {
                        if (_terminalNum >= _perfectSendToKicthenNum)
                        {
                            //提高厨房上位机接收数据效率，故限制最小发订单发送数量
                            if (_orderQueue.NumOfOrderQueue() >= _perfectSendToKicthenNum)
                                DataSendAndRecv.SendOrderToKitchen(_orderQueue, _perfectSendToKicthenNum);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 后台运行
        /// 更新发送到厨房的订单队列状态
        /// 更新数据库订单状态
        /// 更新厨房可用终端数
        /// </summary>
        private void UpdateSendToKitchenOrderState()
        {

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(60);//设定timer间隔，每隔60s做一次数据更新
            timer.Tick += new EventHandler(delegate(object s, EventArgs a)
            {
               
                int finishedOrderNum = 0;
                var receiveFromKitchenList = DataSendAndRecv.ReceiveFromKitchen(); //获取来自厨房发来的数据
                //对厨房订单中的数量进行判断，如果数量为0，则说明没有收到厨房发送的信息
                if (receiveFromKitchenList.Count != 0)
                {
                    int index = -1;//在SendToKitchenList队列中查找处于Making状态订单的索引
                    foreach (var tempOrder in receiveFromKitchenList)
                    {
                        //订单处于制作状态
                        if (tempOrder.OrderState == (int)OrderState.Making)
                        {
                            //对receiveFromKitchenList加锁处理，DataSendAndRecv.SendOrderToKitchen会修改SendToKitchenList
                            lock (DataProtocol.SendToKitchenList)
                            {

                                index = DataProtocol.SendToKitchenList.FindIndex(x => x.SeqId == tempOrder.SeqId);//在SendToKitchenList队列中查找处于Making状态订单的索引
                                DataProtocol.SendToKitchenList[index].OrderState = (int)OrderState.Making;//更新发送到厨房的订单的状态
                                DataProtocol.SendToKitchenList[index].StartMakeTime = DateTime.Now;//订单开始制作的时间
                            }

                            string cmdText = "update FoodOrder set OrderState='"+OrderState.Making+"' where SeqId='" + tempOrder.SeqId + "'";//数据库更新字符串
                            var connection = DataBaseOperation.CreateConnection();//获取数据库连接
                            connection.Open();//打开数据库连接
                            DataBaseOperation.ReviseDataToDataBase(cmdText, connection);//执行数据库更新操作
                            connection.Close();//关闭数据库连接

                            //判断waitToMakeTime与统计时间误差,修改餐桌订单时间
                            #region
                            cmdText = "select * from Food where Id='" + DataProtocol.SendToKitchenList[index].Id + "'";
                            var adapter = DataBaseOperation.GetSqlDataAdapter(cmdText, connection);
                            var dataset = DataBaseOperation.GetDataSet(adapter);
                            int deviation = int.Parse(dataset.Tables[0].Rows[0]["waitToMakeTime"].ToString()) -
                                            (DataProtocol.SendToKitchenList[index].StartMakeTime - DataProtocol.SendToKitchenList[index].SendToKitchen).Minutes;
                            if (Math.Abs(deviation) >= (int)TimeDeviation.WaitToMakeTime)
                            {
                                DataSendAndRecv.ReviseOrderTimeToTable(DataProtocol.SendToKitchenList[index]);
                            }
                            #endregion

                        }

                        //订单处于完成状态（前提：订单处于制作状态）
                        if (tempOrder.OrderState == (int)OrderState.Finished)
                        {
                            finishedOrderNum++;
                            lock (DataProtocol.SendToKitchenList)
                            {
                                index = DataProtocol.SendToKitchenList.FindIndex(x => x.SeqId == tempOrder.SeqId);//在SendToKitchenList队列中查找处于Finished状态订单的索引
                                //订单完成制作前，一定处于制作状态
                                if (DataProtocol.SendToKitchenList[index].OrderState == (int)OrderState.Making)
                                {
                                    DataProtocol.SendToKitchenList[index].OrderState = (int)OrderState.Finished;//更新发送到厨房的订单的状态
                                    DataProtocol.SendToKitchenList[index].FinishTime = DateTime.Now;//订单完成制作的时间
                                }

                                //判断makingTime与统计时间误差,修改餐桌订单时间
                                #region
                                string cmdText = "select * from Food where Id='" + DataProtocol.SendToKitchenList[index].Id + "'";
                                var connection = DataBaseOperation.CreateConnection();//获取数据库连接
                                var adapter = DataBaseOperation.GetSqlDataAdapter(cmdText, connection);
                                var dataset = DataBaseOperation.GetDataSet(adapter);
                                int deviation = int.Parse(dataset.Tables[0].Rows[0]["makingTime"].ToString()) -
                                                (DataProtocol.SendToKitchenList[index].FinishTime - DataProtocol.SendToKitchenList[index].StartMakeTime).Minutes;
                                if (Math.Abs(deviation) >= (int)TimeDeviation.MakingTime)
                                {
                                    DataSendAndRecv.ReviseOrderTimeToTable(DataProtocol.SendToKitchenList[index]);
                                }
                                #endregion


                                //计算各个阶段的时间
                                #region
                                string waitSendToKitchenTim =
                                    (DataProtocol.SendToKitchenList[index].SendToKitchen -
                                     DataProtocol.SendToKitchenList[index].ReceiveFromDbTime).Minutes.ToString();
                                string waitToMakeTime =
                                    (DataProtocol.SendToKitchenList[index].StartMakeTime -
                                     DataProtocol.SendToKitchenList[index].SendToKitchen).Minutes.ToString();
                                string makingTime =
                                    (DataProtocol.SendToKitchenList[index].FinishTime -
                                     DataProtocol.SendToKitchenList[index].StartMakeTime).Minutes.ToString();
                                #endregion

                                //数据发送到数据库MakeOrderTime表（seqId,waitSendToKitchenTime,waitToMakeTime,makingTime
                                //数据发送到数据库FoodOrder表，更新OrderState状态
                                string cmdText1 =
                                    "insert into MakeOrderTime(seqId,waitSendToKitchenTime,waitToMakeTime,makingTime) values('" +
                                    tempOrder.SeqId + "','" + waitSendToKitchenTim + "','" + waitToMakeTime + "','" +
                                    makingTime + "')";
                                string cmdText2 = "update FoodOrder set OrderState='"+OrderState.Finished+"' where SeqId='" + tempOrder.SeqId + "'";

                                //数据库数据更新
                                connection = DataBaseOperation.CreateConnection();
                                connection.Open();
                                DataBaseOperation.ReviseDataToDataBase(cmdText1, connection);//更新MakeOrderTime表
                                DataBaseOperation.ReviseDataToDataBase(cmdText2, connection);//更新FoodOrder表
                                connection.Close();

                            }
                        }
                    }

                    //然后再将完成的订单从DataProtocol.SendToKitchenList队列中删除
                    DataProtocol.SendToKitchenList.RemoveAll(x => x.OrderState == (int)OrderState.Finished);
                    _terminalNum += finishedOrderNum;//更新可用终端数

                    //触发SendOrderToKicthen,根据需求决定是否将订单发送到厨房
                    SendOrderToKicthen();
                }
            });
            
        }

        
        /// <summary>
        /// 打开更新菜品库窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new UpdateFood().Show();

        }
    }
}
