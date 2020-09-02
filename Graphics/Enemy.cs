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
using System.Media;

namespace Graphics
{
    class Enemy 
    {
        vec3 dir;
        public vec3 enemyPos;
        md2LOL enemy;
        md2 samourai;
       public bool attackStarted = false;
       public bool runStarted = false;
        public SoundPlayer soundPlayer;
        bool lol_;
        public Enemy(string md2Path, vec3 enemPos,bool lol)
        {
            string projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            soundPlayer = new SoundPlayer();
            soundPlayer.SoundLocation = projectPath + "\\slap.wav";
            if (lol)
            {
                enemy = new md2LOL(md2Path);
                enemy.StartAnimation(animType_LOL.RUN);

                enemy.rotationMatrix =
                        glm.rotate((float)(270.0f / 180.0f * Math.PI), new vec3(1, 0, 0));
                //glm.rotate((float)(-180.0f / 180.0f * Math.PI), new vec3(0, 0, 1))

                enemy.scaleMatrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));
                enemy.TranslationMatrix = glm.translate(new mat4(1), enemPos);
            }
            else
            {
                samourai = new md2(md2Path);
                samourai.StartAnimation(animType.CROUCH_STAND);
                samourai.rotationMatrix = MathHelper.MultiplyMatrices(new List<mat4>() {
                    glm.rotate((float)(270.0f / 180.0f * Math.PI), new vec3(1, 0, 0)),
                glm.rotate((float)(-180.0f / 180.0f * Math.PI), new vec3(0, 1, 0))});
                samourai.scaleMatrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));
                samourai.TranslationMatrix = glm.translate(new mat4(1), enemPos);
            }
            enemyPos = enemPos;
            lol_ = lol;
        }
        public void Draw(int matID)
        {
            if (lol_)
                enemy.Draw(matID);
            else
                samourai.Draw(matID);
        }

        public void update(vec3 pos, float angle)
        {
            if (lol_)
                enemy.UpdateExportedAnimation();
            else
                samourai.UpdateExportedAnimation();
            float xDist = (pos.x - enemyPos.x) * (pos.x - enemyPos.x);
            float zDist = (pos.z - enemyPos.z) * (pos.z - enemyPos.z);

            double dist = Math.Sqrt(xDist + zDist);

            int rad = 1000;
            
            if (rad >= dist)
            {
                if (dist >= rad / 4)
                {
                    move(new vec3(pos.x, pos.y, pos.z), angle);
                    attackStarted = false;
                    runStarted = true;
                }
                else if (rad/4 > dist)
                {
                    Attack();
                    attackStarted = true;
                    runStarted = false;
                }
            }
            else
            {
                attackStarted = false;
                runStarted = false;
                if (lol_)
                    enemy.StartAnimation(animType_LOL.STAND);
                else
                    samourai.StartAnimation(animType.CROUCH_STAND);
            }

        }

        public void move(vec3 pos, float angle)
        {
            if (!runStarted)
            {
                if (lol_)
                    enemy.StartAnimation(animType_LOL.RUN);
                else
                    samourai.StartAnimation(animType.RUN);
            }

                dir = new vec3((pos.x - enemyPos.x), 0, (pos.z - enemyPos.z));
                dir = glm.normalize(dir);
                enemyPos += 0.35f * dir;
            if (lol_)
            {
                enemy.rotationMatrix = MathHelper.MultiplyMatrices(new List<mat4>() {
                glm.rotate((float)(-90.0f / 180.0f * Math.PI), new vec3(1, 0, 0)),
                glm.rotate((float)(angle), new vec3(0, 1, 0))
            });
                enemy.TranslationMatrix = glm.translate(new mat4(1), (enemyPos));
            }
            else
            {
                samourai.rotationMatrix = MathHelper.MultiplyMatrices(new List<mat4>() {
                glm.rotate((float)(-90.0f / 180.0f * Math.PI), new vec3(1, 0, 0)),
                glm.rotate((float)(angle), new vec3(0, 1, 0))
            });
                samourai.TranslationMatrix = glm.translate(new mat4(1), (enemyPos));
            }
        }

        public void Attack()
        {
            if (!attackStarted)
            {
                if (lol_)
                    enemy.StartAnimation(animType_LOL.ATTACK1);
                else
                    samourai.StartAnimation(animType.CROUCH_ATTACK);
                 slap();
            }
        }
        public void slap()
        {
             soundPlayer.Play();
        }
     }
}
