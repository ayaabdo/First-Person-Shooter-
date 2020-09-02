using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using GlmNet;
using System.IO;
using Graphics._3D_Models;

namespace Graphics
{
    class bullet
    {
        Model3D fireGun;
        vec3 enemyPos;
        Enemy enemy;
        vec3 dir;
     
    public bullet(vec3 gunPos,vec3 target)
        {
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            fireGun = new Model3D();
            fireGun.LoadFile(projectPath + "\\ModelFiles\\45", 2, "45.obj");
            fireGun.scalematrix = glm.scale(new mat4(1), new vec3(100f, 100f, 100f));
            enemyPos += gunPos;
            dir = target;
        }
        public void Draw(int matID)
        {
            fireGun.Draw(matID);
        }
        
        public void update()
        {
            enemyPos += (dir * 0.35f);
            fireGun.transmatrix = glm.translate(new mat4(1), enemyPos);
            //SoundPlayer soundPlayer;
            //soundPlayer = new SoundPlayer();
            //soundPlayer.SoundLocation = projectPath + "\\Shooting An MP5-SoundBible.com-704967207.wav";
            //soundPlayer.Play();
        }
    }
}
