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
    class Renderer
    {
        //old variables
        public Shader sh, shader2D;
        public int transID, modelLoc;
        int viewID;
        int projID;
        int EyePositionID;
        int AmbientLightID;
        int DataID;
        mat4 ProjectionMatrix;
        mat4 ViewMatrix;
        public Camera cam;
        public Model3D gun;
        List<Model3D> trees = new List<Model3D>();
        List<Model3D> buildings = new List<Model3D>();
        List<Model3D> grass = new List<Model3D>();
        Model3D blad, stam, car, stam2, blad2, rock, rock1,building;
        Texture d, f, b, l, r, u;
        mat4 down, front, back, left, right, up;
        Model3D street;
        vec3 playerPos;
        uint ShootID, sqID;
        Texture shoot;
        Texture hp;
        Texture bhp;
        uint hpID;
        public mat4 healthbar;
        public mat4 backhealthbar;
        float scalef;
       // vec3  enemyPos1, enemyPos2, enemyPos3, enemyPos4, enemyPos5, samouriPos;
        Enemy enemy1 ;
        Enemy enemy2 ;
        Enemy enemy3 ;
        Enemy enemy4 ;
        Enemy enemy5 ;
        Enemy samourai;
        float mnX = 1000000000; float mxX = -1;
        float mnY = 1000000000; float mxY = -1;
        float mnZ = 1000000000; float mxZ = -1;
        List<float> mnx_mxx, mny_mxy, mnz_mxz;
        public string projectPath;
        public List<bullet> bullets = new List<bullet>();
        public void Initialize()
        {
             projectPath = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            sh = new Shader(projectPath + "\\Shaders\\SimpleVertexShader.vertexshader", projectPath + "\\Shaders\\SimpleFragmentShader.fragmentshader");
            shoot = new Texture(projectPath + "\\Textures\\gunshot.png", 5);
            shader2D = new Shader(projectPath + "\\Shaders\\2Dvertex.vertexshader", projectPath + "\\Shaders\\2Dfrag.fragmentshader");

            float[] shootvertices = {
                -1,1,0,
                1,0,0,
                0,0,
                0,1,0,
                1,-1,0,
                0,0,1,
                1,1,
                0,1,0,
                -1,-1,0,
                0,0,1,
                0,1,
                0,1,0,
                1,1,0,
                0,0,1,
                1,0,
                0,1,0,
                -1,1,0,
                0,1,0,
                0,0,
                0,1,0,
                1,-1,0,
                1,0,0,
                1,1,
                0,1,0
            };
            ShootID = GPU.GenerateBuffer(shootvertices);

            Gl.glClearColor(0, 0, 0.4f, 1);

            //camera
            cam = new Camera();
            cam.Reset(0, 0, 0, -15, 55, -1217, 0, 1, 0);
            cam.mAngleX = 2.8f;

            ProjectionMatrix = cam.GetProjectionMatrix();
            ViewMatrix = cam.GetViewMatrix();

            gun = new Model3D();
            gun.LoadFile(projectPath + "\\ModelFiles\\hands with gun", 2, "gun.obj");

            playerPos = cam.GetCameraTarget();
            playerPos.y += 7;

            defaultY = cam.GetCameraTarget().y;

            gun.scalematrix = glm.scale(new mat4(1), new vec3(0.2f, 0.2f, 0.2f));
            gun.rotmatrix = glm.rotate(3.1412f, new vec3(0, 1, 0));
            gun.transmatrix = glm.translate(new mat4(1), playerPos);

           
            sh.UseShader();
            DataID = Gl.glGetUniformLocation(sh.ID, "data");
            vec2 data = new vec2(40000, 50);
            Gl.glUniform2fv(DataID, 1, data.to_array());
            int LightPositionID = Gl.glGetUniformLocation(sh.ID, "LightPosition_worldspace");
            vec3 lightPosition = new vec3(1.0f, 1000f, 4.0f);
            Gl.glUniform3fv(LightPositionID, 1, lightPosition.to_array());
            AmbientLightID = Gl.glGetUniformLocation(sh.ID, "ambientLight");
            vec3 ambientLight = new vec3(0.2f, 0.18f, 0.18f);
            Gl.glUniform3fv(AmbientLightID, 1, ambientLight.to_array());
            EyePositionID = Gl.glGetUniformLocation(sh.ID, "EyePosition_worldspace");

            d = new Texture(projectPath + "\\Textures\\desertsky_dn.jpg", 1, false);
            u = new Texture(projectPath + "\\Textures\\desertsky_up.jpg", 2, false);
            l = new Texture(projectPath + "\\Textures\\desertsky_lf_1.jpg", 4, false);
            r = new Texture(projectPath + "\\Textures\\desertsky_rt_1.jpg", 5, false);
            f = new Texture(projectPath + "\\Textures\\desertsky_ft.jpg", 3, false);
            b = new Texture(projectPath + "\\Textures\\desertsky_bk.jpg", 6, false);

            float[] square = {                    /*x,y,z       x,y,z*/
                -1,0,1,  1,0,0, 0,0,       //d   /*-250,0,250   250,0,0*/
                1,0,1,   1,0,0, 1,0,       //u   /*250,0,250    250,0,0*/
                -1,0,-1, 1,0,0, 0,1,       //l   /*-250,0,-250  250,0,0*/
                1,0,1,   1,0,0,  1,0,      //r   /*250,0,250    250,0,0*/
                -1,0,-1, 1,0,0, 0,1,       //f   /*-250,0,-250  250,0,0*/
                1,0,-1,   1,0,0, 1,1       //b   /*250,0,-250   250,0,0*/
            };
            sqID = GPU.GenerateBuffer(square);
            down = glm.scale(new mat4(1), new vec3(7000, 7000, 7000));

            up = MathHelper.MultiplyMatrices(new List<mat4>(){
                //glm.rotate((float)(180.0f / 180.0f * Math.PI), new vec3(1, 0,0)),
                glm.translate(new mat4(1), new vec3(0,1f,0)),
                glm.scale(new mat4(1), new vec3(7000, 11000, 7000))  });

            left = MathHelper.MultiplyMatrices(new List<mat4>(){
                glm.rotate((float)(90.0f / 180.0f * Math.PI), new vec3(0, 0,1)),
                glm.translate(new mat4(1), new vec3(-1f,1f,0)),
                glm.scale(new mat4(1), new vec3(7000, 7000, 7000))  });


            right = MathHelper.MultiplyMatrices(new List<mat4>(){
                glm.rotate((float)(90.0f / 180.0f * Math.PI), new vec3(0, 0, 1)),
                glm.translate(new mat4(1), new vec3(1f,1f,0)),
                glm.scale(new mat4(1), new vec3(7000, 7000, 7000))});


            front = MathHelper.MultiplyMatrices(new List<mat4>(){
                glm.rotate((float)(90.0f / 180.0f * Math.PI), new vec3(1, 0, 0)),
                glm.translate(new mat4(1), new vec3(0,1f,-1f)),
                glm.scale(new mat4(1), new vec3(7000, 7000, 7000))});


            back = MathHelper.MultiplyMatrices(new List<mat4>(){
                glm.rotate((float)(90.0f / 180.0f * Math.PI), new vec3(1, 0, 0)),
                glm.translate(new mat4(1), new vec3(0,1f,1f)),
                glm.scale(new mat4(1), new vec3(7000, 7000, 7000))});

            enemy1 = new Enemy(projectPath + "\\ModelFiles\\animated\\md2LOL\\zombie1.md2", new vec3(1000, 0, -1000),true);
            enemy2 = new Enemy(projectPath + "\\ModelFiles\\animated\\md2LOL\\zombie1.md2", new vec3(1700, 0, -1500),true);
            enemy3 = new Enemy(projectPath + "\\ModelFiles\\animated\\md2LOL\\zombie1.md2", new vec3(2400, 0, -2000),true);
            enemy4 = new Enemy(projectPath + "\\ModelFiles\\animated\\md2LOL\\zombie1.md2", new vec3(3100, 0, -2500),true);
            enemy5 = new Enemy(projectPath + "\\ModelFiles\\animated\\md2LOL\\zombie1.md2", new vec3(3800, 0, -3000), true);
            samourai = new Enemy(projectPath + "\\ModelFiles\\animated\\md2\\samourai\\Samourai.md2", new vec3(300, 70, -300), false);

         building = new Model3D();
         building.LoadFile(projectPath + "\\ModelFiles\\static\\building", 2, "Building 02.obj");
         building.scalematrix = glm.scale(new mat4(1), new vec3(400f, 400f, 400f));
         building.transmatrix = glm.translate(new mat4(1), new vec3(-6500, 140, -1000));
         building.rotmatrix = glm.rotate((float)(90.0f / 180.0f * Math.PI), new vec3(0, 1, 0));
      
            
          for (int i = 0;i < building.meshes.Count; ++i)
           {
               for(int j = 0;j < building.meshes[i].vertices.Count;++j)
               {
                   if (building.meshes[i].vertices[j].x < mnX) mnX = building.meshes[i].vertices[j].x;
                   if (building.meshes[i].vertices[j].x > mxX) mxX = building.meshes[i].vertices[j].x;
                   if (building.meshes[i].vertices[j].y < mnY) mnY = building.meshes[i].vertices[j].y;
                   if (building.meshes[i].vertices[j].y > mxY) mxY = building.meshes[i].vertices[j].y;
                   if (building.meshes[i].vertices[j].z < mnZ) mnZ = building.meshes[i].vertices[j].z;
                   if (building.meshes[i].vertices[j].z > mxZ) mxZ = building.meshes[i].vertices[j].z;
               }
           }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            for(int i = 0;i < 15; ++i)
            {
                trees.Add(new Model3D());
                trees[i].LoadFile(projectPath + "\\ModelFiles\\static\\tree\\Tree", i, "Tree.obj");
                trees[i].scalematrix = glm.scale(new mat4(1), new vec3(250f, 250f, 250f));
                trees[i].transmatrix = glm.translate(new mat4(1), new vec3(2000, 0, 100*(i - 3)));
            }

            stam = new Model3D();
            stam.LoadFile(projectPath + "\\ModelFiles\\static\\vegetation\\canopy", 2, "stam.obj");
            stam.transmatrix = glm.translate(new mat4(1), new vec3(100, 0, -1850));
            stam.scalematrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < stam.meshes.Count; ++i)
            {
                for (int j = 0; j < stam.meshes[i].vertices.Count; ++j)
                {
                    if (stam.meshes[i].vertices[j].x < mnX) mnX = stam.meshes[i].vertices[j].x;
                    if (stam.meshes[i].vertices[j].x > mxX) mxX = stam.meshes[i].vertices[j].x;
                    if (stam.meshes[i].vertices[j].y < mnY) mnY = stam.meshes[i].vertices[j].y;
                    if (stam.meshes[i].vertices[j].y > mxY) mxY = stam.meshes[i].vertices[j].y;
                    if (stam.meshes[i].vertices[j].z < mnZ) mnZ = stam.meshes[i].vertices[j].z;
                    if (stam.meshes[i].vertices[j].z > mxZ) mxZ = stam.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            blad = new Model3D();
            blad.LoadFile(projectPath + "\\ModelFiles\\static\\vegetation\\canopy", 2, "blad.obj");
            blad.transmatrix = glm.translate(new mat4(1), new vec3(100, 0, -1850));
            blad.scalematrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < blad.meshes.Count; ++i)
            {
                for (int j = 0; j < blad.meshes[i].vertices.Count; ++j)
                {
                    if (blad.meshes[i].vertices[j].x < mnX) mnX = blad.meshes[i].vertices[j].x;
                    if (blad.meshes[i].vertices[j].x > mxX) mxX = blad.meshes[i].vertices[j].x;
                    if (blad.meshes[i].vertices[j].y < mnY) mnY = blad.meshes[i].vertices[j].y;
                    if (blad.meshes[i].vertices[j].y > mxY) mxY = blad.meshes[i].vertices[j].y;
                    if (blad.meshes[i].vertices[j].z < mnZ) mnZ = blad.meshes[i].vertices[j].z;
                    if (blad.meshes[i].vertices[j].z > mxZ) mxZ = blad.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            stam2 = new Model3D();
            stam2.LoadFile(projectPath + "\\ModelFiles\\static\\vegetation\\canopy", 2, "stam.obj");
            stam2.transmatrix = glm.translate(new mat4(1), new vec3(2500, 0, -5850));
            stam2.scalematrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < stam2.meshes.Count; ++i)
            {
                for (int j = 0; j < stam2.meshes[i].vertices.Count; ++j)
                {
                    if (stam2.meshes[i].vertices[j].x < mnX) mnX = stam2.meshes[i].vertices[j].x;
                    if (stam2.meshes[i].vertices[j].x > mxX) mxX = stam2.meshes[i].vertices[j].x;
                    if (stam2.meshes[i].vertices[j].y < mnY) mnY = stam2.meshes[i].vertices[j].y;
                    if (stam2.meshes[i].vertices[j].y > mxY) mxY = stam2.meshes[i].vertices[j].y;
                    if (stam2.meshes[i].vertices[j].z < mnZ) mnZ = stam2.meshes[i].vertices[j].z;
                    if (stam2.meshes[i].vertices[j].z > mxZ) mxZ = stam2.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            blad2 = new Model3D();
            blad2.LoadFile(projectPath + "\\ModelFiles\\static\\vegetation\\canopy", 2, "blad.obj");
            blad2.transmatrix = glm.translate(new mat4(1), new vec3(2500, 0, -5850));
            blad2.scalematrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < blad2.meshes.Count; ++i)
            {
                for (int j = 0; j < blad2.meshes[i].vertices.Count; ++j)
                {
                    if (blad2.meshes[i].vertices[j].x < mnX) mnX = blad2.meshes[i].vertices[j].x;
                    if (blad2.meshes[i].vertices[j].x > mxX) mxX = blad2.meshes[i].vertices[j].x;
                    if (blad2.meshes[i].vertices[j].y < mnY) mnY = blad2.meshes[i].vertices[j].y;
                    if (blad2.meshes[i].vertices[j].y > mxY) mxY = blad2.meshes[i].vertices[j].y;
                    if (blad2.meshes[i].vertices[j].z < mnZ) mnZ = blad2.meshes[i].vertices[j].z;
                    if (blad2.meshes[i].vertices[j].z > mxZ) mxZ = blad2.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            car = new Model3D();
            car.LoadFile(projectPath + "\\ModelFiles\\static\\car", 3, "dpv.obj");
           car.transmatrix = glm.translate(new mat4(1), new vec3(-2500, 0, -5850));
           car.scalematrix = glm.scale(new mat4(1), new vec3(5f, 5f, 5f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < car.meshes.Count; ++i)
            {
                for (int j = 0; j < car.meshes[i].vertices.Count; ++j)
                {
                    if (car.meshes[i].vertices[j].x < mnX) mnX = car.meshes[i].vertices[j].x;
                    if (car.meshes[i].vertices[j].x > mxX) mxX = car.meshes[i].vertices[j].x;
                    if (car.meshes[i].vertices[j].y < mnY) mnY = car.meshes[i].vertices[j].y;
                    if (car.meshes[i].vertices[j].y > mxY) mxY = car.meshes[i].vertices[j].y;
                    if (car.meshes[i].vertices[j].z < mnZ) mnZ = car.meshes[i].vertices[j].z;
                    if (car.meshes[i].vertices[j].z > mxZ) mxZ = car.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            for (int i = 0; i < 10; ++i)
            {
                grass.Add(new Model3D());
                grass[i].LoadFile(projectPath + "\\ModelFiles\\static\\vegetation\\varen", 2, "varen.obj");
                grass[i].transmatrix = glm.translate(new mat4(1), new vec3(-500, 20, -5000*(i-1)));
                grass[i].scalematrix = glm.scale(new mat4(1), new vec3(1f, 1f, 1f));
                //grass[i].rotmatrix = glm.rotate((float)(270.0f / 180.0f * Math.PI), new vec3(1, 0, 0));
            }
           
            rock = new Model3D();
            rock.LoadFile(projectPath + "\\ModelFiles\\static\\Rocks", 2, "rock_pack.obj");
            rock.transmatrix = glm.translate(new mat4(1), new vec3(-250, 20, 6900));
            rock.scalematrix = glm.scale(new mat4(1), new vec3(1000f, 1000f, 1000f));

            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < rock.meshes.Count; ++i)
            {
                for (int j = 0; j < rock.meshes[i].vertices.Count; ++j)
                {
                    if (rock.meshes[i].vertices[j].x < mnX) mnX = rock.meshes[i].vertices[j].x;
                    if (rock.meshes[i].vertices[j].x > mxX) mxX = rock.meshes[i].vertices[j].x;
                    if (rock.meshes[i].vertices[j].y < mnY) mnY = rock.meshes[i].vertices[j].y;
                    if (rock.meshes[i].vertices[j].y > mxY) mxY = rock.meshes[i].vertices[j].y;
                    if (rock.meshes[i].vertices[j].z < mnZ) mnZ = rock.meshes[i].vertices[j].z;
                    if (rock.meshes[i].vertices[j].z > mxZ) mxZ = rock.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            rock1 = new Model3D();
            rock1.LoadFile(projectPath + "\\ModelFiles\\static\\Rocks", 2, "rock_pack.obj");
            rock1.transmatrix = glm.translate(new mat4(1), new vec3(2000, 20, -6900));
            rock1.scalematrix = glm.scale(new mat4(1), new vec3(1000f, 1000f, 1000f));
            mnX = 1000000000; mxX = -1;
            mnY = 1000000000; mxY = -1;
            mnZ = 1000000000; mxZ = -1;
            for (int i = 0; i < rock1.meshes.Count; ++i)
            {
                for (int j = 0; j < rock1.meshes[i].vertices.Count; ++j)
                {
                    if (rock1.meshes[i].vertices[j].x < mnX) mnX = rock1.meshes[i].vertices[j].x;
                    if (rock1.meshes[i].vertices[j].x > mxX) mxX = rock1.meshes[i].vertices[j].x;
                    if (rock1.meshes[i].vertices[j].y < mnY) mnY = rock1.meshes[i].vertices[j].y;
                    if (rock1.meshes[i].vertices[j].y > mxY) mxY = rock1.meshes[i].vertices[j].y;
                    if (rock1.meshes[i].vertices[j].z < mnZ) mnZ = rock1.meshes[i].vertices[j].z;
                    if (rock1.meshes[i].vertices[j].z > mxZ) mxZ = rock1.meshes[i].vertices[j].z;
                }
            }
            mnx_mxx.Add(mnX); mnx_mxx.Add(mxX);
            mny_mxy.Add(mnY); mny_mxy.Add(mxY);
            mnz_mxz.Add(mnZ); mnz_mxz.Add(mxZ);

            //street = new Model3D();
            //street.LoadFile(projectPath + "\\ModelFiles\\street", 2, "Straße-Template.obj");

            //2D control
            hp = new Texture(projectPath + "\\Textures\\HP.bmp", 9);
            bhp = new Texture(projectPath + "\\Textures\\BackHP.bmp", 10);
            float[] squarevertices = {
                -1,1,0,
                0,0,

                1,-1,0,
                1,1,

                -1,-1,0,
                0,1,

                1,1,0,
                1,0,

                -1,1,0,
                0,0,

                1,-1,0,
                1,1
            };
            hpID = GPU.GenerateBuffer(squarevertices);
            backhealthbar = MathHelper.MultiplyMatrices(new List<mat4>(){
                glm.scale(new mat4(1), new vec3(0.5f,0.1f, 1)), glm.translate(new mat4(1),new vec3(-0.5f,0.9f,0)) });
            healthbar = MathHelper.MultiplyMatrices(new List<mat4>() {
                glm.scale(new mat4(1), new vec3(0.48f, 0.1f, 1)), glm.translate(new mat4(1), new vec3(-0.5f, 0.9f, 0)) });
            shader2D.UseShader();
            modelLoc = Gl.glGetUniformLocation(shader2D.ID, "model");

            scalef = 1;
            
            //shader configurations
            transID = Gl.glGetUniformLocation(sh.ID, "trans");
            projID = Gl.glGetUniformLocation(sh.ID, "projection");
            viewID = Gl.glGetUniformLocation(sh.ID, "view");

            //enabling Depth test
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LESS);
            c = timer;
        }
        float defaultY;

        public void Draw()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            //use 3D shader
            sh.UseShader();
            //draw 3D model
            playerPos = cam.GetCameraTarget();
            playerPos.y -= 2.5f;
            gun.rotmatrix = glm.rotate(3.1412f + cam.mAngleX, new vec3(0, 1, 0));//* glm.rotate(cam.mAngleY, new vec3(1, 0, 0));
            gun.transmatrix = glm.translate(new mat4(1), playerPos);
            cam.UpdateViewMatrix();
            ProjectionMatrix = cam.GetProjectionMatrix();
            ViewMatrix = cam.GetViewMatrix();

            //send shader values
            Gl.glUniformMatrix4fv(projID, 1, Gl.GL_FALSE, ProjectionMatrix.to_array());
            Gl.glUniformMatrix4fv(viewID, 1, Gl.GL_FALSE, ViewMatrix.to_array());
            Gl.glUniform3fv(EyePositionID, 1, cam.GetCameraPosition().to_array());

            gun.Draw(transID);
            //city.Draw(transID);

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, ShootID);
            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 11 * sizeof(float), (IntPtr)0);

            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 11 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 11 * sizeof(float), (IntPtr)(6 * sizeof(float)));

            Gl.glEnableVertexAttribArray(3);
            Gl.glVertexAttribPointer(3, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 11 * sizeof(float), (IntPtr)(8 * sizeof(float)));

            shoot.Bind();
            vec3 shootpos = cam.GetCameraTarget();
            shootpos.y -= 1.5f;
            shootpos += cam.GetLookDirection() * 8;

            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, MathHelper.MultiplyMatrices(new List<mat4>()
            { glm.scale(new mat4(1),new vec3(2+(float)c/10,2 + (float)c / 10, 2 + (float)c / 10)),
                glm.rotate(cam.mAngleX, new vec3(0, 1, 0)),
                glm.rotate((float)c/10, new vec3(0, 0, 1)),
                glm.translate(new mat4(1),shootpos),
            }).to_array());
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            if (draw)
            {
                cam.mAngleY -= 0.01f;
                Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);
                c--;
                shooting = true;
                if (c < 0)
                {
                    cam.mAngleY = 0;
                    c = timer;
                    draw = false;
                    SoundPlayer soundPlayer;
                    soundPlayer = new SoundPlayer();
                    soundPlayer.SoundLocation = projectPath + "\\Shooting An MP5-SoundBible.com-704967207.wav";
                    soundPlayer.Play();
                }
            }

            //bullets.Draw(transID);
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, down.to_array());
            Gl.glUniformMatrix4fv(viewID, 1, Gl.GL_FALSE, cam.GetViewMatrix().to_array());
            Gl.glUniformMatrix4fv(projID, 1, Gl.GL_FALSE, cam.GetProjectionMatrix().to_array());

            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)0);

            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));

            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, sqID);
            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)0);

            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(3 * sizeof(float)));

            Gl.glEnableVertexAttribArray(2);
            Gl.glVertexAttribPointer(2, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 8 * sizeof(float), (IntPtr)(6 * sizeof(float)));
            //enemyPos = new vec3(xpos, ypos, zpos);
            //sky box
            d.Bind();
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);

            u.Bind();
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, up.to_array());
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);

            l.Bind();
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, left.to_array());
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);

            r.Bind();
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, right.to_array());
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);


            f.Bind();
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, front.to_array());
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);


            b.Bind();
            Gl.glUniformMatrix4fv(transID, 1, Gl.GL_FALSE, back.to_array());
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);
            Gl.glDisable(Gl.GL_BLEND);
            enemy1.Draw(transID);
            enemy2.Draw(transID);
            enemy3.Draw(transID);
            enemy4.Draw(transID);
            enemy5.Draw(transID);
            samourai.Draw(transID);
            building.Draw(transID);
            //for(int i = 0;i < buildings.Count;++i)
            // buildings[i].Draw(transID);
            for(int i = 0;i < trees.Count;++i)
            {
                trees[i].Draw(transID);
            }
            for(int i = 0;i < grass.Count;++i)
             grass[i].Draw(transID);
            blad.Draw(transID);
            stam.Draw(transID);
            blad2.Draw(transID);
            stam2.Draw(transID);
            car.Draw(transID);
           
            rock.Draw(transID);
            rock1.Draw(transID);
            for (int i = 0; i < bullets.Count; ++i)
            {
                bullets[i].Draw(transID);
            }
            //2D controls
            //disable depthtest (no z value in 2D)
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            //use 2D shader
            shader2D.UseShader();
            //draw 2D square
            Gl.glBindBuffer(Gl.GL_ARRAY_BUFFER, hpID);
            Gl.glEnableVertexAttribArray(0);
            Gl.glVertexAttribPointer(0, 3, Gl.GL_FLOAT, Gl.GL_FALSE, 5 * sizeof(float), (IntPtr)0);
            Gl.glEnableVertexAttribArray(1);
            Gl.glVertexAttribPointer(1, 2, Gl.GL_FLOAT, Gl.GL_FALSE, 5 * sizeof(float), (IntPtr)(3 * sizeof(float)));
            //background of healthbar
            Gl.glUniformMatrix4fv(modelLoc, 1, Gl.GL_FALSE, backhealthbar.to_array());
            bhp.Bind();
            //backhealthbar = glm.translate(new mat4(1), enemyPos);
            Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
        }
        int timer = 5;
        int c;
        public bool draw = false;
        public bool shooting = false;
        public void Update(float deltaTime)
        {
            enemy1.update(cam.GetCameraPosition(), cam.mAngleX);
            enemy2.update(cam.GetCameraPosition(), cam.mAngleX);
            enemy3.update(cam.GetCameraPosition(), cam.mAngleX);
            enemy4.update(cam.GetCameraPosition(), cam.mAngleX);
            enemy5.update(cam.GetCameraPosition(), cam.mAngleX);
            samourai.update(cam.GetCameraPosition(), cam.mAngleX);
            for (int i = 0;i < bullets.Count; ++i) 
               bullets[i].update();

            if (enemy1.attackStarted || enemy2.attackStarted || enemy3.attackStarted
                || enemy1.attackStarted || enemy1.attackStarted || samourai.attackStarted)
            {

                Gl.glDisable(Gl.GL_DEPTH_TEST);
                //decrease the health bar by scaling down the 2D front square
                scalef -= 0.0001f;
                if (scalef < 0)
                    scalef = 0;
                healthbar = MathHelper.MultiplyMatrices(new List<mat4>() {
                     glm.scale(new mat4(1), new vec3(0.48f*scalef, 0.1f, 1)),
                    glm.translate(new mat4(1), new vec3(-0.5f-((1-scalef)*0.48f), 0.9f, 0)) });
                Gl.glUniformMatrix4fv(modelLoc, 1, Gl.GL_FALSE, healthbar.to_array());
                hp.Bind();
                //draw front health bar
                Gl.glDrawArrays(Gl.GL_TRIANGLES, 0, 6);
                //re-enable the depthtest to draw the other 3D objects in the scene
                Gl.glEnable(Gl.GL_DEPTH_TEST);
            }
         }
        
        public void SendLightData(float red, float green, float blue, float attenuation, float specularExponent)
        {
            vec3 ambientLight = new vec3(red, green, blue);
            Gl.glUniform3fv(AmbientLightID, 1, ambientLight.to_array());
            vec2 data = new vec2(attenuation, specularExponent);
            Gl.glUniform2fv(DataID, 1, data.to_array());
        }
        public void CleanUp()
        {
            sh.DestroyShader();
        }
    }
}
