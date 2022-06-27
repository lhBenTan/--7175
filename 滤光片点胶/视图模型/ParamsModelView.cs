using DevExpress.Mvvm;
using HandyControl.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace 滤光片点胶
{
    public class ParamsModelView : ViewModelBase
    {
        public ParamsModelView(int id)
        {
            AlgID = id;
            filename = "AlgConfig" + AlgID.ToString() + ".xml";

            CommboxID = new int[10];
            for (int i = 0; i < 10; i++)
            {
                CommboxID[i] = i;
            }
        }

        /// <summary>
        /// Commbox选项
        /// </summary>
        public int[] CommboxID
        {
            get => GetProperty(() => CommboxID);
            set => SetProperty(() => CommboxID, value);
        }

        #region 配置读写
        public int AlgID;

        string filename = "";
        string filePath = "";

        private void InitErr()
        {
            try
            {
                //获得文件路径
                string localFilePath = "";
                localFilePath = filePath;
                XDocument xdoc = new XDocument();
                XDeclaration xdec = new XDeclaration("1.0", "utf-8", "yes");
                xdoc.Declaration = xdec;

                XElement rootEle;
                XElement classEle;
                //XElement childEle;

                //添加根节点
                rootEle = new XElement("AlgConfig");
                xdoc.Add(rootEle);

                classEle = new XElement("SelectedAlg", 0);
                rootEle.Add(classEle);

                classEle = new XElement("ShowMode", 0);
                rootEle.Add(classEle);

                classEle = new XElement("LocThresh", 128);
                rootEle.Add(classEle);

                classEle = new XElement("MaxRadius", 100);
                rootEle.Add(classEle);

                classEle = new XElement("MinRadius", 0);
                rootEle.Add(classEle);

                classEle = new XElement("LensRadius", 0);
                rootEle.Add(classEle);

                classEle = new XElement("HoleRadius", 0);
                rootEle.Add(classEle);
                //AutoCover
                classEle = new XElement("AutoCover", 0);
                rootEle.Add(classEle);

                classEle = new XElement("CoverRadiusMax", 0);
                rootEle.Add(classEle);

                classEle = new XElement("CoverRadiusMin", 0);
                rootEle.Add(classEle);

                classEle = new XElement("HighCut", 0);
                rootEle.Add(classEle);

                classEle = new XElement("LowCut", 0);
                rootEle.Add(classEle);

                classEle = new XElement("sSize", 0);
                rootEle.Add(classEle);

                classEle = new XElement("mSize", 0);
                rootEle.Add(classEle);

                classEle = new XElement("lSize", 0);
                rootEle.Add(classEle);
                
                classEle = new XElement("Radius", 128);
                rootEle.Add(classEle);

                classEle = new XElement("nMaxRadius", 100);
                rootEle.Add(classEle);

                classEle = new XElement("nMinRadius", 0);
                rootEle.Add(classEle);

                classEle = new XElement("D1thresh", 128);
                rootEle.Add(classEle);

                classEle = new XElement("D1SizeMax", 100);
                rootEle.Add(classEle);

                classEle = new XElement("D1SizeMin", 0);
                rootEle.Add(classEle);

                classEle = new XElement("D2AdapSize", 2);
                rootEle.Add(classEle);

                classEle = new XElement("D2AdapC", 2);
                rootEle.Add(classEle);

                classEle = new XElement("D2RoundnessMin", 0.5);
                rootEle.Add(classEle);

                classEle = new XElement("D2RectangularityMin", 0.5);
                rootEle.Add(classEle);

                classEle = new XElement("D2sizeMax", 100);
                rootEle.Add(classEle);

                classEle = new XElement("D2sizeMin", 0);
                rootEle.Add(classEle);

                classEle = new XElement("WorkMode", 0);
                rootEle.Add(classEle);

                classEle = new XElement("X_Defult", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Y_Defult", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Scale", 0);
                rootEle.Add(classEle);

                classEle = new XElement("OffsetMax", 0);
                rootEle.Add(classEle);

                classEle = new XElement("X_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("Y_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("XY_flip", 0);
                rootEle.Add(classEle);

                classEle = new XElement("R_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("G_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("B_min", 0);
                rootEle.Add(classEle);

                classEle = new XElement("R_max", 0);
                rootEle.Add(classEle);

                classEle = new XElement("G_max", 0);
                rootEle.Add(classEle);

                classEle = new XElement("B_max", 0);
                rootEle.Add(classEle);

                xdoc.Save(localFilePath);
            }
            catch
            {
                Growl.Error("[" + filePath + "]生成失败！");
            }
        }

        public int Init(string path)
        {
            int ret = 0;
            filePath = path + "/" + filename ;
            if (XmlHelper.Exists(path, filename))
            {
                try
                {
                    XDocument Config = XDocument.Load(filePath);

                    SelectedAlg = int.Parse(Config.Descendants("SelectedAlg").ElementAt(0).Value);

                    ShowMode = int.Parse(Config.Descendants("ShowMode").ElementAt(0).Value);

                    LocThresh = int.Parse(Config.Descendants("LocThresh").ElementAt(0).Value);
                    MaxRadius = int.Parse(Config.Descendants("MaxRadius").ElementAt(0).Value);
                    MinRadius = int.Parse(Config.Descendants("MinRadius").ElementAt(0).Value);

                    Radius = int.Parse(Config.Descendants("Radius").ElementAt(0).Value);
                    nMaxRadius = int.Parse(Config.Descendants("nMaxRadius").ElementAt(0).Value);
                    nMinRadius = int.Parse(Config.Descendants("nMinRadius").ElementAt(0).Value);

                    D1thresh = int.Parse(Config.Descendants("D1thresh").ElementAt(0).Value);
                    D1SizeMax = int.Parse(Config.Descendants("D1SizeMax").ElementAt(0).Value);
                    D1SizeMin = int.Parse(Config.Descendants("D1SizeMin").ElementAt(0).Value);

                    D2AdapSize = int.Parse(Config.Descendants("D2AdapSize").ElementAt(0).Value);
                    D2AdapC = int.Parse(Config.Descendants("D2AdapC").ElementAt(0).Value);
                    D2RoundnessMin = float.Parse(Config.Descendants("D2RoundnessMin").ElementAt(0).Value);
                    D2RectangularityMin = float.Parse(Config.Descendants("D2RectangularityMin").ElementAt(0).Value);
                    D2sizeMax = int.Parse(Config.Descendants("D2sizeMax").ElementAt(0).Value);
                    D2sizeMin = int.Parse(Config.Descendants("D2sizeMin").ElementAt(0).Value);

                    WorkMode = int.Parse(Config.Descendants("WorkMode").ElementAt(0).Value);

                    X_Defult = float.Parse(Config.Descendants("X_Defult").ElementAt(0).Value);
                    Y_Defult = float.Parse(Config.Descendants("Y_Defult").ElementAt(0).Value);

                    Scale = float.Parse(Config.Descendants("Scale").ElementAt(0).Value);
                    OffsetMax = float.Parse(Config.Descendants("OffsetMax").ElementAt(0).Value);

                    X_flip = int.Parse(Config.Descendants("X_flip").ElementAt(0).Value);
                    Y_flip = int.Parse(Config.Descendants("Y_flip").ElementAt(0).Value);
                    XY_flip = int.Parse(Config.Descendants("XY_flip").ElementAt(0).Value);

                    R_min = int.Parse(Config.Descendants("R_min").ElementAt(0).Value);
                    G_min = int.Parse(Config.Descendants("G_min").ElementAt(0).Value);
                    B_min = int.Parse(Config.Descendants("B_min").ElementAt(0).Value);

                    R_max = int.Parse(Config.Descendants("R_max").ElementAt(0).Value);
                    G_max = int.Parse(Config.Descendants("G_max").ElementAt(0).Value);
                    B_max = int.Parse(Config.Descendants("B_max").ElementAt(0).Value);

                    //ROI设置
                    LensRadius = int.Parse(Config.Descendants("LensRadius").ElementAt(0).Value);
                    HoleRadius = int.Parse(Config.Descendants("HoleRadius").ElementAt(0).Value);
                    //AutoCover
                    AutoCover = int.Parse(Config.Descendants("AutoCover").ElementAt(0).Value);
                    CoverRadiusMax = int.Parse(Config.Descendants("CoverRadiusMax").ElementAt(0).Value);
                    CoverRadiusMin = int.Parse(Config.Descendants("CoverRadiusMin").ElementAt(0).Value);

                    //差分图切片阈值
                    HighCut = int.Parse(Config.Descendants("HighCut").ElementAt(0).Value);
                    LowCut = int.Parse(Config.Descendants("LowCut").ElementAt(0).Value);

                    //脏污点尺寸
                    sSize = int.Parse(Config.Descendants("sSize").ElementAt(0).Value);
                    mSize = int.Parse(Config.Descendants("mSize").ElementAt(0).Value);
                    lSize = int.Parse(Config.Descendants("lSize").ElementAt(0).Value);

                }
                catch (Exception err)
                {
                    Growl.Error("算法" + AlgID + "配置信息丢失！已重新生成");
                    InitErr();
                }
            }
            else
            {
                Growl.Error("算法" + AlgID + "初始化失败！");
                InitErr();
                ret++;
            }

            return ret;
        }

        #endregion

        #region 绑定参数

        /// <summary>
        /// 被选中算法
        /// </summary>
        public int SelectedAlg
        {
            get => GetProperty(() => SelectedAlg);
            set => SetProperty(() => SelectedAlg, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("SelectedAlg").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }
        
        /// <summary>
        /// 显示模式
        /// </summary>
        public int ShowMode
        {
            get => GetProperty(() => ShowMode);
            set => SetProperty(() => ShowMode, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("ShowMode").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 工作模式
        /// </summary>
        public int WorkMode
        {
            get => GetProperty(() => WorkMode);
            set => SetProperty(() => WorkMode, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("WorkMode").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// X基准中心
        /// </summary>
        public float X_Defult
        {
            get => GetProperty(() => X_Defult);
            set => SetProperty(() => X_Defult, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("X_Defult").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// Y基准中心
        /// </summary>
        public float Y_Defult
        {
            get => GetProperty(() => Y_Defult);
            set => SetProperty(() => Y_Defult, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Y_Defult").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 比例尺
        /// </summary>
        public float Scale
        {
            get => GetProperty(() => Scale);
            set => SetProperty(() => Scale, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Scale").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 补偿上限
        /// </summary>
        public float OffsetMax
        {
            get => GetProperty(() => OffsetMax);
            set => SetProperty(() => OffsetMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("OffsetMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// X方向翻转
        /// </summary>
        public int X_flip
        {
            get => GetProperty(() => X_flip);
            set => SetProperty(() => X_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("X_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// Y方向翻转
        /// </summary>
        public int Y_flip
        {
            get => GetProperty(() => Y_flip);
            set => SetProperty(() => Y_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Y_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// XY翻转
        /// </summary>
        public int XY_flip
        {
            get => GetProperty(() => XY_flip);
            set => SetProperty(() => XY_flip, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("XY_flip").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// R最小值
        /// </summary>
        public int R_min
        {
            get => GetProperty(() => R_min);
            set => SetProperty(() => R_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("R_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// G最小值
        /// </summary>
        public int G_min
        {
            get => GetProperty(() => G_min);
            set => SetProperty(() => G_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("G_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// B最小值
        /// </summary>
        public int B_min
        {
            get => GetProperty(() => B_min);
            set => SetProperty(() => B_min, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("B_min").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// R最大值
        /// </summary>
        public int R_max
        {
            get => GetProperty(() => R_max);
            set => SetProperty(() => R_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("R_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// G最大值
        /// </summary>
        public int G_max
        {
            get => GetProperty(() => G_max);
            set => SetProperty(() => G_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("G_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// B最大值
        /// </summary>
        public int B_max
        {
            get => GetProperty(() => B_max);
            set => SetProperty(() => B_max, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("B_max").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 定位-灰度阈值
        /// </summary>
        public int LocThresh
        {
            get => GetProperty(() => LocThresh);
            set => SetProperty(() => LocThresh, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("LocThresh").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 定位-半径上限
        /// </summary>
        public int MaxRadius
        {
            get => GetProperty(() => MaxRadius);
            set => SetProperty(() => MaxRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("MaxRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 定位-半径下限
        /// </summary>
        public int MinRadius
        {
            get => GetProperty(() => MinRadius);
            set => SetProperty(() => MinRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("MinRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-有效径半径
        /// </summary>
        public int Radius
        {
            get => GetProperty(() => Radius);
            set => SetProperty(() => Radius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("Radius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-屏蔽径上限
        /// </summary>
        public int nMaxRadius
        {
            get => GetProperty(() => nMaxRadius);
            set => SetProperty(() => nMaxRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("nMaxRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 区域-屏蔽径下限
        /// </summary>
        public int nMinRadius
        {
            get => GetProperty(() => nMinRadius);
            set => SetProperty(() => nMinRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("nMinRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-灰度阈值
        /// </summary>
        public int D1thresh
        {
            get => GetProperty(() => D1thresh);
            set => SetProperty(() => D1thresh, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1thresh").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-面积上限
        /// </summary>
        public int D1SizeMax
        {
            get => GetProperty(() => D1SizeMax);
            set => SetProperty(() => D1SizeMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1SizeMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型1-面积下限
        /// </summary>
        public int D1SizeMin
        {
            get => GetProperty(() => D1SizeMin);
            set => SetProperty(() => D1SizeMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D1SizeMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-强度
        /// </summary>
        public int D2AdapSize
        {
            get => GetProperty(() => D2AdapSize);
            set => SetProperty(() => D2AdapSize, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2AdapSize").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-容差
        /// </summary>
        public int D2AdapC
        {
            get => GetProperty(() => D2AdapC);
            set => SetProperty(() => D2AdapC, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2AdapC").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-圆度下限
        /// </summary>
        public float D2RoundnessMin
        {
            get => GetProperty(() => D2RoundnessMin);
            set => SetProperty(() => D2RoundnessMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2RoundnessMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-矩形度下限
        /// </summary>
        public float D2RectangularityMin
        {
            get => GetProperty(() => D2RectangularityMin);
            set => SetProperty(() => D2RectangularityMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2RoundnessMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-面积上限
        /// </summary>
        public int D2sizeMax
        {
            get => GetProperty(() => D2sizeMax);
            set => SetProperty(() => D2sizeMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2sizeMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///类型2-面积下限
        /// </summary>
        public int D2sizeMin
        {
            get => GetProperty(() => D2sizeMin);
            set => SetProperty(() => D2sizeMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("D2sizeMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///镜片半径
        /// </summary>
        public int LensRadius
        {
            get => GetProperty(() => LensRadius);
            set => SetProperty(() => LensRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("LensRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        ///通孔半径
        /// </summary>
        public int HoleRadius
        {
            get => GetProperty(() => HoleRadius);
            set => SetProperty(() => HoleRadius, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("HoleRadius").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 自动屏蔽
        /// </summary>
        public int AutoCover
        {
            get => GetProperty(() => AutoCover);
            set => SetProperty(() => AutoCover, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("AutoCover").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 屏蔽区外边缘
        /// </summary>
        public int CoverRadiusMax
        {
            get => GetProperty(() => CoverRadiusMax);
            set => SetProperty(() => CoverRadiusMax, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("CoverRadiusMax").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 屏蔽区内边缘
        /// </summary>
        public int CoverRadiusMin
        {
            get => GetProperty(() => CoverRadiusMin);
            set => SetProperty(() => CoverRadiusMin, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("CoverRadiusMin").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 差分切片上限
        /// </summary>
        public int HighCut
        {
            get => GetProperty(() => HighCut);
            set => SetProperty(() => HighCut, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("HighCut").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 差分切片下限
        /// </summary>
        public int LowCut
        {
            get => GetProperty(() => LowCut);
            set => SetProperty(() => LowCut, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("LowCut").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 微型脏污尺寸
        /// </summary>
        public int sSize
        {
            get => GetProperty(() => sSize);
            set => SetProperty(() => sSize, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("sSize").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 中型脏污尺寸
        /// </summary>
        public int mSize
        {
            get => GetProperty(() => mSize);
            set => SetProperty(() => mSize, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("mSize").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }

        /// <summary>
        /// 大型脏污尺寸
        /// </summary>
        public int lSize
        {
            get => GetProperty(() => lSize);
            set => SetProperty(() => lSize, value, () =>
            {
                XDocument config = XDocument.Load(filePath);
                config.Descendants("lSize").ElementAt(0).SetValue(value);
                config.Save(filePath);
            });
        }
        #endregion
    }
}
