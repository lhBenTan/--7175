using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 滤光片点胶
{
    public class OffsetRotate
    {
        public static int HD_angle = 0;
        public static int IR_angle = 0;

        public static float defaultX = 0;
        public static float defaultY = 0;
        public static float actualX = 0;
        public static float actualY = 0;

        public static float[] Rotate()
        {
            float[] ret = new float[3];

            float offsetX = actualX - defaultX;
            float offsetY = actualY - defaultY; 

            float r = (float)Math.Sqrt(offsetX * offsetX + offsetY * offsetY);

            if (r == 0)
            {
                ret[0] = 0;
                ret[1] = 0;
                ret[2] = 0;

                return ret;
            }
            
            int angle = (int)(Math.Acos(-offsetY / r) * 180 / Math.PI);

            if (offsetX < 0) angle = 360 - angle;

            angle -= HD_angle - IR_angle;

            ret[0] = IR_angle;
            ret[1] = (float)(r * Math.Sin(angle * Math.PI / 180));
            ret[2] = (float)(-r * Math.Cos(angle * Math.PI / 180));
            
            return ret;
        }
    }
}
