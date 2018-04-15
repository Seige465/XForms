using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Util;

namespace XForms.Droid.Helpers
{
    public class DrawingView : View
    {
        Context context;
        public Bitmap bm;
        Canvas canvas;
        Path path;
        Paint paint;

        Paint circlePaint;
        Path circlePath;

        int mWidth;
        int mHeight;

        ImageView backgroundImage;


        int StrokeWidth = 4;
        public DrawingView(Context c, Bitmap bitmap, int strokeWidth, ImageView iv) : base(c)
        {
            context = c;
            bm = bitmap;


            StrokeWidth = strokeWidth;

            path = new Path();
            paint = new Paint(PaintFlags.Dither);
            circlePaint = new Paint();
            circlePath = new Path();

            circlePaint.AntiAlias = true;
            circlePaint.Color = Color.Blue;
            circlePaint.SetStyle(Paint.Style.Stroke);
            circlePaint.StrokeJoin = Paint.Join.Miter;
            circlePaint.StrokeWidth = 4;

            StrokeWidth = strokeWidth;
            WidgetHelper.mPaint.StrokeWidth = StrokeWidth;

            backgroundImage = iv;

        }

        public void ClearCanvas()
        {          
            canvas.DrawColor(Color.Transparent, PorterDuff.Mode.Clear);
            Console.WriteLine("Canvas cleared!!!!!!!!!!!!");
        }

        public void UpdateStrokeWidth(int width)
        {
            //sets the width to a proper size with density in mind
            DisplayMetrics dm = Resources.DisplayMetrics;
            float adjustedWidth = TypedValue.ApplyDimension(ComplexUnitType.Dip, width, dm);
            StrokeWidth = (int)adjustedWidth;
            WidgetHelper.mPaint.StrokeWidth = StrokeWidth;
        }

        int offsetHeight;
        int offsetWidth;

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            decimal scale = 0;
            if(oldw != 0 || oldh != 0)
            {
                if(w < oldw || w > oldw)
                {
                    scale = Decimal.Divide(w, oldw);
                    h = (int)(scale * h);
                }
            
                else if(h < oldh || h > oldh)
                {
                    scale = Decimal.Divide(h, oldh);
                    w = (int)(scale * w);
                }
            }

            Console.WriteLine($"Old Width: {oldw},{oldh}\n New Width: {w},{h}");

            Console.WriteLine($"Parent height: {((RelativeLayout)Parent).Height}");
            Console.WriteLine($"Parent height: {((RelativeLayout)Parent).Width}");

            if (bm == null)
                bm = Bitmap.CreateBitmap(w, h, Bitmap.Config.Argb8888);
            else
            {
                bm = Bitmap.CreateScaledBitmap(bm, w, h, true);
            }
            //bm = bm.Copy(Bitmap.Config.Argb8888, true);


            if (backgroundImage.Height == bm.Height)
                offsetHeight = 0;
            else
                offsetHeight = (backgroundImage.Height - bm.Height) / 2;

            if (backgroundImage.Width == bm.Width)
                offsetWidth = 0;
            else
                offsetWidth = (backgroundImage.Width - bm.Width) / 2;


            canvas = new Canvas(bm);
            //align the layout parameters back to the enclosing view
            //var layoutParams = new RelativeLayout.LayoutParams(w, h);
            //layoutParams.AddRule(LayoutRules.CenterInParent);
            //this.LayoutParameters = layoutParams;

        }


        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            /*
            //do the bitmap stuff here, bitshift right (which is a division by 2)
            int cx = (mWidth - bm.Width) >> 1;
            int cy = (mHeight - bm.Height) >> 1;*/

            //canvas.DrawBitmap(bm, cx, cy, paint);


            canvas.DrawBitmap(bm, null, new RectF(offsetWidth, offsetHeight, bm.Width, bm.Height), paint);
            canvas.DrawPath(path, WidgetHelper.mPaint);
            canvas.DrawPath(circlePath, circlePaint);
            //draw half red onto canvas to show drawable area

        }

        public void SetupDrawingView(Context c, Bitmap bitmap, int strokeWidth)
        {
            context = c;
            bm = bitmap;


            StrokeWidth = strokeWidth;

            path = new Path();
            paint = new Paint(PaintFlags.Dither);
            circlePaint = new Paint();
            circlePath = new Path();

            circlePaint.AntiAlias = true;
            circlePaint.Color = Color.Blue;
            circlePaint.SetStyle(Paint.Style.Stroke);
            circlePaint.StrokeJoin = Paint.Join.Miter;
            circlePaint.StrokeWidth = 4;

            StrokeWidth = strokeWidth;
            WidgetHelper.mPaint.StrokeWidth = StrokeWidth;

            Invalidate();
            RequestLayout();
        }
        /*
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            mWidth = MeasureSpec.GetSize(widthMeasureSpec);
            mHeight = MeasureSpec.GetSize(heightMeasureSpec);

            SetMeasuredDimension(mWidth, mHeight);

        }*/

        float mX, mY;
        const float TOUCH_TOLERANCE = 4;

        private void TouchStart(float x, float y)
        {
            Console.WriteLine("Touch start");
            path.Reset();
            path.MoveTo(x, y);
            mX = x;
            mY = y;
        }

        private void TouchMove(float x, float y)
        {
            Console.WriteLine("Touch move");

            float dx = Math.Abs(x - mX);
            float dy = Math.Abs(y - mY);
            if(dx >= TOUCH_TOLERANCE || dy >= TOUCH_TOLERANCE)
            {
                path.QuadTo(mX, mY, (x + mX) / 2, (y + mY) / 2);
                mX = x;
                mY = y;

                circlePath.Reset();
                circlePath.AddCircle(mX, mY, 30, Path.Direction.Cw);
            }
        }

        private void TouchUp()
        {
            Console.WriteLine("Touch up");

            path.LineTo(mX, mY);
            circlePath.Reset();

            canvas.DrawPath(path, WidgetHelper.mPaint);
            path.Reset();
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            float x = e.GetX();
            float y = e.GetY();

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    TouchStart(x, y);
                    Invalidate();
                    break;
                case MotionEventActions.Move:
                    TouchMove(x, y);
                    Invalidate();
                    break;
                case MotionEventActions.Up:
                    TouchUp();
                    Invalidate();
                    break;
            }
            return true;

        }

    }
}