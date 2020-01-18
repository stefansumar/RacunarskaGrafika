// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Stefan Šumar Mihić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL.Enumerations;
using SharpGL;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Ugao rotacije Meseca
        /// </summary>
        private float m_moonRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije Zemlje
        /// </summary>
        private float m_earthRotation = 0.0f;

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = -2000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height = 0;
        private int w_width;
        private int w_height;
        private AssimpSample.AssimpScene camera;
        private float w_rotation = 20.0f;
        public float visinaKaveza = 280.0f;
        public float visinaKamere = 0.05f;
        public float sirinaKamere = 0.05f;
        

        float[] ambient_light = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };

        // Ovo mi treba za teksture
        private enum TextureObjects { Bricks = 0, Concentrate, Rust}
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private string[] m_textureFiles = { "..//..//images//bricks.jpg", "..//..//images//stone.jpg", "..//..//images//rust.jpg" };
        private uint[] m_textures = null;

        //Animacije
        private Boolean m_startAnimation = false;
        private DispatcherTimer animationTimer;
        private float cameraRotation = -80.0f;
        private float openDoor = -2.7f;
        public float speedRotation = 5.0f;
        public float doorRotation = 0.0f;

        public Boolean StartAnimation
        {
            get { return m_startAnimation; }
            set { m_startAnimation = value; }
        }

        public float CameraRotation
        {
            get { return cameraRotation; }
            set { cameraRotation = value; }
        }

        public float OpenDoor
        {
            get { return openDoor; }
            set { openDoor = value; }
        }




        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        /// 

        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
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
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
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

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneCamera, int width, int height, OpenGL gl)
        {
            this.camera = new AssimpScene(scenePath, sceneCamera, gl);
            this.m_width = width;
            this.m_height = height;
            m_textures = new uint[m_textureCount];
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            //Prvi deo zadatka
            // Testiranje dubine - omogucuje da se vidi najkasnije nacrtan objekat UKLJUCENO
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            // Cull face mode - sakrivanje nevidljivih povrsina UKLJUCENO
            gl.Enable(OpenGL.GL_CULL_FACE_MODE);
            // Color tracking mehanizam
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Enable(OpenGL.GL_NORMALIZE);
            gl.Enable(OpenGL.GL_AUTO_NORMAL);

            

            // Teksture
            // Enable ce omoguciti rad sa teksurama
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            // Stapanje teksture sa materijalom
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            // Ucitavanje slika i kreiranje tekstura
            gl.GenTextures(m_textureCount, m_textures);

            for (int i = 0; i < m_textureCount; i++)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);
                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // Rotiramo sliku zbog koordinatnog sistema OpenGL-a
                image.RotateFlip(RotateFlipType.Rotate90FlipX);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljava providnost slike)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //MipMap linearno filtriranje
                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);		
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);	
                // Podesavanje Wrapinga da bude GL_REPEAT po obema osama
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);		
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);      

                image.UnlockBits(imageData);
                image.Dispose();
            }

            camera.LoadScene();
            camera.Initialize();

            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(1);
            animationTimer.Tick += new EventHandler(UpdateAnimation);

        }

        

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            // Podesavanje pozicije kamere koje je wtf
            //gl.LookAt(0, 199, -145, 0, 0, -m_sceneDistance, 0, 1, 0);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            if (m_startAnimation)
            {
                animationTimer.Start();
            }
            else
            {
                animationTimer.Stop();

                cameraRotation = -80.0f;
                openDoor = -2.7f;
            }


            gl.Viewport(0, 0, m_width, m_height);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Enable(OpenGL.GL_DEPTH);
            gl.LoadIdentity();


            gl.PushMatrix();
            //LookAt je metoda kojom namestam kameru, vidi se bocna strana kaveza i vrh
            //gl.LookAt(0, -50, -2700, 0, 0, -1500, 0, 1, 0);
            gl.Translate(0.0f, -50.0f, m_sceneDistance);
            gl.Rotate(30.0f, -30.0f, 0.0f);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            drawGround(gl);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Bricks]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(1.0f, 1.0f, 1.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            drawWalls(gl);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);
            drawCage(gl);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);
            DrawDoor(gl);
            gl.Disable(OpenGL.GL_TEXTURE_2D);

            drawCamera(gl);

            drawLight(gl);

            drawRedLight(gl);

            drawText(gl);
            
            gl.PopMatrix();

            // Oznaci kraj iscrtavanja
            gl.Flush();
        }

        // Metoda za iscrtavanje podloge
        public void drawGround(OpenGL gl)
        {
            // PushMatrix se poziva da bi se sacuvalo prethodno nacrtano stanje, sprecava se kumulativnost
            gl.PushMatrix();
            //ovo ce biti boja puta
            gl.Color(0.6f, 0.6f, 0.6f);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Concentrate]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Translate(-300.0f, 0.0f, 0.0f);
            //Definisanje tacaka podloge Vertex = tacka
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(500.0f, -300.0f, -485.0f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-500.0f, -300.0f, -485.0f);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-500.0f, -300.0f, 500.0f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(500.0f, -300.0f, 500.0f);
            gl.End();
            gl.PopMatrix();

        }

        // Metoda za iscrtavanje zidova
        public void drawWalls(OpenGL gl)
        {
            // Cube klasa koja nam sluzi za iscvanje zidova
            Cube cube = new Cube();

            //Sredisnji zid
            gl.PushMatrix();
            //gl.Color(1.0f, 0.0f, 0.0f);
            gl.Translate(0.0f, 100.0f, -485.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // Desni, bocni zid
            gl.PushMatrix();
            //gl.Color(0.137255f, 0.556863f, 0.137255f);
            gl.Translate(500.0f, 100.0f, 0.0f);
            gl.Rotate(0.0f, 90.0f, 0.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

            // Levi, bocni zid
            gl.PushMatrix();
            //gl.Color(0.137255f, 0.556863f, 0.137255f);
            gl.Translate(-500.0f, 100.0f, 0.0f);
            gl.Rotate(0.0f, -90.0f, 0.0f);
            gl.Scale(500.0f, 400.0f, 10.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();

        }

        // Metoda za iscrtavanje kaveza
        public void drawCage(OpenGL gl)
        {
            Cylinder kavez = new Cylinder();

            gl.PushMatrix();
            gl.Translate(0.0f, -300.0f, 0.0f);
            kavez.CreateInContext(gl);
            // Dodavanje teksture na kavez
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Rust]);

            // Skaliranje teksture
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(5.0f, 5.0f, 5.0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            kavez.NormalGeneration = Normals.Smooth;
            kavez.NormalOrientation = Orientation.Outside;
            kavez.TextureCoords = true;
            gl.Scale(160.0f, visinaKaveza, 300.0f);
            kavez.TopRadius = 1;
            gl.Rotate(-90.0f, 0.0f, 0.0f);
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
            gl.LineWidth(3.0f);
            kavez.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);


        }

        // Metoda za iscrtavanje kamere
        public void drawCamera(OpenGL gl)
        {
            gl.PushMatrix();
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Translate(350.0f, 100.0f, -300.0f);
            gl.Rotate(0.0f, cameraRotation, 0.0f);
            gl.Rotate(m_earthRotation, 0f, 1f, 0f);
            gl.Scale(visinaKamere, 0.05f, sirinaKamere);
            camera.Draw();
            gl.PopMatrix();

        }

        // Metoda za iscrtavanje teksta
        public void drawText(OpenGL gl)
        {
            gl.PushMatrix();
            gl.Viewport(m_width/2, 0, m_width/2, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-10, 13, -10, 12.5);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.Translate(-9.0f, 10.0f, 0.0f);
            gl.Color(1.0f, 0.0f, 1.0f);

            gl.PushMatrix();
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "Predmet: Racunarska grafika");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "________________________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -1.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "Sk.god: 2019/20");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -1.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "_____________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -2.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "Ime: Stefan");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -2.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "_________");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -3.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "Prezime: Sumar");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -3.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "_____________");
            gl.PopMatrix();
            gl.PushMatrix(); gl.Translate(0.0f, -4.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "Sifra: 17.1");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(0.0f, -4.0f, 0.0f);
            gl.DrawText3D("Arial", 12.0f, 0f, 0f, "_________");
            gl.PopMatrix();

            Resize(gl, m_width, m_height);
            gl.PopMatrix();

        }

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            // Ovde se vrsi definisanje perspektiva projekcije
            m_width = width;
            m_height = height;
            //Viewport ????
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(50.0f, (double)m_width / m_height, 1.0f, 20000f);
            //gl.LookAt(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        private void SetupLighting(OpenGL gl)
        {
            float[] ambijentalnaKomponenta = { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] difuznaKomponenta = { 0.7f, 0.7f, 0.7f, 1.0f };
            // Pridruži komponente svetlosnom izvoru 0
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT,
            ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            // Podesi parametre tackastog svetlosnog izvora
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f); //tackasti izvor cutoff 180
            // Ukljuci svetlosni izvor
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHTING);
            // Pozicioniraj svetlosni izvor
            float[] pozicija = { -40f, 0f, 0f, 1f }; //negativna x-osa, levo od kaveza; kec za reflektivno
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicija);
        }

        private void DrawDoor(OpenGL gl)
        {
            gl.PolygonMode(SharpGL.Enumerations.FaceMode.FrontAndBack, SharpGL.Enumerations.PolygonMode.Filled);
            Cube cube = new Cube();
            gl.PushMatrix();
            gl.Scale(1f, 80f, 40f);
            gl.Translate(160f, openDoor, 0f);
            gl.Rotate(0.0f, doorRotation, 0.0f);
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
        }

        // Funkcija za animaciju
        private void UpdateAnimation(object sender, EventArgs e)
        {


            //Crveno svetlo
            OpenGL gl = new OpenGL();
            float[] reflektorskipos = new float[] { 0f, -200f, -m_sceneDistance + 4000, 1.0f };
            float[] reflektorskiambient = new float[] { 1.0f, 0.0f, 0.4f, 0.0f };
            float[] reflektorskidiffuse = new float[] { 0.0f, 0.0f, 1.0f, 0.0f };
            float[] reflektorskispecular = new float[] { 0.0f, 0.0f, 1.0f, 0.0f };
            float[] reflektorskidirection = new float[] { 200.0f, -1f, -m_sceneDistance + 3500, 0.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, reflektorskiambient);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, reflektorskidiffuse);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, reflektorskispecular);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, reflektorskidirection);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 25.0f);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, reflektorskipos);
            gl.Enable(OpenGL.GL_LIGHT1);

            if (cameraRotation > -80.0f && cameraRotation <= -120.0f)
            {
                cameraRotation -= speedRotation;
                
            }

            else
            {
                cameraRotation += speedRotation;
            }

            openDoor = -1f;
            





        }

        

        public void drawLight(OpenGL gl)
        {
            gl.PushMatrix();
            Sphere sfera = new Sphere();

            gl.Translate(-200.0f, 400.0f, 100.0f);
            gl.Scale(5.0f, 5.0f, 5.0f);
            gl.Color(255f, 255f, 255f);
            sfera.Radius = 7f;

            sfera.CreateInContext(gl);
            SetupLighting(gl);
            sfera.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();


        }

        public void drawRedLight(OpenGL gl)
        {
            gl.PushMatrix();
            Sphere sfera = new Sphere();

            gl.Translate(325.0f, 260.0f, -370.0f);
            gl.Scale(1.0f, 1.0f, 1.0f);
            gl.Color(1.0f, 0.0f, 0.0f);
            sfera.Radius = 7f;

            sfera.CreateInContext(gl);
            sfera.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);
            gl.PopMatrix();


        }



        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
