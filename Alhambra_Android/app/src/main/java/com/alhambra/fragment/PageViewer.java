package com.alhambra.fragment;

import androidx.viewpager.widget.ViewPager;

import android.content.Context;
import android.util.AttributeSet;
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
        return m_enabled && super.onTouchEvent(event);
    }

    @Override
    public boolean onInterceptTouchEvent(MotionEvent event)
    {
        return m_enabled && super.onInterceptTouchEvent(event);
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