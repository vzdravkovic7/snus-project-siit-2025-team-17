using System;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using ClientApp.RobotArmServiceReference;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Linq;

namespace ClientApp
{
    public partial class MainWindow : Window
    {
        private RobotArmServiceClient client;
        private const int cellSize = 50;

        public MainWindow()
        {
            InitializeComponent();
            client = new RobotArmServiceClient();

            DrawGrid();
            // Pokreni periodično osvežavanje stanja ruke sa servera
            StartUpdatingRobotState();
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

        private int GetClientId()
        {
            if (int.TryParse(ClientIdTextBox.Text, out int id))
                return id;
            return 1;
        }

        private async void MoveLeft_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Left");
        private async void MoveRight_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Right");
        private async void MoveUp_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Up");
        private async void MoveDown_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Down");
        private async void Rotate_Click(object sender, RoutedEventArgs e) => await SendCommandAsync("Rotate");

        private async Task SendCommandAsync(string command)
        {
            int clientId = GetClientId();

            switch (command)
            {
                case "Left":
                    client.EnqueueMoveLeft(clientId);
                    break;
                case "Right":
                    client.EnqueueMoveRight(clientId);
                    break;
                case "Up":
                    client.EnqueueMoveUp(clientId);
                    break;
                case "Down":
                    client.EnqueueMoveDown(clientId);
                    break;
                case "Rotate":
                    client.EnqueueRotate(clientId);
                    break;
            }

            // Nakon slanja komande, odmah osveži poziciju sa servera
            await UpdateRobotArmPositionFromServer();
        }

        private async Task UpdateRobotArmPositionFromServer()
        {
            try
            {
                var state = await client.GetCurrentStateAsync(); // ovo vraća RobotArmState sa servera

                Canvas.SetLeft(RobotArmShape, state.X * cellSize + 5);
                Canvas.SetTop(RobotArmShape, state.Y * cellSize + 5);

                RobotArmShape.RenderTransform = new RotateTransform(state.Angle, RobotArmShape.Width / 2, RobotArmShape.Height / 2);
            }
            catch
            {
                // Ignoriši greške (server možda nije dostupan)
            }
        }

        private void StartUpdatingRobotState()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Dispatcher.InvokeAsync(async () => await UpdateRobotArmPositionFromServer());
                    await Task.Delay(500); // osvežavanje svakih 0.5s
                }
            });
        }
    }
}
