using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Shapes;

namespace Paint
{
    /// <summary>
    /// Logika interakcji dla klasy ColorEditWindow.xaml
    /// </summary>
    /// 

    public partial class ColorEditWindow : Window
    {
        public Color SelectedColor { get; set; }


        public ColorEditWindow()
        {
            InitializeComponent();


            // Wczytaj wartości z Properties, jeśli istnieją
            if (Application.Current.Properties.Contains("Red"))
                RedTextBox.Text = Application.Current.Properties["Red"].ToString();
            else RedTextBox.Text = "0";
            if (Application.Current.Properties.Contains("Green"))
                GreenTextBox.Text = Application.Current.Properties["Green"].ToString();
            else GreenTextBox.Text = "0";
            if (Application.Current.Properties.Contains("Blue"))
                BlueTextBox.Text = Application.Current.Properties["Blue"].ToString();
            else BlueTextBox.Text = "0";


        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateRGBInputs())
            {
                // Pobierz wartości RGB z TextBox-ów
                byte red = Convert.ToByte(RedTextBox.Text);
                byte green = Convert.ToByte(GreenTextBox.Text);
                byte blue = Convert.ToByte(BlueTextBox.Text);

                // Ustaw wybrany kolor
                SelectedColor = Color.FromRgb(red, green, blue);



                // Zapisz wartości do Properties
                Application.Current.Properties["Red"] = red.ToString();
                Application.Current.Properties["Green"] = green.ToString();
                Application.Current.Properties["Blue"] = blue.ToString();

                // Zamknij okno
                this.Close();
            }
            
        }
        private void RGBtoHSV(byte red, byte green, byte blue, out double hue, out double saturation, out double value)
        {
            double r = red / 255.0;
            double g = green / 255.0;
            double b = blue / 255.0;

            double min = Math.Min(Math.Min(r, g), b);
            double max = Math.Max(Math.Max(r, g), b);
            double delta = max - min;

            // Value
            value = max;

            // Saturation
            saturation = (max == 0) ? 0 : delta / max;

            // Hue
            hue = 0;

            if (delta != 0)
            {
                if (max == r)
                    hue = ((g - b) / delta) % 6;
                else if (max == g)
                    hue = ((b - r) / delta) + 2;
                else
                    hue = ((r - g) / delta) + 4;
            }

            hue *= 60;

            if (hue < 0)
                hue += 360;
        }

        // ... reszta kodu ...

        // Aktualizacja wartości HSV na interfejsie użytkownika
        private void UpdateHSVValues()
        {
            RGBtoHSV(SelectedColor.R, SelectedColor.G, SelectedColor.B, out double hue, out double saturation, out double value);

            HueTextBox.Text = Math.Round(hue).ToString();
            SaturationTextBox.Text = Math.Round(saturation * 100).ToString();
            ValueTextBox.Text = Math.Round(value * 100).ToString();
        }

        private void ApplyButton_Convert(object sender, RoutedEventArgs e)
        {
            if (ValidateRGBInputs())
            {
                // Pobierz wartości RGB z TextBox-ów
                byte red = Convert.ToByte(RedTextBox.Text);
                byte green = Convert.ToByte(GreenTextBox.Text);
                byte blue = Convert.ToByte(BlueTextBox.Text);

                // Ustaw wybrany kolor
                SelectedColor = Color.FromRgb(red, green, blue);
                UpdateHSVValues();
            }
        }

        private bool ValidateRGBInputs()
        {
            return ValidateInput(RedTextBox) && ValidateInput(GreenTextBox) && ValidateInput(BlueTextBox);
        }


        private bool ValidateInput(TextBox textBox)
        {
            if (byte.TryParse(textBox.Text, out byte result) && result >= 0 && result <= 255)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Wprowadzona wartość musi być liczbą całkowitą z przedziału 0-255.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                textBox.Clear();
                textBox.Focus();
                return false;
            }
        }
    }
}
