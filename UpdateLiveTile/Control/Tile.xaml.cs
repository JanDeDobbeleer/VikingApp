using System.Windows.Controls;
using System.Windows.Media;

namespace UpdateLiveTile.Control
{
    public partial class Tile : UserControl
    {
        public Tile(SolidColorBrush background, string credit, string data, string sms, string vSms, string vCall)
        {
            InitializeComponent();
            MainGrid.Background = background;
            Credit.Text = credit;
            Data.Text = data;
            Sms.Text = sms;
            VSms.Text = vSms;
            VCall.Text = vCall;
        }
    }
}
