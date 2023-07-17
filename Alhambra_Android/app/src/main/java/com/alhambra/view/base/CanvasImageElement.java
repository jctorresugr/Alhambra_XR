package com.alhambra.view.base;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;

public class CanvasImageElement extends CanvasBaseElement{

    protected Bitmap image;
    public Rect drawRect = new Rect();
    public Rect orgRect = new Rect();

    private static final Paint imagePaint = newPaint(255,255,255,85,0, Paint.Style.FILL);

    public void setImage(Bitmap image) {
        this.image = image;
        drawRect.top    =orgRect.top     =0;
        drawRect.bottom =orgRect.bottom  =image.getHeight();
        drawRect.left   =orgRect.left    =0;
        drawRect.right  =orgRect.right   =image.getWidth();
    }

    @Override
    public void draw(Canvas canvas) {
        if(image!=null){
            canvas.drawBitmap(image,orgRect,drawRect,imagePaint);
        }
    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return false;
    }
}
