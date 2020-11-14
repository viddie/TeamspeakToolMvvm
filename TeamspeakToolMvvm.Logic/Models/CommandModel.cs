using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamspeakToolMvvm.Logic.Models {
    public class CommandModel {
        //<controls:IconButton Grid.Row="1" Text="Open Settings" Icon="Cog" Height="25" Margin="10,10,10,0" Command="{Binding OpenSettingsCommand}" CommandParameter="" VerticalAlignment="Top"/>

        public string DisplayName { get; set; }
        public string IconName { get; set; } = "None";
        public RelayCommand Command { get; set; }
    }
}
