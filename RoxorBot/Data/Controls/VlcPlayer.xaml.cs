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
using Vlc.DotNet.Forms;

namespace RoxorBot.Data.Controls
{
    /// <summary>
    /// Interaction logic for VlcPlayer.xaml
    /// </summary>
    public partial class VlcPlayer
    {
        public static DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(VlcControl), typeof(VlcPlayer), new FrameworkPropertyMetadata(OnPlayerPropertyChanged));

        private static void OnPlayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var player = d as VlcPlayer;
            if (player == null)
                return;

            player.Child = e.NewValue as VlcControl;
        }

        public VlcControl Player
        {
            get { return (VlcControl)GetValue(PlayerProperty); }
            set { SetValue(PlayerProperty, value); }
        }

        public VlcPlayer()
        {
            InitializeComponent();
        }
    }
}
