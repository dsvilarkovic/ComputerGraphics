using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL.SceneGraph;
using SharpGL;
using System.Reflection;
using System.Runtime.InteropServices;
using static PF1S1._2.World;

namespace PF1S1._2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi

        //TODO obrisati posle
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
        private const int BORDER = 10;

        /// <summary>
        ///  Pamti staru poziciju kursora da bi mogli da racunamo pomeraj.
        /// </summary>
        private Point oldPos;
        //TODO kraj brisanja

        /// <summary>
        ///  Instanca OpenGL "sveta" - klase koja je zaduzena za iscrtavanje koriscenjem OpenGL-a.
        /// </summary>
        World m_world = null;
        

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // Kreiranje OpenGL sveta
            try
            {
                //TODO 3: nadje taj svet i da ga ubaci u bafer i iscrta
                //TODO 4: pre ovog poziva u 3d modelima se misli da se u debug najde ili release folder iskompajliranom

                String putanja_kamiona = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3DModel\\kamion");
                String putanja_kontejnera = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "3DModel\\kontejner");
                

                m_world = new World(new[] { putanja_kamiona, putanja_kontejnera}, new []{ "toy_truck.obj", "container.obj" }, (int)openGLControl.Width, (int)openGLControl.Height, openGLControl.OpenGL);

                this.DataContext = m_world;
            }
            catch (Exception e)
            {
                MessageBox.Show("Neuspesno kreirana instanca OpenGL sveta. Poruka greške: " + e.Message, "Poruka", MessageBoxButton.OK);
                this.Close();
            }
        }

        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //Iscrtaj svet
            m_world.Draw(args.OpenGL);
        }

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //m_world.Initialize(args.OpenGL);

            
            SetCursorPos((int)(this.Left + this.Width / 2), (int)(this.Top + this.Height / 2));
            m_world.Initialize(args.OpenGL);
        }

        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            m_world.Resize(args.OpenGL, (int)openGLControl.ActualWidth, (int)openGLControl.ActualHeight);
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (LookAt.IsChecked == false)
            {
                switch (e.Key)
                {


                    case System.Windows.Input.Key.F4: this.Close(); break;

                    case System.Windows.Input.Key.W: m_world.RotationX -= 5.0f; break;
                    case System.Windows.Input.Key.S: m_world.RotationX += 5.0f; break;
                    case System.Windows.Input.Key.A: m_world.RotationY -= 5.0f; break;
                    case System.Windows.Input.Key.D: m_world.RotationY += 5.0f; break;



                    case System.Windows.Input.Key.Down: m_world.TranslationY -= 1.0f; break;

                    case System.Windows.Input.Key.Up: m_world.TranslationY += 1.0f; break;
                    case System.Windows.Input.Key.Left: m_world.TranslationX -= 1.0f; break;
                    case System.Windows.Input.Key.Right: m_world.TranslationX += 1.0f; break;

                    case System.Windows.Input.Key.OemPlus: m_world.TranslationZ -= 1.0f; break;
                    case System.Windows.Input.Key.OemMinus: m_world.TranslationZ += 1.0f; break;

                }

            }
            else
            {
                switch (e.Key)
                {
                    case Key.W: m_world.UpdateCameraPosition(0, 0, 1); break;
                    case Key.S: m_world.UpdateCameraPosition(0, 0, -1); break;
                    case Key.A: m_world.UpdateCameraPosition(-1, 0, 0); break;
                    case Key.D: m_world.UpdateCameraPosition(1, 0, 0); break;
                    case Key.Q: m_world.UpdateCameraPosition(0, 1, 0); break;
                    case Key.E: m_world.UpdateCameraPosition(0, -1, 0); break;
                }
            }

            //prekidac za pokretanje animacije
            if (e.Key.Equals(Key.V))
            {
                m_world.IsAnimationStarted = !m_world.IsAnimationStarted;
            }
        }

        private void depthCheckBox_Click(object sender, RoutedEventArgs e)
        {
            m_world.DepthTesting = (bool)depthCheckBox.IsChecked;
        }

        private void cullingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            m_world.Culling = (bool)cullingCheckBox.IsChecked;
        }

        private void outlineCheckBox_Click(object sender, RoutedEventArgs e)
        {
            m_world.Outline = (bool)outlineCheckBox.IsChecked;
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

            if (LookAt.IsChecked.Value) {
                bool outOfBoundsX = false;
                bool outOfBoundsY = false;
                Point point = e.GetPosition(this);
            
                if (point.Y <= BORDER || point.Y >= this.Height - BORDER)
                {
                    outOfBoundsY = true;
                }
                if (point.X <= BORDER || point.X >= this.Width - BORDER)
                {
                    outOfBoundsX = true;
                }

                if (ToolBarTray.IsMouseOver)
                {
                    outOfBoundsY = false;
                    outOfBoundsX = false;

                    return;
                }
                if (!outOfBoundsY && !outOfBoundsX)
                {
                    double deltaX = oldPos.X - point.X;
                    double deltaY = oldPos.Y - point.Y;
                    m_world.UpdateCameraRotation(deltaX, deltaY);
                    oldPos = point;
                }
                else
                {
                    if (outOfBoundsX)
                    {
                        SetCursorPos((int)this.Left + (int)this.Width / 2, (int)this.Top + (int)point.Y);
                        oldPos.X = this.Width / 2;
                        oldPos.Y = point.Y;
                    }
                    else
                    {
                        SetCursorPos((int)this.Left + (int)point.X, (int)this.Top + (int)this.Height / 2);
                        oldPos.Y = this.Height / 2;
                        oldPos.X = point.X;
                    }
                }
            }
        }

        
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.BanderaHeight = (float) Bandera.Value;
                                                 //MessageBox.Show(Bandera.Value.ToString());
            }
            catch(NullReferenceException nre)
            {
                //MessageBox.Show("Izuzetak");
            }
        }

        private void KontejnerSkala_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.YetAnotherKontejnerScale = (float)KontejnerSkala.Value;
            }
            catch(NullReferenceException nre)
            {

            }
        }

        private void RedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.RedValueReflector = (float)RedSlider.Value;
            }
            catch (NullReferenceException nre)
            {

            }
        }

        private void GreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.GreenValueReflector = (float)GreenSlider.Value;
            }
            catch (NullReferenceException nre)
            {

            }

        }

        private void BlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.BlueValueReflector = (float)BlueSlider.Value;
            }
            catch (NullReferenceException nre)
            {

            }
        }

        private void shadingMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_world.SelectedShadingMode = (ShadingMode) shadingMode.SelectedIndex;
            openGLControl.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            shadingMode.ItemsSource = Enum.GetValues(typeof(ShadingMode));
        }

        private void PodlogaTexCoordX_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.PodlogaTexCoord_x = (float) PodlogaTexCoordX.Value;
            }
            catch (NullReferenceException)
            {

            }
        }

        private void PodlogaTexCoordY_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                m_world.PodlogaTexCoord_y = (float) PodlogaTexCoordY.Value;
            }
            catch (NullReferenceException)
            {

            }
        }
             
        
        

        private void LookAt_Click(object sender, RoutedEventArgs e)
        {
            m_world.isLookAtCameraEnabled = (bool)LookAt.IsChecked.Value;
        }

        private void gluLookAt_Click_1(object sender, RoutedEventArgs e)
        {
            m_world.IsGluLookAtCameraEnabled = (bool)gluLookAt.IsChecked.Value;
        }

        private void Regular_Click(object sender, RoutedEventArgs e)
        {
            m_world.isLookAtCameraEnabled = false;
            m_world.IsGluLookAtCameraEnabled = false;
        }
    }
}
