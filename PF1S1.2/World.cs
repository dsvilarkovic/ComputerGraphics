using System;
using System.Collections;
using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.Enumerations;
using SharpGL.SceneGraph.Core;
using System.Diagnostics;
using SharpGL.SceneGraph;
using AssimpSample;
using SharpGL.SceneGraph.Cameras;
using System.Drawing;
using System.Windows;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace PF1S1._2
{
    ///<summary> Klasa koja enkapsulira OpenGL programski kod </summary>
    class World
    {
        #region Atributi

        private String[] tekstProjekta = {"Predmet: Racunarska grafika","Sk.god: 2018/19","Ime: Dusan","Prezime: Svilarkovic","Sifra zad: 1.2"};

        private DisplayList podlogaDisplayList = new DisplayList();
        private uint podlogaID = 0;

        private DispatcherTimer timer1;

        private bool isAnimationStarted = false;

        /// <summary>
        ///	 Izabrani tip teselacije.
        /// </summary>
        private ShadingMode m_selectedShadingMode;

        private double yetAnotherKontejnerScale = 1.0f;


        private static readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        /// <summary>
        ///  Identifikator teksture
        /// </summary>
        private uint[] textureIDs = new uint[m_textureCount];



        private String[] m_textureFiles = {"textures\\asphalt.jpg", "textures\\bricks.jpg", "textures\\metal.jpg" };


        private enum TextureObjects { Asphalt = 0, Bricks, Metal};



        private float podlogaTexCoord_x = 1.0f;
        private float podlogaTexCoord_y = 1.0f;
        /// <summary>
        /// Indikator da li je lookAtCamera podesena 
        /// </summary>
        public bool isLookAtCameraEnabled = false;


        private OpenGL gl;

        /// <summary>
        ///  Indikator stanja mehanizma sakrivanja nevidljivih povrsina.
        /// </summary>
        bool m_culling = false;

        /// <summary>
        ///  Indikator stanja mehanizma iscrtvanja poligona kao linija.
        /// </summary>
        bool m_outline = false;

        /// <summary>
        ///  Indikator stanja mehanizma za testiranje dubine.
        /// </summary>
        bool m_depthTesting = true;


        //TODO obrisati kasnije
        // Atributi koji uticu na ponasanje FPS kamere
        private LookAtCamera lookAtCam;
        private float walkSpeed = 0.1f;
        float mouseSpeed = 0.005f;
        double horizontalAngle = 0f;
        double verticalAngle = 0.0f;

        //Pomocni vektori preko kojih definisemo lookAt funkciju
        private Vertex direction;
        private Vertex right;
        private Vertex up;


        //TODO KRAJ BRISANJA
        private AssimpScene kamion_scene;
        private AssimpScene kontejner_scene;

        private float m_xTranslation = 0.0f;
        private float m_yTranslation = 0.0f;
        private float m_zTranslation = 0.0f;
        private const float POLU_DUZINA = 5f;
        /// <summary>
        ///	 Ugao rotacije sveta oko X ose
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width = 0;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height = 0;

        /// <summary>
        ///	 Mreza za iscrtavanje
        /// </summary>
        Grid grid;
        private float bandera_height = 3.0f;


        private float red_value_reflector = 1.0f;
        private float green_value_reflector = 0.0f;
        private float blue_value_reflector = 0.0f;

        /// <summary>
        ///  Nabrojani tip OpenGL podrzanih tipova sencenja
        /// </summary>
        public enum ShadingMode
        {
            Flat,
            Smooth
        };

        #endregion

        #region Properties

        /// <summary>
        ///	 Izabrani tip sencenja.
        /// </summary>
        public ShadingMode SelectedShadingMode
        {
            get { return m_selectedShadingMode; }
            set
            {
                m_selectedShadingMode = value;
            }
        }



        public float BanderaHeight
        {
            get
            {
                return bandera_height;
            }
            set
            {
                bandera_height = value;
            }
        }
        /// <summary>
        ///  Indikator stanja mehanizma sakrivanja nevidljivih povrsina.
        /// </summary>
        public bool Culling
        {
            get { return m_culling; }
            set { m_culling = value; }
        }

        /// <summary>
        ///  Indikator stanja mehanizma iscrtvanja poligona kao linija.
        /// </summary>
        public bool Outline
        {
            get { return m_outline; }
            set { m_outline = value; }
        }

        /// <summary>
        ///  Indikator stanja mehanizma za testiranje dubine.
        /// </summary>
        public bool DepthTesting
        {
            get { return m_depthTesting; }
            set { m_depthTesting = value; }
        }


        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Translacije sveta oko X ose.
        /// </summary>
        public float TranslationX
        {
            get { return m_xTranslation; }
            set { m_xTranslation = value; }
        }

        /// <summary>
        ///	 Translacije sveta oko Y ose.
        /// </summary>
        public float TranslationY
        {
            get { return m_yTranslation; }
            set { m_yTranslation = value; }
        }

        /// <summary>
        ///	 Translacije sveta oko Z ose.
        /// </summary>
        public float TranslationZ
        {
            get { return m_zTranslation; }
            set { m_zTranslation = value; }
        }

        public double YetAnotherKontejnerScale
        {
            get
            {
                return yetAnotherKontejnerScale;
            }
            set
            {
                yetAnotherKontejnerScale = value;
            }
        }

        public float RedValueReflector
        {
            get
            {
                return red_value_reflector;
            }
            set
            {
                red_value_reflector = value;
            }
        }
        public float GreenValueReflector
        {
            get
            {
                return green_value_reflector;
            }
            set
            {
                green_value_reflector = value;
            }
        }
        public float BlueValueReflector
        {
            get
            {
                return blue_value_reflector;
            }
            set
            {
                blue_value_reflector = value;
            }
        }

        public bool IsGluLookAtCameraEnabled
        {
            get
            {
                return isGluLookAtCameraEnabled;
            }

            set
            {
                isGluLookAtCameraEnabled = value;
            }
        }

        public float PodlogaTexCoord_y
        {
            get
            {
                return podlogaTexCoord_y;
            }
            set
            {
                podlogaTexCoord_y = value;
            }
        }
        public float PodlogaTexCoord_x
        {
            get
            {
                return podlogaTexCoord_x;
            }

            set
            {
                podlogaTexCoord_x = value;
            }
        }

        public bool IsAnimationStarted { get => isAnimationStarted; set => isAnimationStarted = value; }
        public float X_translateDebug { get; internal set; }
        public float Z_translateDebug { get; internal set; }
        public float Y_translateDebug { get; internal set; }
        public float Z_scaleDebug { get; internal set; }
        public float Y_scaleDebug { get; internal set; }
        public float X_scaleDebug { get; internal set; }

        #endregion

        #region Konstruktori

        /// <summary>
        ///		Konstruktor opengl sveta.
        /// </summary>
        public World()
        {

        }

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String[] scenePath, String[] sceneFileName, int width, int height, OpenGL gl)
        {
            this.kamion_scene = new AssimpScene(scenePath[0], sceneFileName[0], gl);
            this.kontejner_scene = new AssimpScene(scenePath[1], sceneFileName[1], gl);
            this.m_width = width;
            this.m_height = height;
        }

        #endregion

        #region Metode

        /// <summary>
        /// Korisnicka inicijalizacija i podesavanje OpenGL parametara
        /// </summary>
        public void Initialize(OpenGL gl)
        {

            this.gl = gl;
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);

            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);


            SetUpLighting(gl);
        
            kamion_scene.LoadScene();
            kamion_scene.Initialize();

            kontejner_scene.LoadScene();
            kontejner_scene.Initialize();

            if(isLookAtCameraEnabled)
                SetupCamera(gl);

            SetupTexture(gl);
            
            gl.ClearColor(0f, 0f, 0f, 1.0f);


            //podesavanje animacije
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(20);
            timer1.Tick += new EventHandler(UpdateKamionPosition);
            timer1.Start();
        }


        private float kamion_up = 0.0f;
        private float kamion_rotate_right = 0.0f;
        private float kamion_right = 0.0f;
        private float brojac_kamion = 1.0f;
        private float vrata_rotate = 0.0f;

        private enum KamionState
        {
            NAPRED, DESNO_DO_STOJ, STOJ, DESNO_OD_STOJ, NAZAD, OTVORI_VRATA, ZATVORI_VRATA
        }

        private KamionState currentKamionState = KamionState.NAPRED;
        private void UpdateKamionPosition(object sender, EventArgs e)
        {
            if (isAnimationStarted)
            {

                switch (currentKamionState)
                {
                    case (KamionState.NAPRED):
                        {
                            //idem napred
                            kamion_up += 0.1f;
                            if (kamion_up > POLU_DUZINA + (YetAnotherKontejnerScale * (POLU_DUZINA / 4)))
                            {
                                currentKamionState = KamionState.DESNO_DO_STOJ;
                            }
                            break;
                        }
                    case (KamionState.DESNO_DO_STOJ):
                        {
                            kamion_rotate_right = 90f;
                            //kamion_up = 0;
                            kamion_right += 0.1f;

                            double predjeni_put_desno = YetAnotherKontejnerScale * (POLU_DUZINA / 2);

                            
                            if (kamion_right > predjeni_put_desno / 2)
                            {
                                currentKamionState = KamionState.STOJ;
                            }

                            break;
                        }
                    case (KamionState.STOJ):
                        {
                            brojac_kamion -= 0.1f;
                            if (brojac_kamion < 0)
                            {
                                currentKamionState = KamionState.OTVORI_VRATA;
                            }
                            break;
                        }

                    case (KamionState.OTVORI_VRATA):
                        {
                            vrata_rotate -= 1.0f;
                            if (vrata_rotate < -45.0f)
                            {
                                currentKamionState = KamionState.ZATVORI_VRATA;
                            }
                            break;
                        }
                    case (KamionState.ZATVORI_VRATA):
                        {
                            vrata_rotate += 1.0f;
                            if (vrata_rotate > 0.0f)
                            {
                                currentKamionState = KamionState.DESNO_OD_STOJ;
                            }
                            break;
                        }
                    case (KamionState.DESNO_OD_STOJ):
                        {
                            kamion_rotate_right = 90f;
                            //kamion_up = 0;
                            kamion_right += 0.1f;

                            double predjeni_put_desno = YetAnotherKontejnerScale * (POLU_DUZINA / 2);

                            if (kamion_right > predjeni_put_desno)
                            {
                                currentKamionState = KamionState.NAZAD;
                                return;
                            }


                            break;
                        }
                    case (KamionState.NAZAD):
                        {
                            kamion_rotate_right = 180f;
                            kamion_up -= 0.1f;
                            //kamion_right = 0.0f;
                            if (kamion_up < 0)
                            {
                                currentKamionState = KamionState.NAPRED;
                                kamion_up = 0f;
                                kamion_rotate_right = 0f;
                                kamion_right = 0f;
                                vrata_rotate = 0f;
                                brojac_kamion = 1.0f;
                            }
                            break;
                        }
                }

            }
            else
            {
                currentKamionState = KamionState.NAPRED;
                kamion_up = 0f;
                kamion_rotate_right = 0f;
                kamion_right = 0f;
                vrata_rotate = 0f;
                brojac_kamion = 1.0f;
            }
        }

        private void SetupTexture(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            /*
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            */

            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            // Ucitaj slike i izgenerisi teksture
            gl.GenTextures(m_textureCount, textureIDs);

            for (int i = 0; i < m_textureCount; i++)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru                 
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIDs[i]);


                String currentDirectory = System.IO.Directory.GetCurrentDirectory();

                String filePath = System.IO.Path.Combine(currentDirectory, m_textureFiles[i]);
                // Ucitaj sliku i podesi parametre teksture                 
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a            
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)        
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb); 

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);

                //ponavljanje po R i S kordinatama
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_R, OpenGL.GL_REPEAT);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
                //filteri mipmap linerano filtriranje
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);

                image.UnlockBits(imageData);
                image.Dispose();
            }
            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }

        private void SetUpLighting(OpenGL gl)
        {
            float[] global_ambient = new float[] { 1f, 1f, 1f, 1f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);

            //TODO 1B: Podesavanje crvenog tackastog izvora
            float[] light0pos = new float[] { 0.0f, BanderaHeight, (float)-(2 * POLU_DUZINA), 1.0f };
            float[] ambijentalnaKomponenta = { 1.0f, 0.0f, 0.0f, 1.0f };
            float[] difuznaKomponenta = { 1.0f, 0.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta);


            //TODO 2B: Podesavanje crvenog reflektorskog 35deg izvora sa podesivom ambijentalnom komponentom
            float[] light1pos = new float[] { POLU_DUZINA/2, 2.0f, (float)-(2 * POLU_DUZINA), 1.0f };
            float[] ambijentalnaKomponenta1 = { 1.0f, 0.0f, 0.0f, 1.0f };
            float[] difuznaKomponenta1 = { 1.0f, 0.0f, 0.0f, 1.0f };
            float[] light1direction = {0.0f, -1.0f,0.0f,1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuznaKomponenta1);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, light1direction);
            
            //gl.Enable(OpenGL.GL_LIGHTING);


            //gl.Enable(OpenGL.GL_LIGHT0);             
            //gl.Enable(OpenGL.GL_LIGHT1);


            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        private void SetUpBanderaLightingHeight(OpenGL gl)
        {
            /*
            gl.PushMatrix();
            gl.Translate(0, BanderaHeight, 0);
            gl.Scale(0.3f, 0.3f, 0.3f);
            Cube cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            */
            float[] light0pos = new float[] { 0.0f, BanderaHeight, (float)-(2 * POLU_DUZINA), 1.0f };
            //float[] light0pos = new float[] { 0.0f, BanderaHeight, 0, 1.0f };
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
        }


        private void SetUpReflectorColorLighting(OpenGL gl)
        {
            /*
            gl.PushMatrix();
            gl.Translate(POLU_DUZINA / 2, 2.0f, (float)-(2 * POLU_DUZINA));
            gl.Scale(0.3f, 0.3f, 0.3f);
            Cube cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            */

            //float[] light1pos = new float[] { POLU_DUZINA / 2, 2.0f, (float)-(2 * POLU_DUZINA), 1.0f };
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light1pos);
            float[] ambijentalnaKomponenta1 = { RedValueReflector, GreenValueReflector, BlueValueReflector, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta1);
        }
        private void SetupCamera(OpenGL gl)
        {
            if (lookAtCam == null)
            {
                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();
                lookAtCam = new LookAtCamera();
                lookAtCam.Position = new Vertex(0f, 0f, -2 * POLU_DUZINA);
                lookAtCam.Target = new Vertex(0f, 0f, -10f);
                lookAtCam.UpVector = new Vertex(0f, 1f, 0f);
                right = new Vertex(1f, 0f, 0f);
                direction = new Vertex(0f, 0f, -1f);
                lookAtCam.Target = lookAtCam.Position + direction;
                lookAtCam.Project(gl);
            }
        }


        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            //Definisati projekciju u perspektivi (fov=45, near=1, a vrednost far zadati po potrebi) 
            //i viewport-om preko celog prozora unutar Resize metode.
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            
            gl.Perspective(45f, (double)width / height, 1f, 100f);
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);



            //inicijalizacija displayliste
            podlogaID = gl.GenLists(1);
            gl.NewList(podlogaID, OpenGL.GL_COMPILE);
            
            gl.EndList();
        }

        
        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();


            setShadingMode(gl);
            setCulling(gl);
            setDepthTesting(gl);
            setOutline(gl);
            

            float[] eyeVector = { 2f, 0.5f, -2*POLU_DUZINA - POLU_DUZINA/4 };
            float[] centerVector = {-100,0,0 };
            float[] upVector = { 0, 1, 0 };


            if (isGluLookAtCameraEnabled == true)
            {
                gl.LookAt(eyeVector[0], eyeVector[1], eyeVector[2],
                    centerVector[0], centerVector[1], centerVector[2],
                    upVector[0], upVector[1], upVector[2]);
            }


            
            SetUpReflectorColorLighting(gl);


                     
            gl.PushMatrix();
                gl.Translate(0f, 0f, (float)-(2*POLU_DUZINA));
                
                MoveAround(gl);
                SetUpBanderaLightingHeight(gl);                
                DrawKontejner(gl);          
                gl.CallList(podlogaID);            
                DrawBandera(gl);
                DrawPodloga(gl);
                DrawKamion(gl);

            gl.PopMatrix();     
            


            //3. Ispisati 2D tekst žutom bojom u donjem desnom uglu prozora 
            //(redefinisati viewport korišćenjem glViewport metode).
            //Font je Arial, 14pt, underline. Tekst treba biti oblika:            
            //DrawProjectInfo(gl);
            
            gl.Flush();
        }
        

        #region draw_nesto
        private double scaleKontejner = 0.6* 2 * 10e-3;
        private bool isGluLookAtCameraEnabled;

        private void DrawKontejner(OpenGL gl)
        {
            gl.PushMatrix();
                gl.Translate(POLU_DUZINA / 2, 0.2f, 0f);
                gl.Rotate(180.0f, 0, 1, 0);
                //skaliranje kontejnera po wpf kontroli
                gl.Scale(YetAnotherKontejnerScale, YetAnotherKontejnerScale, YetAnotherKontejnerScale);


                gl.PushMatrix();
                    gl.Scale(scaleKontejner, scaleKontejner, scaleKontejner);
                   
                    kontejner_scene.Draw();
                gl.PopMatrix();

                DrawZid(gl);
            gl.PopMatrix();

            
        }

        private void DrawZid(OpenGL gl)
        {
            
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIDs[(int)TextureObjects.Bricks]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            //vrati skaliranje
            Cube cube = new Cube();
            
            //zid iza kontejnera
            gl.PushMatrix();
            gl.Translate(0.4f, 0.3f, 0f);
            gl.Scale(0.05f, 0.3f, 0.5f);            
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();


            //zid blizi nama
            gl.PushMatrix();
            gl.Translate(0f, 0.3f, 0f);
            gl.Rotate(90f, 0f, 1f, 0f);
            gl.Translate(-0.5f, 0f, 0f);
            gl.Scale(0.05f, 0.3f, 0.4f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            //zid dalji od nas
            gl.PushMatrix();
            gl.Translate(0f, 0.3f, 0f);
            gl.Rotate(90f, 0f, 1f, 0f);
            gl.Translate(0.5f, 0f, 0f);
            gl.Scale(0.05f, 0.3f, 0.4f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            gl.PopMatrix();
           
        }

        private void DrawKamion(OpenGL gl)
        {
            gl.PushMatrix();

            gl.Translate(-POLU_DUZINA / 2, 0.1f, (-POLU_DUZINA/4)* YetAnotherKontejnerScale);
            gl.Translate(kamion_up, 0, kamion_right);
            gl.Rotate(0, -kamion_rotate_right, 0);


            gl.PushMatrix();
            float scale_factor = 0.5f * 10e-2f;
            gl.Scale(scale_factor, scale_factor, scale_factor);
            gl.Rotate(-90, 0, 0);
            kamion_scene.Draw();
            gl.PopMatrix();
            DrawVrata(gl);

            gl.PopMatrix();
        }

        private void DrawVrata(OpenGL gl)
        {
            
            gl.PushMatrix();

            gl.PushMatrix();
            gl.Rotate(vrata_rotate, 0, 0);
            gl.Scale(0.469f, 0.163f, 0.142f);
            gl.Translate(-0.816f, 4.489f, 3.265f);

            
            Cube cube = new Cube();
            //gl.Color(1f, 1f, 0f);
            cube.Render(gl, RenderMode.Render);

            gl.PopMatrix();

            gl.Rotate(vrata_rotate, 0, 0);
            gl.Translate(-0.4, 0.82, 0);

            gl.PushMatrix();
            gl.Scale(0.51f, 0.08f, 0.387f);
            
            
            Cube cubeTop = new Cube();
            cubeTop.Render(gl, RenderMode.Render);

            gl.PopMatrix();


            gl.PopMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
            //gl.Color(1f, 1f, 1f);
        }
        private void DrawBandera(OpenGL gl)
        {

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIDs[(int)TextureObjects.Metal]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.PushMatrix();
            

            gl.Translate(0f, 0.1f, 0f);
            gl.Rotate(-90f, 1f, 0f, 0f);
            Cylinder cylinder = new Cylinder();
            cylinder.TopRadius = 0.05f;
            cylinder.BaseRadius = cylinder.TopRadius;
            cylinder.Height = BanderaHeight;//3f;
            cylinder.CreateInContext(gl);
            cylinder.Render(gl, RenderMode.Render);
            gl.PopMatrix();


            //kocka kao svetiljka na banderi
            gl.PushMatrix();
            gl.Translate(0f, cylinder.Height, 0f);
            gl.Rotate(-90f, 1f, 0f, 0f);
            gl.Scale(0.5, 0.1, 0.1);
            Cube cube = new Cube();

            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);
        }
        
        
        private void DrawPodloga(OpenGL gl)
        {
            //Podlozi pridružiti teksturu asfalta (slika koja se koristi za teksturu je jedan segmenat asfalta).
            //Pritom obavezno skalirati teksturu. Skalirati teksturu (broj ponavljanja teksture) shodno potrebi.
            //Skalirati teksturu korišćenjem Texture matrice. Definisati koordinate teksture.

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textureIDs[(int)TextureObjects.Asphalt]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.Scale(PodlogaTexCoord_x, PodlogaTexCoord_y, 1f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Begin(OpenGL.GL_QUADS);
                float duzina = POLU_DUZINA*  (float) YetAnotherKontejnerScale;
                gl.Normal(LightingUtilities.
                    FindFaceNormal(-duzina, 0.1f, -duzina,
                                   -duzina, 0.1f, duzina,
                                   duzina, 0.1f, duzina));




                //gl.TexCoord(0.0f, 0.0f);
                gl.TexCoord(0.0f, 0.0f);
                gl.Vertex(-duzina, 0.1f, -duzina);


                //gl.TexCoord(0.0f, PodlogaTexCoord_y);
                gl.TexCoord(0.0f, 1.0f);
                gl.Vertex(-duzina, 0.1f, duzina);

            
                //gl.TexCoord(PodlogaTexCoord_x, PodlogaTexCoord_y);
                gl.TexCoord(1.0f, 1.0f);
                gl.Vertex( duzina, 0.1f, duzina);

                //gl.TexCoord(PodlogaTexCoord_x, 0.0f);
                gl.TexCoord(1.0f, 0.0f);
                gl.Vertex( duzina, 0.1f, -duzina); 
            gl.End();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
        }
        /// <summary>
        ///  Iscrtavanje SharpGL primitive grida.
        /// </summary>
        private void DrawGrid(OpenGL gl)
        {
            gl.PushMatrix();
            Grid grid = new Grid();
            gl.Translate(0f, -1f, -2*POLU_DUZINA);
            gl.Rotate(90f, 0f, 0f);
            grid.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Design);
            gl.PopMatrix();
        }

        public void DrawProjectInfo(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Viewport(m_width /2, 0, m_width / 2, m_height / 2);
            int korak = m_width / 17;
            for (int i = 0; i < tekstProjekta.Length; i++)
            {
                int x = m_width / 4 - 30;
                int y = 12 + korak * i; //Convert.ToInt32(m_height * 0.05 + korak * i);
                
                gl.DrawText(x, y, 1.0f, 1.0f, 0.0f, "Arial", 14, tekstProjekta[i]);
                
                string underline = new string('_', (tekstProjekta[i]).Length);
                gl.DrawText(x, y, 1.0f, 1.0f, 0.0f, "Arial", 13, underline);

            }
            

            gl.Viewport(0, 0, m_width, m_height);
            gl.PopMatrix();
        }

        #endregion draw_nesto
        


        #region pomeraj_kamere
        private void MoveAround(OpenGL gl)
        {
            //kontrole tastature
            if(isGluLookAtCameraEnabled == true)
            {
                if (m_xRotation > 0)
                {
                    m_xRotation = 0;
                }
                if (m_xRotation < -90)
                {
                    m_xRotation = -90;
                }
               
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
                gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
                gl.Translate(0f, 0f, m_zTranslation);
            }
            else if (isLookAtCameraEnabled == false)
            {
                gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
                gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);
                gl.Translate(m_xTranslation, 0f, 0f);
                gl.Translate(0f, m_yTranslation, 0f);
                gl.Translate(0f, 0f, m_zTranslation);
            }
            else
            {
                SetupCamera(gl);
                lookAtCam.Project(gl);
            }
        }

        /// <summary>
        ///  Azurira rotaciju kamere preko pomeraja misa
        /// </summary>
        public void UpdateCameraRotation(double deltaX, double deltaY)
        {
            //Moguce greksa
            if(lookAtCam == null)
            {
                lookAtCam = new LookAtCamera();
            }
            if (isLookAtCameraEnabled)
            {
                horizontalAngle += mouseSpeed * deltaX;
                verticalAngle += mouseSpeed * deltaY;

                direction.X = (float)(Math.Cos(verticalAngle) * Math.Sin(horizontalAngle));
                direction.Y = (float)(Math.Sin(verticalAngle));
                direction.Z = (float)(Math.Cos(verticalAngle) * Math.Cos(horizontalAngle));

                right.X = (float)Math.Sin(horizontalAngle - (Math.PI / 2));
                right.Y = 0f;
                right.Z = (float)Math.Cos(horizontalAngle - (Math.PI / 2));

                up = right.VectorProduct(direction);

                lookAtCam.Target = lookAtCam.Position + direction;
                lookAtCam.UpVector = up;
                
            }
        }

        /// <summary>
        ///  Azurira poziciju kamere preko tipki tastature
        /// </summary>
        public void UpdateCameraPosition(int deltaX, int deltaY, int deltaZ)
        {
            if (isLookAtCameraEnabled)
            {
                Vertex deltaForward = direction * deltaZ;
                Vertex deltaStrafe = right * deltaX;
                Vertex deltaUp = up * deltaY;
                Vertex delta = deltaForward + deltaStrafe + deltaUp;
                lookAtCam.Position += (delta * walkSpeed);
                lookAtCam.Target = lookAtCam.Position + direction;
                lookAtCam.UpVector = up;
            }
        }
        #endregion pomeraj_kamere
        #endregion


        #region glupe_set_metode

        public void setShadingMode(OpenGL gl)
        {
            if (m_selectedShadingMode == ShadingMode.Flat)
            {
                gl.ShadeModel(OpenGL.GL_FLAT);
            }
            else
            {
                gl.ShadeModel(OpenGL.GL_SMOOTH);
            }
        }

        private void setOutline(OpenGL gl)
        {
            // Ako je izabran rezim iscrtavanja objekta kao wireframe, ukljuci ga i obratno
            if (m_outline == true)
            {
                // Iscrtati ceo objekat kao zicani model.
                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            }
            else
            {
                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            }
        }

        private void setDepthTesting(OpenGL gl)
        {
            // Ako je izabrano testiranje dubine ukljuci ga i obratno
            if (m_depthTesting == true)
            {
                gl.Enable(OpenGL.GL_DEPTH_TEST);
            }
            else
            {
                gl.Disable(OpenGL.GL_DEPTH_TEST);
            }
        }

        private void setCulling(OpenGL gl)
        {
            // Ako je izabrano back face culling ukljuci ga i obratno
            if (m_culling == true)
            {
                gl.Enable(OpenGL.GL_CULL_FACE);
            }
            else
            {
                gl.Disable(OpenGL.GL_CULL_FACE);
            }
        }

        #endregion glupe_metode
        #region IDisposable Metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Destruktor.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }
        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            //if (disposing)
            //{
            //    //Oslobodi managed resurse
            //}

            if (disposing)
            {
                kamion_scene.Dispose();
            }
        }

        #endregion


        
    }
}
