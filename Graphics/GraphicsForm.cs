using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using System.Diagnostics;
using System;

namespace Graphics
{
    public partial class GraphicsForm : Form
    {
        Renderer renderer = new Renderer();
        Thread MainLoopThread;

        float deltaTime;
        public GraphicsForm()
        {
            InitializeComponent();
            simpleOpenGlControl1.InitializeContexts();

            MoveCursor();
            

            initialize();
            deltaTime = 0.005f;
            MainLoopThread = new Thread(MainLoop);
            MainLoopThread.Start();

        }
        void initialize()
        {
            renderer.Initialize();   
        }
        delegate void refreshCallBack();

        private void refreshOpenGl()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.simpleOpenGlControl1.InvokeRequired)
            {
                refreshCallBack d = new refreshCallBack(refreshOpenGl);
                this.Invoke(d);
            }
            else
            {
                this.simpleOpenGlControl1.Refresh();
            }
        }
        void MainLoop()
        {
            while (true)
            {
                renderer.Draw();
                renderer.Update(deltaTime);
                refreshOpenGl();
            }
        }
        private void GraphicsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            renderer.CleanUp();
            MainLoopThread.Abort();
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
            renderer.Draw();
            renderer.Update(deltaTime);
        }
        bool moving;
        private void simpleOpenGlControl1_KeyPress(object sender, KeyPressEventArgs e)
        {
            moving = false;
            float speed = 5f;
            if (e.KeyChar == 'a')
            {
                renderer.cam.Strafe(-speed);
                moving = true;
            }
            if (e.KeyChar == 'd')
            {
                renderer.cam.Strafe(speed);
                moving = true;
            }
            if (e.KeyChar == 's')
            {
                renderer.cam.Walk(-speed);
                moving = true;
            }
            if (e.KeyChar == 'w')
            {
                renderer.cam.Walk(speed);
                moving = true;
            }
            if (e.KeyChar == 'z')
            {
                renderer.cam.Fly(-speed);
                moving = true;
            }
            if (e.KeyChar == 'c')
            {
                renderer.cam.Fly(speed);
                moving = true;
            }
            label6.Text = "X: " + renderer.cam.GetCameraPosition().x;
            label7.Text = "Y: " + renderer.cam.GetCameraPosition().y;
            label8.Text = "Z: " + renderer.cam.GetCameraPosition().z;
        }

        float prevX, prevY;
        private void simpleOpenGlControl1_MouseMove(object sender, MouseEventArgs e)
        {
            float speed = 0.05f;
            float delta = e.X - prevX;
            if (delta > 2)
                renderer.cam.Yaw(-speed);
            else if (delta < -2)
                renderer.cam.Yaw(speed);


            delta = e.Y - prevY;
            if (delta > 2)
                renderer.cam.Pitch(-speed);
            else if (delta < -2)
                renderer.cam.Pitch(speed);

            MoveCursor();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            //float r = float.Parse(textBox1.Text);
            //float g = float.Parse(textBox2.Text);
            //float b = float.Parse(textBox3.Text);
            //float a = float.Parse(textBox4.Text);
            //float s = float.Parse(textBox5.Text);
            //renderer.SendLightData(r, g, b, a, s);
        }

        private void simpleOpenGlControl1_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void simpleOpenGlControl1_MouseClick(object sender, MouseEventArgs e)
        {
            renderer.draw = true;
            renderer.cam.mAngleY += 0.1f;
        }

        private void MoveCursor()
        {
            this.Cursor = new Cursor(Cursor.Current.Handle);
            Point p = PointToScreen(simpleOpenGlControl1.Location);
            Cursor.Position = new Point(simpleOpenGlControl1.Size.Width/2+p.X, simpleOpenGlControl1.Size.Height/2+p.Y);
            Cursor.Clip = new Rectangle(this.Location, this.Size);
            prevX = simpleOpenGlControl1.Location.X+simpleOpenGlControl1.Size.Width/2;
            prevY = simpleOpenGlControl1.Location.Y + simpleOpenGlControl1.Size.Height / 2;
        }
    }
}
