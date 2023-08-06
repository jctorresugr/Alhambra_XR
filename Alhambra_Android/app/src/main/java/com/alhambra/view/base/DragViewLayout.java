package com.alhambra.view.base;

import android.animation.ValueAnimator;
import android.content.Context;
import android.graphics.PixelFormat;
import android.util.AttributeSet;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewConfiguration;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.widget.RelativeLayout;

public class DragViewLayout extends RelativeLayout{

    //view所在位置
    int mLastX, mLastY;

    //屏幕高宽
    protected int mScreenWidth, mScreenHeight;

    //view高宽
    protected int mWidth, mHeight;

    protected boolean canBeDragged=true;

    /**
     * 是否在拖拽过程中
     */
    boolean isDrag = false;

    /**
     * 系统最小滑动距离
     * @param context
     */
    int mTouchSlop = 100;

    private boolean hasShown=false;

    protected WindowManager.LayoutParams floatLayoutParams;
    protected WindowManager mWindowManager;

    //手指触摸位置
    private float xInScreen;
    private float yInScreen;
    private float xInView;
    private float yInView;
    public boolean enableAttachEdge=true;
    public View dispatchEventToView = null;


    public DragViewLayout(Context context) {
        this(context, null);
    }

    public DragViewLayout(Context context, AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public DragViewLayout(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        mScreenWidth = context.getResources().getDisplayMetrics().widthPixels;
        mScreenHeight = context.getResources().getDisplayMetrics().heightPixels;
        mTouchSlop = ViewConfiguration.get(context).getScaledTouchSlop();
        mWindowManager = (WindowManager) getContext().getSystemService(Context.WINDOW_SERVICE);
    }

    @Override
    protected void onFinishInflate() {
        super.onFinishInflate();
    }

    @Override
    public boolean dispatchTouchEvent(MotionEvent event) {
        if(canBeDragged){
            int action = event.getAction();
            switch (action) {
                case MotionEvent.ACTION_DOWN:
                    mLastX = (int) event.getRawX();
                    mLastY = (int) event.getRawY();
                    yInView = event.getY();
                    xInView = event.getX();
                    xInScreen = event.getRawX();
                    yInScreen = event.getRawY();
                    break;
                case MotionEvent.ACTION_MOVE:
                    int dx = (int) event.getRawX() - mLastX;
                    int dy = (int) event.getRawY() - mLastY;
                    if (Math.abs(dx) > mTouchSlop || Math.abs(dy) > mTouchSlop) {
                        isDrag = true;
                    }
                    xInScreen = event.getRawX();
                    yInScreen = event.getRawY();
                    mLastX = (int) event.getRawX();
                    mLastY = (int) event.getRawY();
                    //拖拽时调用WindowManager updateViewLayout更新悬浮球位置
                    updateFloatPosition(false);
                    break;
                case MotionEvent.ACTION_UP:
                    if (isDrag) {
                        //执行贴边
                        startEdgeAttach();
                        isDrag = false;
                    }
                    break;
                default:
                    break;
            }
        }
        if(dispatchEventToView!=null){
            dispatchEventToView.dispatchTouchEvent(event);
        }
        return super.dispatchTouchEvent(event);
    }

    //更新悬浮球位置
    private void updateFloatPosition(boolean isUp) {
        if(!isDrag){
            return;
        }
        int x = (int) (xInScreen - xInView);
        int y = (int) (yInScreen - yInView);
        if(isUp) {
            x = isRightFloat() ? mScreenWidth : 0;
        }
        if(y < 0) {
            y = 0;
        }
        if(y > mScreenHeight - mHeight) {
            y = mScreenHeight - mHeight;
        }
        floatLayoutParams.x = x;
        floatLayoutParams.y = y;
        //更新位置
        mWindowManager.updateViewLayout(this, floatLayoutParams);
    }

    /**
     * 是否靠右边悬浮
     * @return
     */
    boolean isRightFloat() {
        return xInScreen > mScreenWidth / 2;
    }


    //执行贴边动画
    protected void startEdgeAttach(){
        if(!enableAttachEdge){
            return;
        }
        ValueAnimator valueAnimator;
        if (floatLayoutParams.x+mWidth/2 < mScreenWidth / 2) {
            valueAnimator = ValueAnimator.ofInt(floatLayoutParams.x, 0);
        } else {
            valueAnimator = ValueAnimator.ofInt(floatLayoutParams.x, mScreenWidth - mWidth);
        }
        valueAnimator.setDuration(200);
        valueAnimator.addUpdateListener(animation -> {
            floatLayoutParams.x = (int)animation.getAnimatedValue();
            mWindowManager.updateViewLayout(DragViewLayout.this, floatLayoutParams);
        });
        valueAnimator.start();
    }

    //悬浮球显示
    public void show() {
        if(hasShown){
            return;
        }
        hasShown=true;
        floatLayoutParams = new WindowManager.LayoutParams(
                WindowManager.LayoutParams.WRAP_CONTENT,
                WindowManager.LayoutParams.WRAP_CONTENT,
                WindowManager.LayoutParams.FIRST_SUB_WINDOW,
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE |
                        WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL |
                        WindowManager.LayoutParams.FLAG_LAYOUT_NO_LIMITS,
                PixelFormat.RGBA_8888);
        floatLayoutParams.gravity = Gravity.LEFT | Gravity.TOP;
        if(mWidth==0){ //avoid outside of the screen
            floatLayoutParams.x = 0;
        }else{
            floatLayoutParams.x = mScreenWidth-mWidth;
        }
        floatLayoutParams.y = (int)(mScreenHeight * 0.3);
        mWindowManager.addView(this, floatLayoutParams);
        updateSize();
    }

    public void hide(){
        if(!hasShown){
            return;
        }
        hasShown=false;
        mWindowManager.removeView(this);
    }

    @Override
    protected void onLayout(boolean changed, int l, int t, int r, int b) {
        super.onLayout(changed, l, t, r, b);
        updateSize();
    }

    private void updateSize(){
        mWidth = getWidth();
        mHeight = getHeight();
    }

    public void setSize(int width, int height){
        mWidth=width;
        mHeight =height;
        ViewGroup.LayoutParams layoutParams = this.getLayoutParams();
        layoutParams.height = height;
        layoutParams.width = width;
    }

    public void setTouchSlop(int mTouchSlop) {
        this.mTouchSlop = mTouchSlop;
    }

    public boolean isCanBeDragged() {
        return canBeDragged;
    }

    public void setCanBeDragged(boolean canBeDragged) {
        this.canBeDragged = canBeDragged;
    }

}
