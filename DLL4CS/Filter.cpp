#include "pch.h"
#include "Filter.h"

void SurfaceFittig(Mat src, Mat& dst, int para1, int para2)
{
#if 0
	int num_cols = para1;
	int num_rows = para2;

	float xBorder = src.cols*1.0 / num_cols;
	float yBorder = src.rows*1.0 / num_rows;

	Mat input, tmp;
	input = src.clone();
	copyMakeBorder(input, tmp, yBorder, yBorder, xBorder, xBorder, BORDER_REFLECT101);

	for (size_t i = 0; i < num_cols; i++)
	{
		for (size_t j = 0; j < num_rows; j++)
		{
			float x = i * xBorder;
			float y = j * yBorder;

			float w1 = (0 + 1)*xBorder;
			float h1 = (0 + 1)*yBorder;
			float w3 = (0 + 3)*xBorder;
			float h3 = (0 + 3)*yBorder;

			Mat src_img = tmp(Rect(x, y, w3, h3));
			Mat dst_img = src(Rect(x, y, w1, h1));
			Mat dst_imgEx = dst(Rect(x, y, w1, h1));

			Bd(src_img, dst_img, dst_imgEx);
		}
	}
#else
	int size = 1 + 2 * para1;
	float bMat[4][4] = {
		{	1	,4	,1	,0} ,
		{	-3	,0	,3	,0} ,
		{	3	,-6	,3	,0} ,
		{	-1	,3	,-3	,1}
	};

	Mat _b = Mat(4, 4, CV_32FC1, bMat);
	Mat u = Mat::zeros(Size(4, 1), CV_32FC1);
	Mat v = Mat::zeros(Size(4, 1), CV_32FC1);

	v.at<float>(0, 0) = 1;
	v.at<float>(0, 1) = 3 * 1.0 / 6;
	v.at<float>(0, 2) = v.at<float>(0, 1)*v.at<float>(0, 1);
	v.at<float>(0, 3) = v.at<float>(0, 2)*v.at<float>(0, 1);

	u.at<float>(0, 0) = 1;
	u.at<float>(0, 1) = 3 * 1.0 / 6;
	u.at<float>(0, 2) = u.at<float>(0, 1)*u.at<float>(0, 1);
	u.at<float>(0, 3) = u.at<float>(0, 2)*u.at<float>(0, 1);

	Mat b2t = _b.t();
	Mat v2t = v.t();
	Mat ub = u * _b;
	Mat bv = b2t * v2t;

	float a = ub.at<float>(0, 0);
	float b = ub.at<float>(0, 1);
	float c = ub.at<float>(0, 2);
	float d = ub.at<float>(0, 3);

	float e = bv.at<float>(0, 0);
	float f = bv.at<float>(1, 0);
	float g = bv.at<float>(2, 0);
	float h = bv.at<float>(3, 0);

	float ksrc[16] = {
		a*e,a*f,a*g,a*h,
		b*e,b*f,b*g,b*h,
		c*e,c*f,c*g,c*h,
		d*e,d*f,d*g,d*h,
	};

	float *_k = (float*)malloc(sizeof(float)*(3 * size + 4)*(3 * size + 4));
	std::memset(_k, 0, sizeof(float)*(3 * size + 4)*(3 * size + 4));
	int num = 0;
	for (int i = 0; i < 3 * size + 4; i += size + 1)
	{
		for (int j = 0; j < 3 * size + 4; j += size + 1)
		{
			_k[j + i * (3 * size + 4)] = ksrc[num++] / 36;
		}
	}

	Mat kernel(3 * size + 4, 3 * size + 4, CV_32FC1, _k);
	filter2D(src, dst, CV_8UC1, kernel, Point(-1, -1));
	free(_k);
#endif // 0


}