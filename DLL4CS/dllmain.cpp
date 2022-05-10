// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

#include <opencv.hpp>
#include <opencv2/imgproc/types_c.h>
#include "putText.h"
using namespace cv;
using namespace std;

struct BmpBuf
{
	unsigned char* data_Output;
	int size;
	unsigned char* data_Input;
	int h;
	int w;

};

void DirtyTestG1R1(BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
#pragma region 本地参数
	bool flag = false;
	stringstream str;
	vector<Vec4i> hierarchy;
	vector<vector<Point2i>> contours;
	int _area = 0, err1 = 0, err2 = 0, tmm = -1, num;
	Mat src, temp, dst, labels, stats, centroids, toBlur, Polared, Blured, Diff, show, DePolared, mask, ROI;
	
	str << "G1R1" << endl;

	//src = Mat(data.h, data.w, CV_8UC1, data.data_Input);//
	src = imread("C:\\Users\\Administrator\\Desktop\\7175\\外观\\G1R1\\0324\\8.bmp", 0);
#pragma endregion

#pragma region 参数载入
	int ShowMode = atoi(input_Parameter[0]);//显示模式：0-正常显示 1-定位画面 2-线状脏污 3-点状脏污

	int LocThresh = atoi(input_Parameter[1]);	//定位-灰度阈值
	int MaxRadius = atoi(input_Parameter[2]);	//定位-最大半径
	int MinRadius = atoi(input_Parameter[3]);	//定位-最小半径

	int Radius = atoi(input_Parameter[4]);		//区域-镜片外径
	int eRadius = atoi(input_Parameter[5]);		//区域-有效径

	int D2Thresh = atoi(input_Parameter[7]);	//点状脏污识别-分割阈值
	int D2sizeMax = atoi(input_Parameter[8]);	//点状脏污识别-脏污最大面积
	int D2sizeMin = atoi(input_Parameter[9]);	//点状脏污识别-脏污最小面积


	int  D1AdapSize = atoi(input_Parameter[10]);	//脏污1-强度
	D1AdapSize = D1AdapSize * 2 + 1;
	int  D1AdapC = atoi(input_Parameter[11]);		//脏污1-容差
	float DoR = (float)atof(input_Parameter[12]);		//脏污1-圆度上限 0-1
	int D1SizeMax = atoi(input_Parameter[14]);	//脏污1-面积上限
	int D1SizeMin = atoi(input_Parameter[15]);		//脏污1-面积下限
#pragma endregion

#pragma region 定位
	cvtColor(src, dst, COLOR_GRAY2RGB);

	threshold(src, temp, LocThresh, 255, THRESH_BINARY);
	findContours(temp, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_NONE, Point(0, 0));

	if (ShowMode == 1)
	{
		cvtColor(temp, dst, COLOR_GRAY2RGB);
	}

	for (size_t i = 0; i < contours.size(); i++)
	{
		vector<Point2i> tmpCont;
		convexHull(contours[i], tmpCont);
		double tmpArea = contourArea(tmpCont, 0);
		if (tmpArea > CV_PI*MaxRadius*MaxRadius || tmpArea < CV_PI*MinRadius*MinRadius)continue;
		if (tmpArea > _area)
		{
			_area = (long)tmpArea;
			tmm = i;
		}
	}

#pragma endregion

	if (tmm >= 0)
	{

#pragma region 圆心获取
		RotatedRect ell = fitEllipse(contours[tmm]);
		ellipse(dst, ell, Scalar(255, 0, 0), 2);

		circle(dst, ell.center, MaxRadius, Scalar(0, 255, 0), 2);
		circle(dst, ell.center, MinRadius, Scalar(0, 255, 0), 2);
		circle(dst, ell.center, Radius, Scalar(0, 0, 255), 2);
		circle(dst, ell.center, eRadius, Scalar(0, 255, 255), 2);
#pragma endregion

#pragma region 预处理
		warpPolar(src, Polared, Size(Radius, Radius * 2 * CV_PI), ell.center, Radius, WARP_POLAR_LINEAR);

		toBlur = Polared;
		blur(toBlur, Blured, Size(7, 101));
		Diff = toBlur - Blured;

		//按区域进行放大
		Diff(Rect(0, 0, eRadius, Radius * 2 * CV_PI)) *= 7;
		Diff(Rect(eRadius + 1, 0, Radius - eRadius - 1, Radius * 2 * CV_PI)) *= 4;
		warpPolar(Diff, DePolared, src.size(), ell.center, Radius, WARP_INVERSE_MAP);

		mask = Mat::zeros(DePolared.size(), DePolared.type());
		circle(mask, ell.center, Radius - 1, Scalar(255), -1);

		DePolared.copyTo(ROI, mask);
#pragma endregion

#pragma region 亮线识别

		adaptiveThreshold(ROI, show, 255, ADAPTIVE_THRESH_MEAN_C, THRESH_BINARY, D1AdapSize, D1AdapC);

		if (ShowMode == 2)
		{
			cvtColor(show, dst, COLOR_GRAY2RGB);
		}
		//threshold(DePolared, show, 180, 255, THRESH_BINARY);

		//vector<Vec4i> lines;
		//HoughLinesP(show, lines, 1, CV_PI / 180, 80, 60, 10);
		//for (size_t i = 0; i < lines.size(); i++)
		//{
		//	Vec4i l = lines[i];
		//	line(dst, Point(l[0], l[1]), Point(l[2], l[3]), Scalar(55, 100, 195), 2);
		//}

		Mat kernel = getStructuringElement(MORPH_CROSS, Size(7, 7));
		morphologyEx(show, show, MORPH_DILATE, kernel);

		kernel = getStructuringElement(MORPH_CROSS, Size(5, 5));
		morphologyEx(show, show, MORPH_ERODE, kernel);

		kernel = getStructuringElement(MORPH_CROSS, Size(3, 3));
		morphologyEx(show, show, MORPH_CLOSE, kernel);

		//kernel = getStructuringElement(MORPH_CROSS, Size(5, 5));
		//morphologyEx(show, show, MORPH_OPEN, kernel);

#if 0
		//利用圆度及面积筛选点状脏污
		num = connectedComponentsWithStats(show, labels, stats, centroids);

		//生成随机颜色，用于区分不同连通域
		RNG rng(10086);
		vector<Vec3b> colors;
		for (int i = 0; i < num; i++)
		{
			//使用均匀分布的随机数确定颜色
			Vec3b vec3 = Vec3b(rng.uniform(0, 256), rng.uniform(0, 256), rng.uniform(0, 256));
			colors.push_back(vec3);
		}

		for (size_t i = 0; i < num; i++)
		{
			int x = stats.at<int>(i, CC_STAT_LEFT);
			int y = stats.at<int>(i, CC_STAT_TOP);
			int w = stats.at<int>(i, CC_STAT_WIDTH);
			int h = stats.at<int>(i, CC_STAT_HEIGHT);
			int a = stats.at<int>(i, CC_STAT_AREA);

			if (a * DoL < CV_PI *  w * h)
			{
				rectangle(dst, Point(x, y), Point(x + w, y + h), Scalar(255, 0, 0));
			}

			// 外接矩形
			Rect rect(x, y, w, h);
			rectangle(dst, rect, colors[i], 1, 8, 0);
		}
#endif // 0

		findContours(show, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_NONE, Point(0, 0));

		for (size_t i = 0; i < contours.size(); i++)
		{
			RotatedRect box = minAreaRect(contours[i]);

			double area = contourArea(contours[i]);
			double len = arcLength(contours[i], 1);
			double roundness = (4 * CV_PI*area) / (len*len);

			float w = box.size.width;
			float h = box.size.height;
			float a = box.size.area();

			//float minrectArea = w * h;
			//float Rectangularity;
			//if (minrectArea == 0)
			//{
			//	Rectangularity = 0;
			//}
			//else
			//{
			//	Rectangularity = area / minrectArea;
			//}

			if (w < 1 || h < 1)continue;
			if (a < D1SizeMin || a>D1SizeMax)continue;

			if (roundness < DoR)
			{
				Point2f rect[4];
				box.points(rect);

				for (size_t i = 0; i < 4; i++)
				{
					line(dst, rect[i], rect[(i + 1) % 4], Scalar(255, 0, 0), 1);
				}

				err1++;
			}
		}
#pragma endregion

#pragma region 落尘识别
		threshold(ROI, show, D2Thresh, 255, THRESH_BINARY);

		if (ShowMode == 3)
		{
			cvtColor(show, dst, COLOR_GRAY2RGB);
		}

		kernel = getStructuringElement(MORPH_CROSS, Size(3, 3));
		morphologyEx(show, show, MORPH_OPEN, kernel);

		num = connectedComponentsWithStats(show, labels, stats, centroids);
		for (int i = 0; i < num; i++)
		{
			int x = stats.at<int>(i, CC_STAT_LEFT);
			int y = stats.at<int>(i, CC_STAT_TOP);
			int w = stats.at<int>(i, CC_STAT_WIDTH);
			int h = stats.at<int>(i, CC_STAT_HEIGHT);
			int a = stats.at<int>(i, CC_STAT_AREA);

			if (a > D2sizeMin && a < D2sizeMax)
			{
				rectangle(dst, Point(x, y), Point(x + w, y + h), Scalar(255, 255, 0));
				err2++;

				//stringstream str;
				//str << "面积" << a << endl;
				//putTextZH(dst, str.str().c_str(), Point(x + w, y + h), Scalar(0, 255, 0), 15, "黑体", 0);
			}

		}
#pragma endregion

		str << "毛丝数：" << err1 << endl;
		str << "落尘数：" << err2 << endl;

		if (err2 < 1 && err1 < 1)
		{
			flag = true;
		}
	}
	else
	{
		str << "未找到镜头" << endl;
	}


#pragma region 文字输入
	//字体大小
	int text_Size;
	text_Size = (int)((data.w* data.h / 10000 - 30) * 0.078 + 25) * 2;
	//位置
	Point text_Localtion01;
	text_Localtion01.x = text_Size / 3;
	text_Localtion01.y = text_Size / 3;
	Point text_Localtion02;
	text_Localtion02.x = text_Size / 3;
	text_Localtion02.y = data.h - text_Size * 4;
	Point text_Localtion03;
	text_Localtion03.x = text_Size / 3;
	text_Localtion03.y = data.h - text_Size * 3;

	Scalar fontColor = Scalar(0, 255, 0);
	if (!flag)fontColor = Scalar(0, 0, 255);

	std::string text = str.str();
	putTextZH(dst, text.c_str(), text_Localtion01, fontColor, text_Size, "黑体", 0);
#pragma endregion

#pragma region 结果返回
	output_Parameter_Float[0] = flag;
	output_Parameter_Float[1] = (float)err1;
	output_Parameter_Float[2] = (float)err2;
#pragma endregion

#pragma region 图片返回
	int size = dst.total() * dst.elemSize();
	data.size = size;
	data.h = dst.rows;
	data.w = dst.cols;

	data.data_Output = (uchar *)calloc(size, sizeof(uchar));
	std::memcpy(data.data_Output, dst.data, size * sizeof(BYTE));
#pragma endregion
}

void DirtyTestP2R1(BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
#pragma region 本地参数申明

	bool flag = false;
	stringstream str;
	vector<Vec4i> hierarchy;
	vector<vector<Point2i>> contours;
	int _area = 0, tmm = -1, err1 = 0, err2 = 0, num;
	Mat	src, dst, temp, labels, stats, centroids, toBlur, Polared, Blured, Diff, show, DePolared, mask, ROI, kernel;
	
	str << "P2R1" << endl;
	//src = Mat(data.h, data.w, CV_8UC1, data.data_Input);//默认判胶相机使用的是彩色相机
	src = imread("C:\\Users\\Administrator\\Desktop\\7175\\外观\\P2R1\\0325\\2\\26.bmp", 0);
	cvtColor(src, dst, COLOR_GRAY2RGB);
#pragma endregion

#pragma region 参数导入
	

	int ShowMode = atoi(input_Parameter[0]);//显示模式：0-正常显示 1-定位画面 2-线状脏污 3-点状脏污

	int LocThresh = atoi(input_Parameter[1]);//定位-灰度阈值
	int MaxRadius = atoi(input_Parameter[2]);//定位-最大半径
	int MinRadius = atoi(input_Parameter[3]);	//定位-最小半径


	int Radius = atoi(input_Parameter[4]);//镜片有效半径
	int nMaxRadius = atoi(input_Parameter[5]);//屏蔽区域-半径上限
	int nMinRadius = atoi(input_Parameter[6]);//屏蔽区域-半径下限

	int D1thresh = atoi(input_Parameter[7]);	//脏污1-灰度阈值
	int D1SizeMax = atoi(input_Parameter[8]);	//脏污1-面积上限
	int D1SizeMin = atoi(input_Parameter[9]);	//脏污1-面积下限
	

	int AdapSize = atoi(input_Parameter[10]);			//脏污2-强度
	AdapSize = AdapSize * 2 + 1;
	int AdapC = atoi(input_Parameter[11]);				//脏污2-容差
	double RoundnessMin = atof(input_Parameter[12]);		//脏污2-圆度下限
	double RectangularityMin = atof(input_Parameter[13]);//脏污2-矩形度下限
	int D2sizeMax = atoi(input_Parameter[14]);			//脏污2-最大面积
	int D2sizeMin = atoi(input_Parameter[15]);			//脏污2-最小面积
#pragma endregion

#pragma region 轮廓大小定位
	threshold(src, temp, LocThresh, 255, THRESH_BINARY);
	findContours(temp, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_NONE, Point(0, 0));

	if (ShowMode == 1)
	{
		cvtColor(temp, dst, COLOR_GRAY2RGB);
	}

	for (size_t i = 0; i < contours.size(); i++)
	{
		vector<Point2i> tmpCont;
		convexHull(contours[i], tmpCont);
		double tmpArea = contourArea(tmpCont, 0);
		if (tmpArea > CV_PI*MaxRadius*MaxRadius || tmpArea < CV_PI*MinRadius*MinRadius)continue;
		if (tmpArea > _area)
		{
			_area = (long)tmpArea;
			tmm = i;
		}
	}
#pragma endregion

	if (tmm >= 0)
	{
#pragma region 圆心获取

		RotatedRect ell = fitEllipse(contours[tmm]);
		ellipse(dst, ell, Scalar(255, 0, 0), 2);

		circle(dst, ell.center, MaxRadius, Scalar(0, 255, 0), 2);
		circle(dst, ell.center, MinRadius, Scalar(0, 255, 0), 2);
		circle(dst, ell.center, nMinRadius, Scalar(0, 0, 255), 2);
		circle(dst, ell.center, nMaxRadius, Scalar(0, 0, 255), 2);
		circle(dst, ell.center, Radius, Scalar(0, 0, 255), 2);

#pragma endregion

#pragma region 预处理
		warpPolar(src, Polared, Size(Radius, Radius * 2 * CV_PI), ell.center, Radius, WARP_POLAR_LINEAR);

		toBlur = Polared;
		blur(toBlur, Blured, Size(3, 101));

		Diff = toBlur - Blured;
		Diff *= 5;

		rectangle(Diff, Point(nMinRadius, 0), Point(nMaxRadius, Radius * 2 * CV_PI), Scalar(0), -1);
		warpPolar(Diff, DePolared, src.size(), ell.center, Radius, WARP_INVERSE_MAP);

		mask = Mat::zeros(DePolared.size(), DePolared.type());
		circle(mask, ell.center, Radius - 1, Scalar(255), -1);

		DePolared.copyTo(ROI, mask);
#pragma endregion

#pragma region 亮线识别
		threshold(ROI, show, D1thresh, 255, THRESH_BINARY);

		if (ShowMode == 2)
		{
			cvtColor(show, dst, COLOR_GRAY2RGB);
		}

		num = connectedComponentsWithStats(show, labels, stats, centroids);
		for (int i = 0; i < num; i++)
		{
			int x = stats.at<int>(i, CC_STAT_LEFT);
			int y = stats.at<int>(i, CC_STAT_TOP);
			int w = stats.at<int>(i, CC_STAT_WIDTH);
			int h = stats.at<int>(i, CC_STAT_HEIGHT);
			int a = stats.at<int>(i, CC_STAT_AREA);

			if (a > D1SizeMin && a < D1SizeMax)
			{
				rectangle(dst, Point(x, y), Point(x + w, y + h), Scalar(0, 255, 255));
				err1++;
			}

		}
#pragma endregion

#pragma region 落尘识别
		adaptiveThreshold(ROI, show, 255, ADAPTIVE_THRESH_GAUSSIAN_C, THRESH_BINARY, AdapSize * 2 + 1, AdapC);

		kernel = getStructuringElement(MORPH_ELLIPSE, Size(3, 3));
		morphologyEx(show, show, MORPH_CLOSE, kernel);

		if (ShowMode == 3)
		{
			cvtColor(show, dst, COLOR_GRAY2RGB);
		}

		findContours(show, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_NONE, Point(0, 0));

		for (size_t i = 0; i < contours.size(); i++)
		{
			RotatedRect box = minAreaRect(contours[i]);

			double area = contourArea(contours[i]);
			double len = arcLength(contours[i], 1);
			double Roundness = (4 * CV_PI*area) / (len*len);

			float w = box.size.width;
			float h = box.size.height;
			float a = box.size.area();

			float minrectArea = w * h;
			double Rectangularity;
			if (minrectArea == 0)
			{
				Rectangularity = 0;
			}
			else
			{
				Rectangularity = area / minrectArea;
			}

			if (w < 1 || h < 1)continue;
			if (Rectangularity < RectangularityMin)continue;
			if (Roundness < RoundnessMin)continue;

			if (a > D2sizeMin && a < D2sizeMax)
			{
				Point2f rect[4];
				box.points(rect);

				for (size_t i = 0; i < 4; i++)
				{
					line(dst, rect[i], rect[(i + 1) % 4], Scalar(255, 255, 0), 1);
				}

				//stringstream str;
				//str << "面积" << a << endl;
				//str << "圆度" << Roundness << endl;
				//str << "矩形度" << Rectangularity << endl;
				//putTextZH(dst, str.str().c_str(), box.center, Scalar(0, 255, 0), 15, "黑体", 0);

				err2++;
			}
		}
#pragma endregion

		str << "不良类型1：" << err1 << endl;
		str << "不良类型2：" << err2 << endl;


		if (err2 <1 && err1 <1)
		{
			flag = true;
		}
	}
	else
	{
		str << "未找到镜头！";
	}

#pragma region 文字输入
	//字体大小
	int text_Size = (int)((data.w* data.h / 10000 - 30) * 0.078 + 25) * 2;
	//位置
	Point text_Localtion01;
	text_Localtion01.x = text_Size / 3;
	text_Localtion01.y = text_Size / 3;
	Point text_Localtion02;
	text_Localtion02.x = text_Size / 3;
	text_Localtion02.y = data.h - text_Size * 4;
	Point text_Localtion03;
	text_Localtion03.x = text_Size / 3;
	text_Localtion03.y = data.h - text_Size * 3;

	Scalar fontColor = Scalar(0, 255, 0);
	if (!flag)fontColor = Scalar(0, 0, 255);

	std::string text = str.str();
	putTextZH(dst, text.c_str(), text_Localtion01, fontColor, text_Size, "黑体", 0);
#pragma endregion

#pragma region 结果返回
	output_Parameter_Float[0] = flag;
	output_Parameter_Float[1] = (float)err1;
	output_Parameter_Float[2] = (float)err2;
#pragma endregion

#pragma region 图片返回
	int size = dst.total() * dst.elemSize();
	data.size = size;
	data.h = dst.rows;
	data.w = dst.cols;

	data.data_Output = (uchar *)calloc(size, sizeof(uchar));
	std::memcpy(data.data_Output, dst.data, size * sizeof(BYTE));
#pragma endregion
}

Mat RotateImage(Mat src, double angle)
{
	Mat dst;
	try
	{
		Size dst_sz(src.cols, src.rows);

		Point2f center(static_cast<float>(src.cols / 2.0), static_cast<float>(src.rows / 2.0));

		Mat rot_mat = getRotationMatrix2D(center, angle, 1.0);

		warpAffine(src,dst, rot_mat, dst_sz, INTER_LINEAR, BORDER_REPLICATE);
	}
	catch (const std::exception&)
	{
		return dst;
	}

	return dst;
}

void IRtest(BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
#pragma region 参数导入
	//显示模式 0表示正常 1表示灰度显示
	int ShowMode = stoi(input_Parameter[0]);
	//工作模式 0表示识别 1表示调试
	int WorkMode = stoi(input_Parameter[1]);
	
	//默认坐标
	float x_defult = stof(input_Parameter[2]);
	float y_defult = stof(input_Parameter[3]);
	//比例尺
	float scale = stof(input_Parameter[4]);
	//补偿值上限
	float offsetMax = stof(input_Parameter[5]);
	//最小尺寸
	int minSize = stoi(input_Parameter[6]);
	//最大尺寸
	int maxSize = stoi(input_Parameter[7]);

	//翻转
	int x_flip = stoi(input_Parameter[8]);
	int y_flip = stoi(input_Parameter[9]);
	int xy_flip = stoi(input_Parameter[10]);

	//颜色提取
	int R_min = stoi(input_Parameter[11]);
	int G_min = stoi(input_Parameter[12]);
	int B_min = stoi(input_Parameter[13]);

	int R_max = stoi(input_Parameter[14]);
	int G_max = stoi(input_Parameter[15]);
	int B_max = stoi(input_Parameter[16]);
#pragma endregion

#pragma region 本地参数
	bool flag = false;
	stringstream str;
	Mat src, output, roi, blured, HSV, mask;
	src = Mat(data.h, data.w, CV_8UC3, data.data_Input);//默认判胶相机使用的是彩色相机
	//Mat temp = imread("C:\\Users\\Administrator\\Desktop\\7175\\滤光片\\5.bmp", 1);
	//resize(temp, src, Size(0, 0), 0.25, 0.25);
	//src = imread("C:\\Users\\Administrator\\Desktop\\7175\\滤光片\\5.bmp", 1);
	output = src.clone();

	Scalar Low = Scalar(R_min, G_min, B_min), High = Scalar(R_max, G_max, B_max);

	RotatedRect TheIR;

	float x_out = 0;
	float y_out = 0;
	float a_out = 0;
#pragma endregion

#pragma region 预处理
	Rect rect = Rect(x_defult - maxSize / 2, y_defult - maxSize / 2, maxSize, maxSize);

	//绘制ROI
	rectangle(output, rect, Scalar(0, 255, 0), 2);
	mask = Mat::zeros(src.size(), src.type());
	rectangle(mask, rect, Scalar(255,255,255), -1);
	src.copyTo(roi, mask);

	//提取滤光片
	blur(roi, blured, Size(3, 3));
	cvtColor(blured, HSV, COLOR_BGR2RGB);
	inRange(HSV, Low, High, mask);

	Mat kernel = getStructuringElement(MORPH_ELLIPSE, Size(5, 5));
	morphologyEx(mask, mask, MORPH_CLOSE, kernel);

	if (ShowMode == 1)
	{
		//output = mask.clone();
		cvtColor(mask, output, CV_GRAY2BGR);
	}
#pragma endregion

#pragma region 部品定位
	vector<vector<Point>> contours;
	vector<Vec4i> hierarcy;
	findContours(mask, contours, hierarcy, RETR_EXTERNAL, CHAIN_APPROX_NONE);
	for (size_t i = 0; i < contours.size(); i++)
	{
		RotatedRect box = minAreaRect(Mat(contours[i]));

		float rate = box.size.width / box.size.height;
		float size = box.size.width * box.size.height;

		if (size < minSize*minSize || size > maxSize*maxSize)continue;
		if (rate < 0.95 || rate > 1.05)continue;

		if (size > TheIR.size.area())
		{
			TheIR = box;
		}
	}
#pragma endregion

#pragma region 结果提取
	if (0 < TheIR.size.area())
	{
		Point2f rect[4];
		TheIR.points(rect);

		for (size_t j = 0; j < 4; j++)
		{
			line(output, rect[j], rect[(j + 1) % 4], Scalar(0, 255, 0), 2);
			line(output, rect[j], rect[(j + 2) % 4], Scalar(255, 255, 0), 2);
		}
		circle(output, TheIR.center, 2, Scalar(255, 0, 255), -1);

		a_out = TheIR.angle;
		x_out = TheIR.center.x - x_defult;
		y_out = TheIR.center.y - y_defult;
		line(output, TheIR.center, TheIR.center + Point2f(x_out, y_out), Scalar(255, 255, 0), 2);

		line(output, TheIR.center, TheIR.center + 100 * Point2f(sin(a_out*CV_PI / 180), -cos(a_out*CV_PI / 180)), Scalar(255, 0, 255), 2);

		//根据输出转换 这里还要验证
		if (WorkMode == 0)
		{
			float r = sqrt(x_out*x_out + y_out * y_out);
			int angle = (int)(int)(acos(-y_out / r) * 180 / CV_PI);

			if (x_out < 0)angle = 360 - angle;

			angle -= a_out;

			y_out = -r * cos(angle*CV_PI / 180);
			x_out = r * sin(angle*CV_PI / 180);
		}

		x_out *= scale;
		y_out *= scale;
		line(output, TheIR.center, TheIR.center + Point2f(x_out, y_out), Scalar(0, 255, 255), 2);


		if (x_flip == 1)
		{
			x_out *= -1;
		}

		if (y_flip == 1)
		{
			y_out *= -1;
		}

		if (xy_flip == 1)
		{
			y_out += x_out;
			x_out = y_out - x_out;
			y_out = y_out - x_out;

		}

		
		str << "角度:" << TheIR.angle << endl;
		str << "基准中心:(" << x_defult << "," << y_defult << ")" << endl;
		str << "部品中心:(" << TheIR.center.x << "," << TheIR.center.y << ")" << endl;
		
		str << "x补偿:" << x_out << endl;
		str << "y补偿:" << y_out << endl;
		

		if (abs(x_out) < offsetMax && abs(y_out) < offsetMax)
		{
			flag = true;
		}
		else
		{
			str << "偏移量过大" << endl;
			x_out = 0;
			y_out = 0;
		}
		
	}
	else
	{
		str << "未找到部品" << endl;
	}

	//if (flag)
	//{
	//	putTextZH(output, str.str().c_str(), TheIR.center, Scalar(255, 0, 0), 20, "黑体", 0);
	//}
	//else
	//{
	//	putTextZH(output, str.str().c_str(), TheIR.center, Scalar(0, 0, 255), 20, "黑体", 0);
	//}
#pragma endregion

#pragma region 文字输入
	//字体大小
	int text_Size = (int)((data.w* data.h / 10000 - 30) * 0.078 + 25);
	//位置
	Point text_Localtion01;
	text_Localtion01.x = text_Size / 3;
	text_Localtion01.y = text_Size / 3;
	Point text_Localtion02;
	text_Localtion02.x = text_Size / 3;
	text_Localtion02.y = data.h - text_Size * 4;
	Point text_Localtion03;
	text_Localtion03.x = text_Size / 3;
	text_Localtion03.y = data.h - text_Size * 3;

	if (flag)
	{
		putTextZH(output, str.str().c_str(), text_Localtion01, Scalar(255, 0, 0), text_Size, "黑体", 0);
	}
	else
	{
		putTextZH(output, str.str().c_str(), text_Localtion01, Scalar(0, 0, 255), text_Size, "黑体", 0);
	}
#pragma endregion

#pragma region 结果输出
	output_Parameter_Float[0] = flag;
	output_Parameter_Float[1] = a_out;
	output_Parameter_Float[2] = x_out * 1000;
	output_Parameter_Float[3] = y_out * 1000;
#pragma endregion

#pragma region 图片返回
	int size = output.total() * output.elemSize();
	data.size = size;
	data.h = output.rows;
	data.w = output.cols;

	data.data_Output = (uchar *)calloc(size, sizeof(uchar));
	std::memcpy(data.data_Output, output.data, size * sizeof(BYTE));
#pragma endregion

}

void HDtest(BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
#pragma region 本地参数
	Mat src;
	//src = imread("C:\\Users\\Administrator\\Desktop\\7175\\外观\\P6\\11.bmp", 0);
	src = Mat(data.h, data.w, CV_8UC1, data.data_Input);//默认使用判胶相机
	Mat gray, mask, roi,output;
	vector<vector<Point>> contours;
	vector<Vec4i> hierarcy;
	stringstream str;
	RotatedRect ell;
	Point2f center, end;
	float a_out = 0, x_out = 0, y_out = 0;
	bool flag = false;
#pragma endregion

#pragma region 参数导入
	int ShowMode = stoi(input_Parameter[0]);
	//工作模式 0表示识别 1表示调试
	int WorkMode = stoi(input_Parameter[1]);

	float x_defult = stof(input_Parameter[2]);
	float y_defult = stof(input_Parameter[3]);

	//比例尺
	float scale = stof(input_Parameter[4]);
	//补偿值上限
	float offsetMax = stof(input_Parameter[5]);

	int minRadius = stoi(input_Parameter[6]);
	int maxRadius = stoi(input_Parameter[7]);

	//翻转
	int x_flip = stoi(input_Parameter[8]);
	int y_flip = stoi(input_Parameter[9]);
	int xy_flip = stoi(input_Parameter[10]);

	int MinGray = stoi(input_Parameter[11]);
	int MaxGray = stoi(input_Parameter[14]);
	
	int AminRadius = stoi(input_Parameter[12]);
	int AmaxRadius = stoi(input_Parameter[15]);
	
	int AngMinGray = stoi(input_Parameter[13]);
	int AngMaxGray = stoi(input_Parameter[16]);
#pragma endregion

#pragma region 预处理
	cvtColor(src, output, CV_GRAY2RGB);

	//roi跟threshold的顺序可以变更
	threshold(src, gray, MaxGray, 255, THRESH_TOZERO_INV);
	threshold(gray, gray, MinGray, 255, THRESH_BINARY);

	mask = Mat::zeros(src.size(), src.type());
	circle(mask, Point2f(x_defult, y_defult), maxRadius, Scalar(255), -1);
	circle(mask, Point2f(x_defult, y_defult), minRadius, Scalar(0), -1);

	gray.copyTo(roi, mask);

	if (ShowMode == 1)
	{
		cvtColor(gray, output, CV_GRAY2RGB);
	}

	circle(output, Point2f(x_defult, y_defult), maxRadius, Scalar(0, 255, 255), 2);
	circle(output, Point2f(x_defult, y_defult), minRadius, Scalar(0, 255, 255), 2);
	findContours(gray, contours, hierarcy, RETR_TREE, CHAIN_APPROX_NONE);
#pragma endregion

#pragma region 轮廓筛选
	int MaxL = -1;
	long area = -1;

	for (size_t i = 0; i < contours.size(); i++)
	{

		vector<Point2i> tmp;
		convexHull(contours[i], tmp);
		double tmpArea = contourArea(tmp);
		double tmpLen = arcLength(tmp, 0);
		if (tmpLen > 2 * CV_PI*maxRadius || tmpLen < 2 * CV_PI*minRadius) continue;
		if (tmpArea > CV_PI*maxRadius*maxRadius || tmpArea < CV_PI*minRadius*minRadius) continue;
		if (tmpArea > area)
		{
			area = (long)tmpArea;
			MaxL = i;
		}
	}
#pragma endregion

#pragma region 轮廓风险
	if (MaxL >= 0)
	{

#pragma region 部品中心查找
		float radius;
		vector<Point> tmpCont;

		convexHull(contours[MaxL], tmpCont);
		ell = fitEllipse(tmpCont);
		ellipse(output, ell, Scalar(255, 0, 0), 2);	//有效轮廓以蓝色

		line(output, ell.center + Point2f(5, 0), ell.center + Point2f(-5, 0), Scalar(0, 255, 255), 2);
		line(output, ell.center + Point2f(0, 5), ell.center + Point2f(0, -5), Scalar(0, 255, 255), 2);
		center = ell.center;

		str << "定位中心:(" << center.x << "," << center.y << ")" << endl;
		str << "校准中心:(" << x_defult << "," << y_defult << ")" << endl;
		radius = ell.size.width / 2;

		x_out = (center.x - x_defult)*scale;
		y_out = (center.y - y_defult)*scale;

		if (x_flip == 1)
		{
			x_out *= -1;
		}

		if (y_flip == 1)
		{
			y_out *= -1;
		}

		if (xy_flip == 1)
		{
			y_out += x_out;
			x_out = y_out - x_out;
			y_out = y_out - x_out;
		}
#pragma endregion

#pragma region 部品角度查找
		mask *= 0;
		roi *= 0;

		circle(mask, center, AmaxRadius, Scalar(255), -1);
		circle(mask, center, AminRadius, Scalar(0), -1);

		threshold(src, gray, AngMaxGray, 255, THRESH_TOZERO_INV);
		threshold(gray, gray, AngMinGray, 255, THRESH_BINARY);

		if (ShowMode == 2)
		{
			cvtColor(gray, output, CV_GRAY2RGB);
		}
		gray.copyTo(roi, mask);
		findContours(roi, contours, hierarcy, RETR_TREE, CHAIN_APPROX_NONE);

		circle(output, center, AmaxRadius, Scalar(0, 255, 0), 2);
		circle(output, center, AminRadius, Scalar(0, 255, 0), 2);

		MaxL = -1;
		area = -1;
		for (size_t i = 0; i < contours.size(); i++)
		{
			vector<Point2i> tmp;
			convexHull(contours[i], tmp);
			double tmpArea = contourArea(tmp);
			if (tmpArea > area)
			{
				area = (long)tmpArea;
				MaxL = i;
			}
		}

		if (MaxL >=0 && 10 < contours[MaxL].size())
		{
			convexHull(contours[MaxL], tmpCont);
			ell = fitEllipse(tmpCont);
			ellipse(output, ell, Scalar(255, 255, 0), 2);	//有效轮廓以蓝色
			line(output, ell.center + Point2f(5, 0), ell.center + Point2f(-5, 0), Scalar(255, 255, 0), 2);
			line(output, ell.center + Point2f(0, 5), ell.center + Point2f(0, -5), Scalar(255, 255, 0), 2);
			end = ell.center;
			line(output, center, end, Scalar(255, 0, 0), 2);

			Point2f p1 = Point2f(0, 1);
			Point2f p2 = end - center;

			float c = -p2.y / (sqrt(p2.x*p2.x + p2.y*p2.y));

			a_out = acos(c) * 180 / CV_PI;
			if (center.x > end.x)
			{
				a_out = 360 - a_out;
			}
			str << "角度" << a_out << endl;
			str << "补偿:(" << x_out << "," << y_out << ")" << endl;

			if (abs(x_out) < offsetMax && abs(y_out) < offsetMax)
			{
				flag = true;
			}
			else
			{
				str << "偏移量过大" << endl;
				x_out = 0;
				y_out = 0;
			}
		}
		else
		{
			str << "未找到有效缺口" << endl;
			end = Point2f(0, 0);
		}
#pragma endregion

	}
	else
	{
		str << "未找到有效边缘" << endl;
	}
#pragma endregion

#pragma region 文字输入
	//字体大小
	int text_Size = (int)((data.w* data.h / 10000 - 30) * 0.078 + 25);
	//位置
	Point text_Localtion01;
	text_Localtion01.x = text_Size / 3;
	text_Localtion01.y = text_Size / 3;
	Point text_Localtion02;
	text_Localtion02.x = text_Size / 3;
	text_Localtion02.y = data.h - text_Size * 4;
	Point text_Localtion03;
	text_Localtion03.x = text_Size / 3;
	text_Localtion03.y = data.h - text_Size * 3;

	if (flag)
	{
		putTextZH(output, str.str().c_str(), text_Localtion01, Scalar(255, 0, 0), text_Size, "黑体", 0);
	}
	else
	{
		putTextZH(output, str.str().c_str(), text_Localtion01, Scalar(0, 0, 255), text_Size, "黑体", 0);
	}
#pragma endregion

#pragma region 结果输出
	output_Parameter_Float[0] = flag;
	output_Parameter_Float[1] = a_out;
	output_Parameter_Float[2] = x_out * 1000;
	output_Parameter_Float[3] = y_out * 1000;
#pragma endregion

#pragma region 图片返回
	int size = output.total() * output.elemSize();
	data.size = size;
	data.h = output.rows;
	data.w = output.cols;

	data.data_Output = (uchar *)calloc(size, sizeof(uchar));
	std::memcpy(data.data_Output, output.data, size * sizeof(BYTE));
#pragma endregion
}



void ErrOutput(BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
	Mat src = Mat(data.h, data.w, CV_8UC1, data.data_Input);//默认非点胶后相机提供的原始图像为黑白图像
	Mat	output;
	stringstream str;
	cvtColor(src, output, COLOR_GRAY2RGB);

	str << "算法异常退出！";

#pragma region 文字输入
	//字体大小
	int text_Size = (int)((data.w* data.h / 10000 - 30) * 0.078 + 25) * 2;
	//位置
	Point text_Localtion01;
	text_Localtion01.x = text_Size / 3;
	text_Localtion01.y = text_Size / 3;
	Point text_Localtion02;
	text_Localtion02.x = text_Size / 3;
	text_Localtion02.y = data.h - text_Size * 4;
	Point text_Localtion03;
	text_Localtion03.x = text_Size / 3;
	text_Localtion03.y = data.h - text_Size * 3;

	Scalar fontColor = Scalar(0, 255, 255);

	std::string text = str.str();
	putTextZH(output, text.c_str(), text_Localtion01, fontColor, text_Size, "黑体", 0);
#pragma endregion

#pragma region 结果返回
	output_Parameter_Float[0] = false;
#pragma endregion

#pragma region 图片返回
	int size = output.total() * output.elemSize();
	data.size = size;
	data.h = output.rows;
	data.w = output.cols;

	data.data_Output = (uchar *)calloc(size, sizeof(uchar));
	std::memcpy(data.data_Output, output.data, size * sizeof(BYTE));
#pragma endregion

}

bool MV_EntryPoint(int type, BmpBuf &data, char** input_Parameter, float* output_Parameter_Float)
{
	try
	{
		switch (type)
		{
		case 0: DirtyTestG1R1(data, input_Parameter, output_Parameter_Float); break;
		case 1: DirtyTestP2R1(data, input_Parameter, output_Parameter_Float); break;
		case 2:	IRtest(data, input_Parameter, output_Parameter_Float); break;
		case 3:	HDtest(data, input_Parameter, output_Parameter_Float); break;

		default:
			break;
		}
	}
	catch (const std::exception&)
	{
		ErrOutput(data, input_Parameter, output_Parameter_Float);
	}
	

	return false;
}

bool MV_Release(BmpBuf &data)
{
	delete data.data_Output;
	data.data_Output = NULL;

	data.size = 0;
	return 0;
}