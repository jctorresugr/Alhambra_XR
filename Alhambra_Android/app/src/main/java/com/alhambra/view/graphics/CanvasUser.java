package com.alhambra.view.graphics;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;

import com.alhambra.dataset.UserData;
import com.alhambra.view.base.CanvasInteractiveElement;
import com.sereno.math.MathUtils;
import com.sereno.math.TranslateMatrix;
import com.sereno.math.Vector3;

public class CanvasUser extends CanvasInteractiveElement
    implements UserData.OnUserDataChangeListener{

    private UserData userData;
    private TranslateMatrix translateMatrix;

    private static final int radius = 50;
    private static final Path path;
    private static final Paint pathPaint;
    static{
        float hr = radius*0.5f;
        path = new Path();
        path.setFillType(Path.FillType.EVEN_ODD);
        path.moveTo(0,0);
        path.lineTo(hr,-hr);
        path.lineTo(0,radius);
        path.lineTo(-hr,-hr);
        path.close();

        pathPaint = newPaint(200,100,10,125,6.0f,Paint.Style.STROKE);
    }

    @Override
    public void draw(Canvas canvas) {
        float deg = userData.getRotation().y;
        canvas.translate(x,y);
        canvas.rotate(deg);
        //canvas.drawCircle(x,y,radius,pathPaint);
        canvas.drawPath(path,pathPaint);
        canvas.rotate(-deg);
        canvas.translate(-x,-y);
    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return MathUtils.distanceSqr(x0,y0,x,y)<radius*radius*2;
    }

    public UserData getUserData() {
        return userData;
    }

    public void setUserData(UserData userData) {
        if(this.userData!=userData){
            if(this.userData!=null){
                this.userData.dataChangeListeners.removeListener(this);
            }
            this.userData = userData;
            this.userData.dataChangeListeners.addListener(this);
        }

    }

    public TranslateMatrix getTranslateMatrix() {
        return translateMatrix;
    }

    public void setTranslateMatrix(TranslateMatrix translateMatrix) {
        this.translateMatrix = translateMatrix;
    }

    @Override
    public void onUserDataChange(UserData data) {
        updatePosition();
        dirty();
    }

    public void updatePosition(){
        Vector3 position = userData.getPosition();
        if(position==null){
            return;
        }
        this.x = translateMatrix.transformPointX(position.x);
        this.y = translateMatrix.transformPointY(position.z);
    }
}
