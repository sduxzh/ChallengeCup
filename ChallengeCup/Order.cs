using System;

namespace 挑战杯
{
    public class Order
    {
        public int Id;//菜品号
        public int SeqId;//订单队列序号
        public int TableId;//餐桌号
        public int OrderState;//订单当前制作状态(制作中，等待中，制作完成)
        public DateTime ReceiveFromDbTime;//订单从数据库读取所用时间
        public DateTime SendToKitchen;//订单发送到厨房的时间
        public DateTime StartMakeTime;//订单开始制作时间
        public DateTime FinishTime;//订单完成制作时间
        public int ForcastTime;//订单预计完成时间
        public int ReviseTime;//订单修正时间
    }
}