using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ClientApp.RobotArmServiceReference;
using System.ServiceModel;
using System.Threading.Tasks;
using ClientApp.Helpers;

namespace ClientApp
{
    public partial class MainWindow : Window, IRobotArmServiceCallback
    {
        private RobotArmServiceClient client;
        private const int cellSize = 50;

        public MainWindow()
        {
            InitializeComponent();

            DrawGrid();
            SetRobotArmToCanvasCenter();

            var instanceContext = new InstanceContext(this);
            client = new RobotArmServiceClient(instanceContext);

            try
            {
                client.Subscribe();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void DrawGrid()
        {
            for (int i = 0; i <= 5; i++)
            {
                var hLine = new Line
                {
                    X1 = 0,
                    Y1 = i * cellSize,
                    X2 = 5 * cellSize,
                    Y2 = i * cellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                RobotCanvas.Children.Add(hLine);

                var vLine = new Line
                {
                    X1 = i * cellSize,
                    Y1 = 0,
                    X2 = i * cellSize,
                    Y2 = 5 * cellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                RobotCanvas.Children.Add(vLine);
            }
        }

        private void RobotArmShape_Loaded(object sender, RoutedEventArgs e)
        {
            SetRobotArmToCanvasCenter();
        }

        private void SetRobotArmToCanvasCenter()
        {
            double centerX = (RobotCanvas.Width - RobotArmShape.ActualWidth) / 2;
            double centerY = (RobotCanvas.Height - RobotArmShape.ActualHeight) / 2;

            Canvas.SetLeft(RobotArmShape, centerX);
            Canvas.SetTop(RobotArmShape, centerY);

            RobotArmShape.RenderTransform =
                new RotateTransform(0, RobotArmShape.ActualWidth / 2, RobotArmShape.ActualHeight / 2);
        }

        private int GetClientId()
        {
            if (int.TryParse(ClientIdTextBox.Text, out int id))
                return id;
            return -1;
        }

        private async void MoveLeft_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("MoveLeft");
        private async void MoveRight_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("MoveRight");
        private async void MoveUp_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("MoveUp");
        private async void MoveDown_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("MoveDown");
        private async void Rotate_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Rotate");

        private async Task SendCommandAsync(string command)
        {
            int clientId = GetClientId();
            string message = $"{clientId}:{command}";
            string hmac = SecurityHelper.ComputeHmac(message);

            try
            {
                switch (command)
                {
                    case "MoveLeft": await client.EnqueueMoveLeftAsync(clientId, hmac); break;
                    case "MoveRight": await client.EnqueueMoveRightAsync(clientId, hmac); break;
                    case "MoveUp": await client.EnqueueMoveUpAsync(clientId, hmac); break;
                    case "MoveDown": await client.EnqueueMoveDownAsync(clientId , hmac); break;
                    case "Rotate": await client.EnqueueRotateAsync(clientId, hmac); break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        public void OnStateChanged(RobotArmState state)
        {
            Dispatcher.Invoke(() =>
            {
                Canvas.SetLeft(RobotArmShape, state.X * cellSize + 5);
                Canvas.SetTop(RobotArmShape, state.Y * cellSize + 5);

                double centerX = RobotArmShape.ActualWidth / 2;
                double centerY = RobotArmShape.ActualHeight / 2;

                RobotArmShape.RenderTransform = new RotateTransform(state.Angle, centerX, centerY);
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                client.Unsubscribe();
                client.Close();
            }
            catch
            {
                client.Abort();
            }
            base.OnClosed(e);
        }
    }
}
