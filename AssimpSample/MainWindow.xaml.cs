using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using SharpGL.SceneGraph;
using SharpGL;
using Microsoft.Win32;


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
                m_world = new World(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3D Models\\Duck"), "camera.3ds", (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
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
            if (e.Key != Key.M && m_world.StartAnimation == true)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.F6: this.Close(); break;
                case Key.Up: if (m_world.RotationX <= 35) m_world.RotationX += 7.0f; break;
                case Key.Down: if (m_world.RotationX >= 35 || m_world.RotationX >= -21) m_world.RotationX -= 7.0f; break;
                case Key.Right: if (m_world.RotationY >= 0) m_world.RotationY -= 7.0f; break;
                case Key.Left: if (m_world.RotationY <= 56) m_world.RotationY += 7.0f; break;
                case Key.Add: m_world.SceneDistance += 700.0f; break;
                case Key.Subtract: m_world.SceneDistance -= 700.0f; break;
                case Key.M:
                    if (m_world.StartAnimation)
                        m_world.StartAnimation = false;
                    else
                    {
                        m_world.StartAnimation = true;
                    }
                    cageHeightSlider.IsEnabled = !m_world.StartAnimation;
                    cameraRotationSpeedSlider.IsEnabled = !m_world.StartAnimation;
                    cameraScaleSlider.IsEnabled = !m_world.StartAnimation;
                    break;

                case Key.F2:
                    OpenFileDialog opfModel = new OpenFileDialog();
                    bool result = (bool) opfModel.ShowDialog();
                    if (result)
                    {

                        try
                        {
                            World newWorld = new World(Directory.GetParent(opfModel.FileName).ToString(), Path.GetFileName(opfModel.FileName), (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);
                            m_world.Dispose();
                            m_world = newWorld;
                            m_world.Initialize(openGLControl.OpenGL);
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta:\n" + exp.Message, "GRESKA", MessageBoxButton.OK );
                        }
                    }
                    break;
            }
        }

        private void HeightOfCageSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                float number = (float)cageHeightSlider.Value;

                if (number < 280.0f)
                {
                    return;
                }
                else
                {
                    m_world.visinaKaveza = number;
                }
            }
        }

        private void CameraScale(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                float number = (float)cameraScaleSlider.Value;

                if (number < 0.05f)
                {
                    return;
                }
                else
                {
                    m_world.visinaKamere = number;
                    m_world.sirinaKamere = number;
                }
            }
        }

        private void cameraSpeedSlider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (m_world != null)
            {
                m_world.speedRotation = (int)cameraRotationSpeedSlider.Value;
            }
        }
    }
}
