package com.alhambra.view;

import android.animation.Animator;
import android.animation.AnimatorListenerAdapter;
import android.animation.ValueAnimator;
import android.content.Context;
import android.util.AttributeSet;
import android.view.ViewGroup;
import android.widget.ImageButton;

import androidx.viewpager.widget.ViewPager;

import com.alhambra.R;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.UserData;
import com.alhambra.experiment.ExperimentDataCollection;
import com.alhambra.view.base.DragViewLayout;
import com.sereno.color.Color;
import com.sereno.math.MathUtils;

//copy from https://juejin.cn/post/6911582503212384263
public class FloatMiniMapView extends DragViewLayout {

    private MapView m_mapView;
    private ImageButton scaleButton;
    public ViewPager pager;
    private boolean isZoomed = false;
    private static final float SMALL_ZOOM = 0.35f;
    private static final float BIG_ZOOM = 0.75f;
    public FloatMiniMapView(Context context) {
        super(context);
        init(context);
    }

    public FloatMiniMapView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context);
    }

    public FloatMiniMapView(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context);
    }

    public void init(Context context){
        setClickable(true);
        setTouchSlop(10);


        //add map view
        m_mapView = new MapView(context);
        m_mapView.cleanColor = Color.toARGB8888(233,233,233,233);
        m_mapView.fixedBounds=false;
        int basicSize = (int) (Math.min(mScreenWidth,mScreenHeight)*SMALL_ZOOM);
        LayoutParams params2 = new LayoutParams(basicSize,basicSize);
        addView(m_mapView,params2);
        m_mapView.setClickable(true);
        m_mapView.matrix.setScale(SMALL_ZOOM,SMALL_ZOOM);
        lastSize=basicSize;
        mWidth=mHeight=basicSize;

        scaleButton = new ImageButton(context);
        scaleButton.setImageResource(R.drawable.maximize);
        scaleButton.setClickable(true);
        addView(scaleButton,64,64);


        scaleButton.setOnClickListener(v->{
            if(isZoomed){
                setToSmallView();
            }else{
                setToBigView();
            }

            ExperimentDataCollection.add("ui_minimap_scaleButton",isZoomed);
        });
    }

    private int lastSize = 0;

    public void setToSmallView(){
        if(!isZoomed){
            return;
        }
        isZoomed=false;
        setMapSize((int) (Math.min(mScreenWidth,mScreenHeight)*SMALL_ZOOM));
        scaleButton.setImageResource(R.drawable.maximize);
    }

    public void setToBigView(){
        if(isZoomed){
            return;
        }
        isZoomed=true;
        setMapSize((int) (Math.min(mScreenWidth,mScreenHeight)*BIG_ZOOM));
        scaleButton.setImageResource(R.drawable.minimize);
    }

    public void setMapSize(int size){
        if(lastSize==size){
            return;
        }
        /*
        ViewGroup.LayoutParams layoutParams = m_mapView.getLayoutParams();
        layoutParams.width=size;
        layoutParams.height=size;
        setSize(size,size);
        float scaleRatio = Math.min(size/(float)Math.min(mScreenWidth,mScreenHeight),1.0f);
        m_mapView.matrix.setScale(scaleRatio,scaleRatio);
        m_mapView.requestLayout();
        requestLayout();*/
        sizeAnimation(lastSize,size);
        lastSize=size;
        startEdgeAttach();
    }

    protected void sizeAnimation(int beforeSize, int targetSize){
        ValueAnimator valueAnimator = ValueAnimator.ofFloat(0.0f,1.0f);

        int orgX,targetX,orgY,targetY;
        orgX = floatLayoutParams.x;
        if (floatLayoutParams.x+mWidth/2 < mScreenWidth / 2) {
            targetX = 0;
        } else {
            targetX = mScreenWidth - targetSize;
        }

        orgY=floatLayoutParams.y;
        if(floatLayoutParams.y+targetSize>mScreenHeight){
            targetY = mScreenHeight-targetSize;
        }else{
            targetY =orgY;
        }

        valueAnimator.setDuration(100);
        valueAnimator.addUpdateListener(animation -> {
            float value = valueAnimator.getAnimatedFraction();
            int curSize = (int) MathUtils.interpolate(beforeSize, targetSize, value);
            ViewGroup.LayoutParams layoutParams = m_mapView.getLayoutParams();
            layoutParams.width=curSize;
            layoutParams.height=curSize;
            float scaleRatio = Math.min(curSize/(float)Math.min(mScreenWidth,mScreenHeight),1.0f);
            m_mapView.matrix.setScale(scaleRatio,scaleRatio);


            floatLayoutParams.x =  (int) MathUtils.interpolate(orgX,targetX, value);
            //noinspection SuspiciousNameCombination
            floatLayoutParams.y =  (int) MathUtils.interpolate(orgY,targetY, value);
            mWindowManager.updateViewLayout(FloatMiniMapView.this, floatLayoutParams);

            requestLayout();
        });
        valueAnimator.addListener(new AnimatorListenerAdapter() {
            @Override
            public void onAnimationStart(Animator animation) {
                super.onAnimationStart(animation);
                m_mapView.fixedBounds=true;
                enableAttachEdge=false;
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                super.onAnimationEnd(animation);
                m_mapView.fixedBounds=false;
                enableAttachEdge=true;
                m_mapView.refreshBoundsInfo();
            }
        });
        valueAnimator.start();
    }

    public void setData(AnnotationDataset annotationDataset, UserData userData, SelectionData selectionData){
        m_mapView.setDataset(annotationDataset);
        m_mapView.setUserData(userData);
        m_mapView.setSelectionData(selectionData);
    }

    public MapView getMapView() {
        return m_mapView;
    }
}
