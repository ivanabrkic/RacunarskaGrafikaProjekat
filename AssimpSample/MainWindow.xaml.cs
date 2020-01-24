using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using SharpGL.SceneGraph;
using Microsoft.Win32;
using System.Windows.Controls;

namespace AssimpSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        /// <summary>
        ///	 Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;

        #endregion Atributi

        #region Konstruktori

        public MainWindow()
        {
            // Inicijalizacija komponenti
            InitializeComponent();

            // Kreiranje OpenGL sveta
            try
            {
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\PlateCandle"), "DecorativePlate.obj", "13496_Table_Candle_v1_L3.obj", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        #endregion Konstruktori

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.Width, (int)openGLControl.Height);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!m_world.Animation)
            {
                switch (e.Key)
                {
                    case Key.F5: this.Close(); break;
                    case Key.T:
                        if (m_world.RotationX <= 40.0f)
                        {
                            m_world.RotationX += 5.0f;
                        }
                        break;
                    case Key.G:
                        if (m_world.RotationX >= 15.0f)
                        {
                            m_world.RotationX -= 5.0f;
                        }
                        break;
                    case Key.F:
                        m_world.RotationY -= 5.0f;
                        break;
                    case Key.H:
                        m_world.RotationY += 5.0f;
                        break;
                    case Key.A:
                        if (Rotacija.SelectedItem.Equals("DA"))
                        {
                            m_world.RotationYCandlePlate -= 5.0f;
                        }
                        break;
                    case Key.D:
                        if (Rotacija.Text.Equals("DA"))
                        {
                            m_world.RotationYCandlePlate += 5.0f;
                        }
                        break;
                    case Key.Add:
                        if (m_world.SceneDistance <= 1500.0f)
                        {
                            m_world.SceneDistance += 500.0f;
                        }
                        break;
                    case Key.Subtract:
                        m_world.SceneDistance -= 500.0f; break;

                    case Key.C: m_world.BeginAnimation(); break;
                }
            }
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void ComboBox_SelectionChanged_1(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem boja = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (Boja.IsInitialized)
            {
                if (boja.Content.Equals("CRVENA"))
                {
                    m_world.Red = 1.0f;
                    m_world.Green = 0.0f;
                    m_world.Blue = 0.0f;
                }
                else if (boja.Content.Equals("ZELENA"))
                {
                    m_world.Red = 0.0f;
                    m_world.Green = 1.0f;
                    m_world.Blue = 0.0f;
                }
                else
                {
                    m_world.Red = 0.0f;
                    m_world.Green = 0.0f;
                    m_world.Blue = 1.0f;
                }
            }
        }

        private void ComboBox_SelectionChanged_2(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBoxItem scale = (sender as ComboBox).SelectedItem as ComboBoxItem;
            if (Skala.IsInitialized)
            {
                switch (scale.Content)
                {
                    case "0.5":
                        m_world.Scale = 0.5f;
                        break;
                    case "2":
                        m_world.Scale = 2f;
                        break;
                    case "5":
                        m_world.Scale = 5f;
                        break;
                    case "10":
                        m_world.Scale = 10f;
                        break;
                    case "15":
                        m_world.Scale = 15f;
                        break;
                    case "20":
                        m_world.Scale = 20f;
                        break;
                }
            }
        }
    }
}
