using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using SlimDX.DirectInput;

namespace DirectInputApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Joystick _joystick;
        private IList<DeviceInstance> _devices;
        private DirectInput _directInput = new DirectInput();
        private int fatness;
        private int red = 0;
        private int green = 0;
        private int blue = 0;
        public MainWindow()
        {
            var devices = _directInput.GetDevices();
            _devices = devices;
            InitializeComponent();

            foreach (var device in devices)
            {
                devicesComboBox.Items.Add(device.InstanceName);
            }
        }

        private async Task SetState()
        {
            while(true)
            {
                var x = _joystick.GetCurrentState().X;
                var y = _joystick.GetCurrentState().Y;

                var buttons = _joystick.GetCurrentState().GetButtons();
                var leftButton = buttons.ElementAt(2);
                var rightButton = buttons.ElementAt(0);
                var rButton = buttons.ElementAt(3);
                var gButton = buttons.ElementAt(1);
                var bButton = buttons.ElementAt(4);
                var clearButton = buttons.ElementAt(5);
                var closeButton = buttons.ElementAt(8);
                SetMouse(x, y, leftButton);
                SetJoystickPosition(x, y);
                SetSlider();
                SetTriggers(leftButton, rightButton);
                SetRGB(rButton, gButton, bButton);
                SetCanvas(x, y, rightButton, red, green, blue, clearButton);
                checkClose(closeButton);

                await Task.Delay(1);
            }
        }

        private void SetSlider()
        {
            slider.Value = _joystick.GetCurrentState().Z;
            fatness = (int)_joystick.GetCurrentState().Z / 4096;
        }

        private void SetJoystickPosition(int x, int y)
        {
            xBox.Text = x.ToString();
            yBox.Text = y.ToString();
        }

        private void SetTriggers(bool leftButton, bool rightButton)
        {
            leftTrigger.Background = leftButton ? new SolidColorBrush(Color.FromRgb(0, 255, 0)) : new SolidColorBrush(Color.FromRgb(255, 0, 0));
            rightTrigger.Background = rightButton ? new SolidColorBrush(Color.FromRgb(0, 255, 0)) : new SolidColorBrush(Color.FromRgb(255, 0, 0));
        }
        private void  SetRGB(bool rButton, bool gButton, bool bButton)
        {

            if (rButton)
            {
                red += 1;
                if (red > 255)
                    red = red % 256;
            }
            if (gButton)
            { 
                green += 1;
                if  (green > 255)
                    green = green % 256;
            }
            if (bButton)
            {
                blue += 1;
                if (blue > 255)
                    blue = blue % 256;
            }
            rgbBox.Text = "red: " + red + "\ngreen: " + green + "\nblue: " + blue;
            rbgCanvas.Background = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
        }
        private void SetCanvas(int x, int y, bool leftButton, int red, int green, int blue, bool clearButton)
        {
            var elipse = CreateDot(x, y);
            canvasJoystick.Children.Clear();
            canvasJoystick.Children.Add(elipse);

            var elipse2 = CreateDot(x, y);
            canvasJoystickPaint.Children.Add(elipse2);

            if(!leftButton)
            {
                canvasJoystickPaint.Children.Remove(elipse2);
            }

            if(clearButton)
            {
                canvasJoystickPaint.Children.Clear();
      
            }
        }

        private void checkClose(bool closeButton)
        {
            if (closeButton)
                this.Close();
        }

        private void SetMouse(int x, int y, bool leftButton)
        {
            //var screenWidth = SystemParameters.PrimaryScreenWidth;
            //var screenHeight = SystemParameters.PrimaryScreenHeight;
            var screenWidth = canvasJoystickPaint.Width;
            var screenHeight = canvasJoystickPaint.Height;

            if (leftButton)
            {
                Win32.DoMouseClick((uint)(x * screenWidth / 65535), (uint)(y * screenHeight / 65535));
            }

            Win32.SetCursorPos((int)((x * screenWidth / 65535)+canvasJoystickPaint.Margin.Left+this.Left+7), (int)((y * screenHeight / 65535)+canvasJoystickPaint.Margin.Top+this.Top+29));
        }

        private Ellipse CreateDot(int x, int y)
        {
            var elipse = new Ellipse();
            elipse.Height = 23-fatness;
            elipse.Width = 23-fatness;
            elipse.Fill = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
            Canvas.SetLeft(elipse, x * canvasJoystickPaint.Width / 65535 - elipse.Width / 2 - 2);
            Canvas.SetTop(elipse, y * canvasJoystickPaint.Height / 65535 - elipse.Height / 2 - 2);
            return elipse;
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
  
            var pad = _devices.First(x => x.InstanceName == (string)devicesComboBox.SelectedItem);
            _joystick = new Joystick(_directInput, pad.InstanceGuid);
            _joystick.Acquire();
            await SetState();
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            canvasJoystickPaint.Children.Clear();
        }
    }
}