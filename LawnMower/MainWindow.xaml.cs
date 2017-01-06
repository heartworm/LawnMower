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

namespace LawnMower {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private Controller c;
        public MainWindow(Controller c) {
            this.c = c;
            this.DataContext = this;
            InitializeComponent();
        }

        private void sldFuel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            txtFuel.Text = sldFuel.Value.ToString();
            if (chkAutoSend.IsChecked.Value) c.sendFuel((byte)sldFuel.Value);
        }

        private void btnPulse_Click(object sender, RoutedEventArgs e) {
            c.sendPulse();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e) {
            c.sendFuel((byte)sldFuel.Value);
        }

        private void btnGetData_Click(object sender, RoutedEventArgs e) {
            c.sendGetData();
        }

        public void showData(int pulseLength, bool running, bool revLimit, double rpm) {
            txtData.Text = String.Format("PulseLength: {0}\nRunning: {1}\nLimiter: {2}\nRotInt: {3}\n", 
                pulseLength, running, revLimit, rpm);
        }

        public void errorQuit(string msg) {
            MessageBox.Show(msg, "Fatal Error");
            Application.Current.Shutdown();
        }

        private void chkPollData_Changed(object sender, RoutedEventArgs e) {
            c.pollData = chkPollData.IsChecked.Value;
        }
    }
}
