using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using Tao.OpenGl;

namespace x_IMU_IMU_and_AHRS_Algorithms
{
    /// <summary>
    /// 3D Cuboid form class.
    /// </summary>
    public partial class Form_3Dcuboid : Form
    {
        #region Variables and enumerations

        /// <summary>
        /// Form update timer.
        /// </summary>
        private Timer formUpdateTimer;

        /// <summary>
        /// Array of image file paths.
        /// </summary>
        private string[] imageFiles;

        /// <summary>
        /// Array of textures
        /// </summary>
        private uint[] textures;

        /// <summary>
        /// Dimensions of cuboid.
        /// </summary>
        private float halfXdimension, halfYdimension, halfZdimension;

        /// <summary>
        /// Transformation matrix describing translation and orientation of cuboid.
        /// </summary>
        private float[] transformationMatrix;

        /// <summary>
        /// Camera views of the cuboid.
        /// </summary>
        public enum CameraViews
        {
            Right,
            Left,
            Back,
            Front,
            Top,
            Bottom
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the form should minimise when closed by the user.
        /// </summary>
        public bool MinimizeInsteadOfClose { get; set; }

        /// <summary>
        /// Gets or sets a value describing the camera view of the cuboid. See Form_3Dcuboid.CameraViews.
        /// </summary>
        public CameraViews CameraView { get; set; }

        /// <summary>
        /// Gets or sets the distance of the camera from the world origin.
        /// </summary>
        public float CameraDistance { get; set; }

        /// <summary>
        /// Gets or sets the translation vector describing the position of the cuboid relative to world origin.
        /// </summary>
        public float[] TranslationVector
        {
            get
            {
                return new float[] { transformationMatrix[12],
                                     transformationMatrix[13],
                                     transformationMatrix[14] };
            }
            set
            {
                if (value.Length != 3) throw new Exception("Array must be of length 3.");
                transformationMatrix[12] = value[0];
                transformationMatrix[13] = value[1];
                transformationMatrix[14] = value[2];
            }
        }

        /// <summary>
        /// Gets or sets the rotation matrix describing the orientation of the cuboid relative to world.
        /// </summary>
        /// <remarks>
        /// Index order is row major. See http://en.wikipedia.org/wiki/Row-major_order
        /// </remarks> 
        public float[] RotationMatrix
        {
            get
            {
                return new float[] {transformationMatrix[0], transformationMatrix[4], transformationMatrix[8],
                                    transformationMatrix[1], transformationMatrix[5], transformationMatrix[9],
                                    transformationMatrix[2], transformationMatrix[6], transformationMatrix[10]};
            }
            set
            {
                if (value.Length != 9) throw new Exception("Array must be of length 9.");
                transformationMatrix[0] = value[0]; transformationMatrix[4] = value[1]; transformationMatrix[8] = value[2];
                transformationMatrix[1] = value[3]; transformationMatrix[5] = value[4]; transformationMatrix[9] = value[5];
                transformationMatrix[2] = value[6]; transformationMatrix[6] = value[7]; transformationMatrix[10] = value[8];
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        public Form_3Dcuboid()
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, new float[] { 6, 4, 2 }, CameraViews.Front, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths)
            : this(imageFilePaths, new float[] { 6, 4, 2 }, CameraViews.Front, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, float[] dimensions)
            : this(imageFilePaths, dimensions, CameraViews.Front, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, float[] dimensions, CameraViews cameraView)
            : this(imageFilePaths, dimensions, cameraView, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, CameraViews cameraView)
            : this(imageFilePaths, new float[] { 6, 4, 2 }, cameraView, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, CameraViews cameraView, float cameraDistance)
            : this(imageFilePaths, new float[] { 6, 4, 2 }, cameraView, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, float cameraDistance)
            : this(imageFilePaths, new float[] { 6, 4, 2 }, CameraViews.Front, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        public Form_3Dcuboid(float[] dimensions)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, dimensions, CameraViews.Front, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        public Form_3Dcuboid(float[] dimensions, CameraViews cameraView)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, dimensions, cameraView, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(float[] dimensions, CameraViews cameraView, float cameraDistance)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, dimensions, cameraView, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(float[] dimensions, float cameraDistance)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, dimensions, CameraViews.Front, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        public Form_3Dcuboid(CameraViews cameraView)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, new float[] { 6, 4, 2 }, cameraView, 50.0f)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(CameraViews cameraView, float cameraDistance)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, new float[] { 6, 4, 2 }, cameraView, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(float cameraDistance)
            : this(new string[] { "Form_3Dcuboid/Right.png", "Form_3Dcuboid/Left.png", "Form_3Dcuboid/Back.png", "Form_3Dcuboid/Front.png", "Form_3Dcuboid/Top.png", "Form_3Dcuboid/Bottom.png" }, new float[] { 6, 4, 2 }, CameraViews.Front, cameraDistance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Form_3Dcuboid"/> class.
        /// </summary>
        /// <param name="imageFilePaths">
        /// File paths of images used for 6 faces of cuboid. Index order is: Right (+X), Left (-X), Back (+Y), Front (-Y), Top (+Z), and Bottom (-Z).
        /// </param>
        /// <param name="dimensions">
        /// Dimensions of the cuboid.
        /// </param>
        /// <param name="cameraView">
        /// Value describing the camera view of the cuboid.
        /// </param>
        /// <param name="cameraDistance">
        /// Distance of the camera from the world origin.
        /// </param>
        public Form_3Dcuboid(string[] imageFilePaths, float[] dimensions, CameraViews cameraView, float cameraDistance)
        {
            InitializeComponent();
            MinimizeInsteadOfClose = false;
            transformationMatrix = new float[] {1.0f, 0.0f, 0.0f, 0.0f,
                                                0.0f, 1.0f, 0.0f, 0.0f,
                                                0.0f, 0.0f, 1.0f, 0.0f,
                                                0.0f, 0.0f, 0.0f, 1.0f};
            imageFiles = imageFilePaths;
            halfXdimension = dimensions[0] / 2;
            halfYdimension = dimensions[1] / 2;
            halfZdimension = dimensions[2] / 2;
            CameraView = cameraView;
            CameraDistance = cameraDistance;
            formUpdateTimer = new Timer();
            formUpdateTimer.Interval = 20;
            formUpdateTimer.Tick += new EventHandler(formUpdateTimer_Tick);
        }

        #endregion

        #region Form events

        /// <summary>
        /// Form visible changed event to start/stop form update formUpdateTimer.
        /// </summary>
        private void Form_3Dcuboid_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                formUpdateTimer.Start();
            }
            else
            {
                formUpdateTimer.Stop();
            }
        }

        /// <summary>
        /// Form closing event to minimise form instead of close.
        /// </summary>
        private void Form_3Dcuboid_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MinimizeInsteadOfClose)
            {
                this.WindowState = FormWindowState.Minimized;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Timer tick event to refresh graphics.
        /// </summary>
        private void formUpdateTimer_Tick(object sender, EventArgs e)
        {
            lock (this)
            {
                simpleOpenGlControl.Refresh();
            }
        }

        #endregion

        #region SimpleOpenGlControl methods

        /// <summary>
        /// Form load event to initialises OpenGL graphics.
        /// </summary>
        private void simpleOpenGlControl_Load(object sender, EventArgs e)
        {
            simpleOpenGlControl.InitializeContexts();
            simpleOpenGlControl.SwapBuffers();
            simpleOpenGlControl_SizeChanged(sender, e);
            Gl.glShadeModel(Gl.GL_SMOOTH);
            Gl.glEnable(Gl.GL_LINE_SMOOTH);
            Gl.glEnable(Gl.GL_TEXTURE_2D);						    // Enable Texture Mapping            
            Gl.glEnable(Gl.GL_NORMALIZE);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glEnable(Gl.GL_DEPTH_TEST);						    // Enables Depth Testing
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glHint(Gl.GL_POLYGON_SMOOTH_HINT, Gl.GL_NICEST);     // Really Nice Point Smoothing
            textures = LoadTextureFromImage(imageFiles);
        }

        /// <summary>
        /// Loads textures from files.
        /// </summary>
        /// <param name="filesNames">
        /// File paths of texture image files.
        /// </param> 
        private uint[] LoadTextureFromImage(string[] filesNames)
        {
            int numOfPic = filesNames.Length;
            uint[] texture = new uint[numOfPic];
            Bitmap[] bitmap = new Bitmap[numOfPic];
            BitmapData[] bitmapdata = new BitmapData[numOfPic];
            for (int im = 0; im < numOfPic; im++)
            {
                if (File.Exists(filesNames[im]))
                {
                    bitmap[im] = new Bitmap(filesNames[im]);
                }
            }
            Gl.glGenTextures(numOfPic, texture);
            for (int i = 0; i < numOfPic; i++)
            {
                bitmap[i].RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, bitmap[i].Width, bitmap[i].Height);
                bitmapdata[i] = bitmap[i].LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                Gl.glBindTexture(Gl.GL_TEXTURE_2D, texture[i]);
                Gl.glTexEnvf(Gl.GL_TEXTURE_ENV, Gl.GL_TEXTURE_ENV_MODE, Gl.GL_MODULATE);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_NEAREST);
                Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_NEAREST);
                Glu.gluBuild2DMipmaps(Gl.GL_TEXTURE_2D, (int)Gl.GL_RGB, bitmap[i].Width, bitmap[i].Height, Gl.GL_BGR_EXT, Gl.GL_UNSIGNED_BYTE, bitmapdata[i].Scan0);
                bitmap[i].UnlockBits(bitmapdata[i]);
                bitmap[i].Dispose();
            }
            return texture;
        }

        /// <summary>
        /// Window resize event to adjusts perspective.
        /// </summary>
        private void simpleOpenGlControl_SizeChanged(object sender, EventArgs e)
        {
            int height = simpleOpenGlControl.Size.Height;
            int width = simpleOpenGlControl.Size.Width;
            Gl.glViewport(0, 0, width, height);
            Gl.glPushMatrix();
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(10, (float)width / (float)height, 1.0, 250);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glPopMatrix();
        }

        /// <summary>
        /// Redraw cuboid polygons.
        /// </summary>
        private void simpleOpenGlControl_Paint(object sender, PaintEventArgs e)
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);    // Clear screen and DepthBuffer
            Gl.glLoadIdentity();
            Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);

            // Set camera view and distance
            Gl.glTranslatef(0, 0, -1.0f * CameraDistance);
            switch (CameraView)
            {
                case (CameraViews.Right):
                    Gl.glRotatef(-90, 0, 1, 0);
                    Gl.glRotatef(-90, 1, 0, 0);
                    break;
                case (CameraViews.Left):
                    Gl.glRotatef(90, 0, 1, 0);
                    Gl.glRotatef(-90, 1, 0, 0);
                    break;
                case (CameraViews.Back):
                    Gl.glRotatef(90, 1, 0, 0);
                    Gl.glRotatef(180, 0, 1, 0);
                    break;
                case (CameraViews.Front):
                    Gl.glRotatef(-90, 1, 0, 0);
                    break;
                case (CameraViews.Top):
                    break;
                case (CameraViews.Bottom):
                    Gl.glRotatef(180, 1, 0, 0);
                    break;
            }

            Gl.glPushMatrix();
            Gl.glMultMatrixf(transformationMatrix);                         // apply transformation matrix to cuboid

            // +'ve x face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[0]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(1, 0, 0); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glNormal3f(1, 0, 0); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(1, 0, 0); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(halfXdimension, halfYdimension, halfZdimension);
            Gl.glNormal3f(1, 0, 0); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(halfXdimension, halfYdimension, -halfZdimension);
            Gl.glEnd();

            // -'ve x face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[1]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(-1, 0, 0); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(-halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glNormal3f(-1, 0, 0); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(-halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(-1, 0, 0); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-halfXdimension, halfYdimension, halfZdimension);
            Gl.glNormal3f(-1, 0, 0); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-halfXdimension, halfYdimension, -halfZdimension);
            Gl.glEnd();

            // +'ve y face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[2]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(0, 1, 0); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(-halfXdimension, halfYdimension, -halfZdimension);
            Gl.glNormal3f(0, 1, 0); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(-halfXdimension, halfYdimension, halfZdimension);
            Gl.glNormal3f(0, 1, 0); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(halfXdimension, halfYdimension, halfZdimension);
            Gl.glNormal3f(0, 1, 0); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(halfXdimension, halfYdimension, -halfZdimension);
            Gl.glEnd();

            // -'ve y face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[3]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(0, -1, 0); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glNormal3f(0, -1, 0); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(0, -1, 0); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(0, -1, 0); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glEnd();

            // +'ve z face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[4]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(0, 0, 1); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(0, 0, 1); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(halfXdimension, -halfYdimension, halfZdimension);
            Gl.glNormal3f(0, 0, 1); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(halfXdimension, halfYdimension, halfZdimension);
            Gl.glNormal3f(0, 0, 1); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-halfXdimension, halfYdimension, halfZdimension);
            Gl.glEnd();

            // -'ve z face
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, textures[5]);
            Gl.glBegin(Gl.GL_QUADS);
            Gl.glNormal3f(0, 0, -1); Gl.glTexCoord2f(0, 1); Gl.glVertex3f(-halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glNormal3f(0, 0, -1); Gl.glTexCoord2f(1, 1); Gl.glVertex3f(halfXdimension, -halfYdimension, -halfZdimension);
            Gl.glNormal3f(0, 0, -1); Gl.glTexCoord2f(1, 0); Gl.glVertex3f(halfXdimension, halfYdimension, -halfZdimension);
            Gl.glNormal3f(0, 0, -1); Gl.glTexCoord2f(0, 0); Gl.glVertex3f(-halfXdimension, halfYdimension, -halfZdimension);
            Gl.glEnd();

            Gl.glPopMatrix();
            Gl.glFlush();
        }

        #endregion
    }
}