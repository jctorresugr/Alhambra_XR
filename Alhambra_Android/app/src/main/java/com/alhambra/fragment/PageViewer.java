package com.alhambra.fragment;

import androidx.viewpager.widget.ViewPager;

import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;

public class PageViewer extends ViewPager
{
    private boolean m_enabled = true;

    public PageViewer(Context context, AttributeSet attrs)
    {
        super(context, attrs);
    }

    public PageViewer(Context context)
    {
        super(context);
    }

    @Override
    public boolean onTouchEvent(MotionEvent event)
    {
        //strange bug
        //https://stackoverflow.com/questions/16459196/java-lang-illegalargumentexception-pointerindex-out-of-range-exception-dispat
        // use try catch to resolve the pointerIndex out of range issue
        try{
            return m_enabled && super.onTouchEvent(event);
        }catch(IllegalArgumentException e){
            //print if you want to see these errors
            //e.printStackTrace();
            Log.i("PageViewer","Impede IllegalArgumentException crush");
        }
        return m_enabled;

    }

    @Override
    public boolean onInterceptTouchEvent(MotionEvent event)
    {
        //strange bug
        //https://stackoverflow.com/questions/16459196/java-lang-illegalargumentexception-pointerindex-out-of-range-exception-dispat
        // use try catch to resolve the pointerIndex out of range issue
        try{
            return m_enabled && super.onInterceptTouchEvent(event);
        }catch(IllegalArgumentException e){
            //print if you want to see these errors
            //e.printStackTrace();
            Log.i("PageViewer","Impede IllegalArgumentException crush");
        }
        return m_enabled;
    }

    public void setPagingEnabled(boolean enabled)
    {
        m_enabled = enabled;
    }

    public boolean isPagingEnabled()
    {
        return m_enabled;
    }
}