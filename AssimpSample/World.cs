// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Threading;
using Lighting;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;

namespace AssimpSample
{
    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        private enum TextureObjects { Brick = 0, Floor};
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;

        /// <summary>
        ///	 Identifikatori OpenGL tekstura
        /// </summary>
        private uint[] m_textures = null;

        /// <summary>
        ///	 Putanje do slika koje se koriste za teksture
        /// </summary>
        private string[] m_textureFiles = { "images\\bricks.jpg", "images\\wood.jpg"};

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene sveca;
        private AssimpScene tanjir;
        private SharpGL.SceneGraph.Quadrics.Sphere shadedSphere;

        private float scale = 2.0f;

        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        private float red = 1.0f;
        private float green = 0.0f;
        private float blue = 0.0f;

        public float Red
        {
            get { return red; }
            set { red = value; }
        }
        public float Green
        {
            get { return green; }
            set { green = value; }
        }
        public float Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotationCandlePlate = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 2000.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private DispatcherTimer timer;

        private bool animation = false;

        public bool Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        private float pomeriX = 0.0f;
        private float pomeriZ = 0.0f;

        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene1
        {
            get { return sveca; }
            set { sveca = value; }
        }

        public AssimpScene Scene2
        {
            get { return tanjir; }
            set { tanjir = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationYCandlePlate
        {
            get { return m_yRotationCandlePlate; }
            set { m_yRotationCandlePlate = value; }
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
        public World(String scenePath, String sceneFileName1, String sceneFileName2, int width, int height, OpenGL gl)
        {
            this.tanjir = new AssimpScene(scenePath, sceneFileName1, gl);
            this.sveca = new AssimpScene(scenePath, sceneFileName2, gl);
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
            gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.FrontFace(OpenGL.GL_CCW);

            SetupLighting(gl);

            // Podesavanje sfere
            shadedSphere = new SharpGL.SceneGraph.Quadrics.Sphere();
            shadedSphere.CreateInContext(gl);
            shadedSphere.Radius = 5f;
            shadedSphere.Material = new SharpGL.SceneGraph.Assets.Material();
            shadedSphere.Material.Ambient = Color.Red;
            shadedSphere.Material.Diffuse = Color.White;
            shadedSphere.Material.Shininess = 128;
            shadedSphere.Material.Bind(gl);
            //

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);

            gl.GenTextures(m_textureCount, m_textures);

            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, ImageLockMode.ReadOnly,
                                                      PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT); // wrapping
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT); // wrapping// Linear Filtering

                image.UnlockBits(imageData);
                image.Dispose();
            }

            tanjir.LoadScene();
            tanjir.Initialize();

            sveca.LoadScene();
            sveca.Initialize();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5);
            timer.Tick += new EventHandler(UpdateAnimation);
        }

        private void UpdateAnimation(object sender, EventArgs e)
        {
            pomeriZ -= 10f;

            if (pomeriZ == -500)
            {
                pomeriX = 0.0f;
                pomeriZ = 0.0f;
                Animation = false;
                timer.Stop();
            }
        }

        public void BeginAnimation()
        {
            RotationX = 0.0f;
            RotationY = 0.0f;
            m_sceneDistance = 2000.0f;
            Animation = true;
            timer.Start();
        }

        private void SetupLighting(OpenGL gl)
        {
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);

            gl.Enable(OpenGL.GL_NORMALIZE);

            gl.ShadeModel(OpenGL.GL_SMOOTH);

            float[] global_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);

            float[] light0pos = new float[] { 0.0f, 1000.0f, 0.0f, 1.0f };
            float[] light0ambient = new float[] { 0.1f, 0.1f, 0.1f, 1.0f };
            float[] light0diffuse = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 180.0f);

            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.Enable(OpenGL.GL_LIGHT1);
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void DrawSveca(OpenGL gl)
        {
            gl.PushMatrix();


            if (Animation)
            {
                gl.Translate(0.0f + pomeriX, 0.0f, 0.0f + pomeriZ);
            }

            float[] light1pos = new float[] { 0.0f, 120.0f, 0.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, light1pos);

            shadedSphere.Slices = 120;
            shadedSphere.Stacks = 120;
            shadedSphere.Material.Bind(gl);

            gl.Translate(0.0f, 120.0f, 0.0f);

            shadedSphere.Render(gl, SharpGL.SceneGraph.Core.RenderMode.Render);

            gl.Translate(0.0f, -120.0f, 0.0f);

            gl.Rotate(m_yRotationCandlePlate, 0.0f, 1.0f, 0.0f);

            gl.Scale(10f, 10f, 10f);
            gl.Rotate(-90.0f, 0.0f, 0.0f);
            sveca.Draw();
            gl.PopMatrix();

        }

        public void DrawTanjir(OpenGL gl)
        {
            gl.PushMatrix();

            if (Animation)
            {
                gl.Translate(0.0f + pomeriX, 0.0f, 0.0f + pomeriZ);
            }


            gl.Rotate(m_yRotationCandlePlate, 0.0f, 1.0f, 0.0f);

            gl.Scale(scale, scale, scale);
            gl.Translate(0.0f, 10.0f, 0.0f);
            tanjir.Draw();
            gl.PopMatrix();
        }

        public void DrawText(OpenGL gl)
        {

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-30, 0, -8, 12);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.Color(0.0f, 191.0f, 255.0f); 
            gl.Translate(-4f, -2f, 0.0f);

            gl.PushMatrix();
            gl.Translate(-1.5f, -2f, 0f);
            gl.Scale(0.5f, 0.5f, 0.5f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "Predmet: Racunarska grafika");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1.5f, -2.1f, 0f);
            gl.Scale(0.58f, 0.58f, 0.58f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "____________________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1.5f, -2.7f, 0f);
            gl.Scale(0.5f, 0.5f, 0.5f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "Sk.god: 2019/20");
            gl.PopMatrix();


            gl.PushMatrix();
            gl.Translate(-1.5f, -2.8f, 0f);
            gl.Scale(0.6f, 0.6f, 0.6f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "___________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1.5f, -3.5f, 0f);
            gl.Scale(0.5f, 0.5f, 0.5f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "Ime: Ivana");
            gl.PopMatrix();


            gl.PushMatrix();
            gl.Translate(-1.5f, -3.6f, 0f);
            gl.Scale(0.61f, 0.61f, 0.61f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "_______");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1.5f, -4.2f, 0f);
            gl.Scale(0.5f, 0.5f, 0.5f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "Prezime: Brkic");
            gl.PopMatrix();


            gl.PushMatrix();
            gl.Translate(-1.5f, -4.3f, 0f);
            gl.Scale(0.6f, 0.6f, 0.6f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "__________");
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(-1.5f, -5f, 0f);
            gl.Scale(0.5f, 0.5f, 0.5f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "Sifra zad: 14.2");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-1.5f, -5.1f, 0f);
            gl.Scale(0.58f, 0.58f, 0.58f);
            gl.DrawText3D("Times New Roman", 5f, 1f, 0.1f, "__________");
            gl.PopMatrix();
        }

        public void DrawPodloga(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

            gl.Scale(30f, 30f, 30f);

            gl.PushMatrix();

            gl.Translate(0f, -3f, 0f);

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Floor]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0.0f, 1.0f, 0.0f);
            //pod
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-20f, 0f, 20f);
            gl.TexCoord(0.0f, 3.0f);
            gl.Vertex(-20f, 0f, -20f);
            gl.TexCoord(3.0f, 3.0f);
            gl.Vertex(20f, 0f, -20f);
            gl.TexCoord(3.0f, 0.0f);
            gl.Vertex(20f, 0f, 20f);

            gl.End();

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Brick]);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(LightingUtilities.FindFaceNormal(20.0f, 20.0f, -20.0f, 20.0f, 20.0f, 20.0f, 20.0f, 0.0f, 20.0f));
            //desna strana
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(20f, 20f, -20f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(20f, 20f, 20f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(20f, 0f, 20f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(20f, 0f, -20f);
            //leva strana
            gl.Normal(LightingUtilities.FindFaceNormal(-20.0f, 20.0f, 20.0f, -20.0f, 20.0f, -20.0f, -20.0f, 0.0f, -20.0f));
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-20f, 20f, 20f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-20f, 20f, -20f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(-20f, 0f, -20f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-20f, 0f, 20f);
            //napred
            gl.Normal(LightingUtilities.FindFaceNormal(20.0f, 20.0f, 20.0f, -20.0f, 20.0f, 20.0f, -20.0f, 0.0f, 20.0f));
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(20f, 20f, 20f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(-20f, 20f, 20f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(-20f, 0f, 20f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(20f, 0f, 20f);
            //nazad
            gl.Normal(LightingUtilities.FindFaceNormal(-20.0f, 20.0f, -20.0f, 20.0f, 20.0f, -20.0f, 20.0f, 0.0f, -20.0f));
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(-20f, 20f, -20f);
            gl.TexCoord(1.0f, 0.0f);
            gl.Vertex(20f, 20f, -20f);
            gl.TexCoord(1.0f, 1.0f);
            gl.Vertex(20f, 0f, -20f);
            gl.TexCoord(0.0f, 1.0f);
            gl.Vertex(-20f, 0f, -20f);

            gl.End();

            gl.PopMatrix();
        }
        

        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.PushMatrix();

            gl.LookAt(0.0f, 200.0f,  -m_sceneDistance+pomeriZ+180.0f, 0.0f + pomeriX, 100f, -2000 + pomeriZ, 0.0f, 1.0f, 0.0f);

            gl.Translate(0.0f, 0.0f, -2000);
            gl.Rotate(m_xRotation, 1.0f, 0.0f, 0.0f);
            gl.Rotate(m_yRotation, 0.0f, 1.0f, 0.0f);

            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Color(red, green, blue);

            float[] light1diffuse = new float[] { red, green, blue, 1.0f };

            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, light1diffuse);

            DrawSveca(gl);

            DrawTanjir(gl);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(0.5f, 0.5f, 0.5f);

            DrawPodloga(gl);

            gl.Disable(OpenGL.GL_TEXTURE_2D);

            DrawText(gl);

            gl.PopMatrix();

            gl.Viewport(0, 0, m_width, m_height); // kreiraj viewport po celom prozoru
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.Flush();
        }
        

        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.Viewport(0, 0, width, height); // kreiraj viewport po celom prozoru
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, (double)width / height, 1f, 20000f);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                tanjir.Dispose();
                sveca.Dispose();
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
