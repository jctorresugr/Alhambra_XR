package com.alhambra.view;

import android.content.Context;
import android.util.AttributeSet;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.viewpager.widget.ViewPager;

import com.alhambra.R;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.UserData;
import com.alhambra.view.base.DragViewLayout;

//copy from https://juejin.cn/post/6911582503212384263
public class FloatMiniMapView extends DragViewLayout {

    private MapView m_mapView;
    public ViewPager pager;
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
        m_mapView = new MapView(context);
        m_mapView.matrix.setScale(0.5f,0.5f);
        final TextView textView = new TextView(context);
        textView.setOnClickListener(l-> Toast.makeText(this.getContext(),"Clicked!",Toast.LENGTH_SHORT).show());
        LayoutParams params = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
        LayoutParams params2 = new LayoutParams(500, 500);
        addView(textView, params);
        addView(m_mapView,params2);
        m_mapView.setClickable(true);
        m_mapView.setOnClickListener(
                l->{
                    Toast.makeText(this.getContext(),"Clicked!",Toast.LENGTH_SHORT).show();
                    pager.setCurrentItem(2);
                }
        );
    }

    public void setData(AnnotationDataset annotationDataset, UserData userData){
        m_mapView.setDataset(annotationDataset);
        m_mapView.setUserData(userData);
    }
}
