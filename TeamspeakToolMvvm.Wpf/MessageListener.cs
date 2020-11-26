using AdvancedSettings.Logic.Messages;
using AdvancedSettings.Wpf.Views;
using TeamspeakToolMvvm.Logic.ViewModels;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TeamspeakToolMvvm.Logic.Messages;

namespace TeamspeakToolMvvm.Wpf
{
    public class MessageListener
    {
        /// <summary>
        /// Dummy property to have something to bind to
        /// </summary>
        public bool BindableProperty => true;

        public AdvancedSettings.Wpf.MessageListener SettingsMessageListener { get; set; }

        public MessageListener()
        {
            SettingsMessageListener = new AdvancedSettings.Wpf.MessageListener();
            InitMessenger();
        }


        private void InitMessenger()
        {
            Messenger.Default.Register<OpenSettingsViewMessage<MySettingsViewModel>>(SettingsMessageListener, SettingsMessageListener.HandleOpenSettingsView);
            Messenger.Default.Register<StopApplicationMessage>(this, StopApplication);
        }

        public void StopApplication(StopApplicationMessage msg) {
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                Application.Current.Shutdown();
            }));
        }
    }
}
