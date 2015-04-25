using GlmNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class FlyingCamera
    {
        int pCur; // For mosue rotation
        int iForw, iBack, iLeft, iRight;

        vec3 vEye, vView, vUp;
        float fSpeed;
        float fSensitivity; // How many degrees to rotate per pixel moved by mouse (nice value is 0.10)

        // Main functions
        //void rotateWithMouse();
        //void update();
        //mat4 look();

        //void setMovingKeys(int a_iForw, int a_iBack, int a_iLeft, int a_iRight);
        //void resetMouse();

        public  FlyingCamera(vec3 a_vEye, vec3 a_vView, vec3 a_vUp, float a_fSpeed, float a_fSensitivity)
        {
            vEye = a_vEye; vView = a_vView; vUp = a_vUp;
            fSpeed = a_fSpeed;
            fSensitivity = a_fSensitivity;
        }

        public  FlyingCamera()
        {
            vEye = new vec3(0.0f, 0.0f, 0.0f);
            vView = new vec3(0.0f, 0.0f, -1.0f);
            vUp = new vec3(0.0f, 1.0f, 0.0f);
            fSpeed = 25.0f;
            fSensitivity = 0.1f;
        }

        // Functions that get viewing angles
        float getAngleX()
        {
            vec3 vDir = new vec3();
            vec3 vDir2 = new vec3();
            vDir = vView - vEye;
            vDir = glm.normalize(vDir);
            vDir2 = vDir; vDir2.y = 0.0f;
            vDir2 = glm.normalize(vDir2);
            float fAngle = (float)Math.Acos(glm.dot(vDir2, vDir)) * (180.0f / (float)Math.PI);
            if (vDir.y < 0) fAngle *= -1.0f;
            return fAngle;
        }

        float getAngleY()
        {
            vec3 vDir = new vec3();
            vDir = vView - vEye; vDir.y = 0.0f;
            glm.normalize(vDir);
            float fAngle = (float)Math.Acos(glm.dot(new vec3(0, 0, -1), vDir)) * (180.0f / (float)Math.PI);
            if (vDir.x < 0) fAngle = 360.0f - fAngle;
            return fAngle;
        }



        public void RotateWithMouse()
        {


        }


//        void CFlyingCamera::resetMouse()
//{
//    RECT rRect; GetWindowRect(appMain.hWnd, &rRect);
//    int iCentX = (rRect.left+rRect.right)>>1,
//        iCentY = (rRect.top+rRect.bottom)>>1;
//    SetCursorPos(iCentX, iCentY);
//}
//        void CFlyingCamera::setMovingKeys(int a_iForw, int a_iBack, int a_iLeft, int a_iRight)
//{
//    iForw = a_iForw;
//    iBack = a_iBack;
//    iLeft = a_iLeft;
//    iRight = a_iRight;
//}

        public void Update()
        {
            RotateWithMouse();
            vec3 vMove = new vec3();
            vec3 vStrafe = new vec3();
            vec3 vMoveBy = new vec3();
            // Get view direction
            vMove = vView - vEye;
            vMove = glm.normalize(vMove);
            vMove *= fSpeed;

            vStrafe = glm.cross(vView - vEye, vUp);
            vStrafe = glm.normalize(vStrafe);
            vStrafe *= fSpeed;

            int iMove = 0;

            // Get vector of move
            //if(Keys::key(iForw))vMoveBy += vMove*appMain.sof(1.0f);
            //if(Keys::key(iBack))vMoveBy -= vMove*appMain.sof(1.0f);
            //if(Keys::key(iLeft))vMoveBy -= vStrafe*appMain.sof(1.0f);
            //if(Keys::key(iRight))vMoveBy += vStrafe*appMain.sof(1.0f);
            vEye += vMoveBy; vView += vMoveBy;
        }

        public mat4 Look()
        {
            return glm.lookAt(vEye, vView, vUp);
        }

    }
}
