using System.Collections.Generic;
using System.Windows;

namespace 挑战杯
{
    /// <summary>
    /// UpdateFood.xaml 的交互逻辑
    /// </summary>
    public partial class UpdateFood : Window
    {
        public UpdateFood()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //为了后期拓展，声明 List类型的newOrderList
            List<Food> newOrderList =new List<Food>();//更新菜品库List
            //判断输入框是否为空
            if (this.foodName.Text != "请输入菜品名称"&& this.foodPrice.Text != "请输入菜品价格" &this.foodProfile.Text != "请输入菜品简介")
            {
                Food newFood = new Food()
                {
                    Name = this.foodName.Text
                };
                newOrderList.Add(newFood);//

                //数据库订单更新
                string cmdText = "insert into Food(Name) values('" + newFood.Name + "')";
                var connection = DataBaseOperation.CreateConnection();
                connection.Open();
                DataBaseOperation.ReviseDataToDataBase(cmdText, connection);
                connection.Close();

                MessageBox.Show("添加成功");

                //更新厨房菜品库
               // DataSendAndRecv.UpdateOrderToTabel(newOrderList);

                //更新餐桌菜品库
               // DataSendAndRecv.UpdateOrderToKitchen(newOrderList);
               
            }
            else
            {
                MessageBox.Show("请完善菜品名、菜品价格、菜品介绍");
            }          
        }

        //优化用户体验
        #region
        /// <summary>
        /// 优化用户体验
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foodName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodName.Text == "请输入菜品名称")
            {
                this.foodName.Text = "";
            }

        }

        /// <summary>
        /// 优化用户体验
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foodPrice_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodPrice.Text == "请输入菜品价格")
            {
                this.foodPrice.Text = "";
            }
        }

        /// <summary>
        /// 优化用户体验
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foodProfile_GotFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodProfile.Text == "请输入菜品简介")
            {
                this.foodProfile.Text = "";
            }
        }
        

        private void foodName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodName.Text == "")
                this.foodName.Text = "请输入菜品名称";

        }

        private void foodPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodPrice.Text == "")
                this.foodPrice.Text = "请输入菜品价格";
        }
        private void foodProfile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.foodProfile.Text == "")
                this.foodProfile.Text = "请输入菜品简介";
        }

        #endregion

        /// <summary>
        /// 重置Textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foodName.Text = "请输入菜品名称";
            foodPrice.Text = "请输入菜品价格";
            foodProfile.Text = "请输入菜品简介";
        }


    }
}
