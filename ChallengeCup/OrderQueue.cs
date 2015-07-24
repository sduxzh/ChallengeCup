using System.Collections.Generic;

namespace 挑战杯
{
    public class OrderQueue
    {
        
        private Queue<Order> _orderQueue;//订单队列
        private int _orderNum;
        public OrderQueue()
        {
            _orderQueue =new Queue<Order>();//初始化订单队列
        }

        /// <summary>
        /// 将订单添加到订单队列中
        /// </summary>
        /// <param name="order"></param>
        public void AddToOderQueue(Order order)
        {
            //队列是否为空
            if (_orderQueue==null)
            {
                _orderQueue=new Queue<Order>();
            }
            else
            {
                _orderQueue.Enqueue(order);//将菜品添加进订单队列
                _orderNum++;
            }      
        }

        /// <summary>
        /// 从订单队列中取出订单
        /// </summary>
        /// <returns></returns>
        public Order  RemoveFromOderQueue()
        {
            if (_orderQueue == null) return null;
            var removedOrder =_orderQueue.Dequeue();
            _orderNum--;
            return removedOrder;
            //订单队列中无菜品，返回null
        }

        /// <summary>
        /// 获取订单队列中的订单数量
        /// </summary>
        /// <returns></returns>
        public int NumOfOrderQueue()
        {
            return _orderNum;
        }
    }
}