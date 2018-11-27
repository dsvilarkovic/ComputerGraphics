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

namespace PF1S1._2
{
    ///<summary> Klasa koja enkapsulira OpenGL programski kod </summary>
    class World
    {
        #region Atributi

        private String[] tekstProjekta = {"Predmet: Racunarska grafika","Sk.god: 2018/19","Ime: Dusan","Prezime: Svilarkovic","Sifra zad: 1.2"};

        private DisplayList podlogaDisplayList = new DisplayList();
        private uint podlogaID = 0;


        /// <summary>
        /// Indikator da li je lookAtCamera podesena 
        /// </summary>
        public bool isLookAtCameraEnabled = false;


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


        #endregion

        #region Properties

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
            
            kamion_scene.LoadScene();
            kamion_scene.Initialize();

            kontejner_scene.LoadScene();
            kontejner_scene.Initialize();

            if(isLookAtCameraEnabled)
                SetupCamera(gl);
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
            DrawPodloga(gl);
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


            // Ako je izabrano back face culling ukljuci ga i obratno
            if (m_culling == true)
            {
                gl.Enable(OpenGL.GL_CULL_FACE);
            }
            else
            {
                gl.Disable(OpenGL.GL_CULL_FACE);
            }

            // Ako je izabrano testiranje dubine ukljuci ga i obratno
            if (m_depthTesting == true)
            {
                gl.Enable(OpenGL.GL_DEPTH_TEST);
            }
            else
            {
                gl.Disable(OpenGL.GL_DEPTH_TEST);
            }

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



            //gl.Viewport(0, 0, m_width, m_height);
            //gl.MatrixMode(OpenGL.GL_PROJECTION);
            //gl.LoadIdentity();

            //gl.Perspective(45f, (double)m_width / m_height, 1f, 100f);





            //TODO posle brisanja  lookAtCam otkomentarisati
            //lookAtCam.Project(gl);
            MoveAround(gl);

            gl.PushMatrix();
            
            gl.Color(0f, 1f, 0f);
            
            gl.PushMatrix();
                gl.Translate(0f, 0f, (float)-(2*POLU_DUZINA));
                DrawKamion(gl);
                DrawKontejner(gl);          
                //crtanje podloge
                gl.Color(0.8f, 0.8f, 0.8f);
                gl.CallList(podlogaID);            
                //DrawZid(gl);
                DrawBandera(gl);   
            gl.PopMatrix();     

            gl.PopMatrix();


            //3. Ispisati 2D tekst žutom bojom u donjem desnom uglu prozora 
            //(redefinisati viewport korišćenjem glViewport metode).
            //Font je Arial, 14pt, underline. Tekst treba biti oblika:            
            DrawProjectInfo(gl);
            
            gl.Flush();
        }


        private double scaleKontejner = 2 * 10e-4;
        private void DrawKontejner(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(POLU_DUZINA / 2, 0.1f, 0f);

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
            gl.Color(0f, 0f, 1f);


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


            gl.PopMatrix();
        }

        private void DrawKamion(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Translate(-POLU_DUZINA / 2, 0.1f, 0f);
            gl.Rotate(-90f, 0f, 1f, 0f);
            gl.Scale(0.4 * 10e-3, 0.4 * 10e-3, 0.4 * 10e-3);
            kamion_scene.Draw();
            gl.PopMatrix();
        }

        private void DrawBandera(OpenGL gl)
        {
            //crtanje bandere - Cylinder i  Cube
            gl.Color(1f, 0f, 0f);
            //sipka bandere
            gl.PushMatrix();
            gl.Translate(0f, 0.1f, 0f);
            gl.Rotate(-90f, 1f, 0f, 0f);
            Cylinder cylinder = new Cylinder();
            cylinder.TopRadius = 0.05f;
            cylinder.BaseRadius = cylinder.TopRadius;
            cylinder.Height = 3f;
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
        }

        private void MoveAround(OpenGL gl)
        {
            //kontrole tastature
            if (isLookAtCameraEnabled == false)
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

        private void DrawSphere(OpenGL gl)
        {
            gl.Color(0f, 0f, 1f);
            gl.PushMatrix();
            Sphere sphere = new Sphere();
            sphere.Radius = 0.5f;
            sphere.CreateInContext(gl);
            sphere.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }
        private void DrawPodloga(OpenGL gl)
        {
            
            gl.Begin(OpenGL.GL_QUADS);
                gl.Vertex(-POLU_DUZINA, 0.1f, -POLU_DUZINA);
                gl.Vertex(-POLU_DUZINA, 0.1f, POLU_DUZINA);
                gl.Vertex( POLU_DUZINA, 0.1f, POLU_DUZINA);
                gl.Vertex( POLU_DUZINA, 0.1f, -POLU_DUZINA); 
            gl.End();
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

        /// <summary>
        ///  Funkcija ograničava vrednost na opseg min - max
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
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

        #endregion

        #region IDisposable Metode

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
