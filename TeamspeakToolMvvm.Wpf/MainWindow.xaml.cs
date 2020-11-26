using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
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
using TeamspeakToolMvvm.Logic.Messages;
using TeamspeakToolMvvm.Logic.ViewModels;

namespace TeamspeakToolMvvm.Wpf
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<AddLogMessage>(this, HandleAddLogMessage);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Messenger.Default.Send(new ApplicationClosingMessage());
            Messenger.Default.Unregister(this);
        }

        public void HandleAddLogMessage(AddLogMessage msg) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                SimpleIoc.Default.GetInstance<MainViewModel>().LogTexts.Insert(0, msg.Message);
            }));
        }
    }
}
