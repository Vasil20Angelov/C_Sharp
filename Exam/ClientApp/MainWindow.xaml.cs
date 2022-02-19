using EducationServices;
using SOAPService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IOrderService client;

        public MainWindow()
        {
            InitializeComponent();

            Random rand = new Random();
            Title = string.Format($"Order Client {rand.Next(1, 1000)}");
            client = new OrderService();

            Dispatcher.Invoke(new Action(() =>
            {
                Course[] courses = client.GetCourses();
                string[] titles = new string[courses.Length];
                for (int i = 0; i < courses.Length; i++)
                {
                    titles[i] = courses[i].Title;
                }
                CtrlOrderCourse.CboTitles.ItemsSource = titles;
            }));

            CtrlOrderCourse.OrderHandler += CtrlOrderCourse_OrderHandler;
        }

        private void CtrlOrderCourse_OrderHandler(object sender, OrderServiceEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    client.Write2File(Title, new Course(e.type, e.title, e.numOfStudents));
                    MessageBox.Show("Successful order!", "Order");
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString(), "Error", MessageBoxButton.OK);
                }
            }
            ));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Environment.Exit(0);
        }
    }
}
