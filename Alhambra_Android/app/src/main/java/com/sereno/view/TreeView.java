package com.sereno.view;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;

import com.sereno.Tree;
import com.alhambra.R;

public class TreeView extends ViewGroup implements Tree.ITreeListener<View>
{
    /** Class representing the measure of the tree view*/
    private static class TreeViewMeasureState
    {
        int width;  /*!< The current width*/
        int height; /*!< The current height*/
        int measureState; /*!< The state of the measure*/
        int widthMode;    /*!< What is the width mode of this layout?*/
        int heightMode;   /*!< What is the height mode of this layout?*/
        int maximumWidth; /*!< The width associated with the width mode*/
        int maximumHeight; /*!< The height associated with the height mode*/
        int topOffset = 0; /*!< The top offset*/

        @Override
        public Object clone()
        {
            TreeViewMeasureState s = new TreeViewMeasureState();
            s.width             = width;
            s.height            = height;
            s.measureState      = measureState;
            s.widthMode         = widthMode;
            s.heightMode        = heightMode;
            s.maximumWidth      = maximumWidth;
            s.maximumHeight     = maximumHeight;
            s.topOffset         = topOffset;
            return s;
        }
    }

    /** The left offset to apply per level in the tree view*/
    private int m_leftOffsetPerLevel = 0;

    /**  The top offset to apply per level in the tree view*/
    private int m_topOffsetPerChild  = 0;

    private int m_strokeWidth = 1;

    /**  The internal tree data*/
    private Tree<View> m_tree;

    /**  The Paint used when drawing*/
    private Paint m_paint;

    /** The image representing the in extended*/
    private Bitmap m_inExtendImg;

    /** The image representing the not extend state*/
    private Bitmap m_notExtendImg;

    /** The extend bitmap width*/
    private int m_extendWidth;

    /** The extend bitmap height*/
    private int m_extendHeight;

    /**  Constructor with the view's context as @parameter
     * @param c the Context associated with the view*/
    public TreeView(Context c)
    {
        super(c);
        init(null);
    }

    /**  Constructor with the view's context as @parameter and the XML data
     *
     * @param c the Context associated with the view
     * @param a the XML attributes of the view
     * @param style the style ID of the View (see View.Style)
     */
    public TreeView(Context c, AttributeSet a, int style)
    {
        super(c, a, style);
        init(a);
    }

    /**  Constructor with the view's context as @parameter and the XML data
     *
     * @param c the Context associated with the view
     * @param a the XML attributes of the view*/
    public TreeView(Context c, AttributeSet a)
    {
        super(c, a);
        init(a);
    }

    /**  Initialize the TreeView object*/
    public void init(AttributeSet a)
    {
        //Read the AttributeSet
        TypedArray ta = getContext().obtainStyledAttributes(a, R.styleable.TreeView);
        m_leftOffsetPerLevel = ta.getDimensionPixelSize(R.styleable.TreeView_leftOffsetPerLevel, 10);
        m_topOffsetPerChild  = ta.getDimensionPixelSize(R.styleable.TreeView_topOffsetPerChild,15);
        m_strokeWidth        = ta.getDimensionPixelSize(R.styleable.TreeView_strokeWidth, 1);
        int inExtendSrc      = ta.getResourceId(R.styleable.TreeView_inExtendSrc, -1);
        int notExtendSrc     = ta.getResourceId(R.styleable.TreeView_notExtendSrc, -1);
        m_extendWidth        = ta.getDimensionPixelSize(R.styleable.TreeView_extendWidth, 32);
        m_extendHeight       = ta.getDimensionPixelSize(R.styleable.TreeView_extendHeight, 32);

        //Load default bitmap
        if(inExtendSrc == -1)
            m_inExtendImg = BitmapFactory.decodeResource(getResources(), R.drawable.in_expend);
        else
            m_inExtendImg = BitmapFactory.decodeResource(getResources(), inExtendSrc);

        if(notExtendSrc == -1)
            m_notExtendImg = BitmapFactory.decodeResource(getResources(), R.drawable.not_expend);
        else
            m_notExtendImg = BitmapFactory.decodeResource(getResources(), notExtendSrc);
        ta.recycle();

        m_tree = new Tree<>(null);
        m_tree.addListener(this);
        setWillNotDraw(false);

        m_paint = new Paint();
        setStrokeWidth(m_strokeWidth);
    }

    @Override
    public MarginLayoutParams generateLayoutParams(AttributeSet attrs) {
        return new MarginLayoutParams(getContext(), attrs);
    }

    @Override
    protected MarginLayoutParams generateDefaultLayoutParams() {
        return new MarginLayoutParams(LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
    }

    @Override
    protected ViewGroup.LayoutParams generateLayoutParams(ViewGroup.LayoutParams p) {
        return new LayoutParams(p);
    }

    @Override
    protected boolean checkLayoutParams(ViewGroup.LayoutParams p) {
        return p != null;
    }

    /**  Measure each leaf size and aggregate the different leaf sizes
     * @param leftOffset the left offset for the current leaf
     * @param state the layout state (current width, height and measure specifications)
     * @param extend is the parent extended?
     * @param t the current leaf to look at*/
    protected TreeViewMeasureState onMeasureLeaf(int leftOffset, TreeViewMeasureState state, boolean extend, Tree<View> t)
    {
        if(t.value != null && t.value.getVisibility() == GONE)
            return state;

        if(t.value != null && extend)
        {
            if(t.getChildren().size() > 0)
                leftOffset += m_extendWidth;

            int maxWidth  = Math.max(0, state.maximumWidth-leftOffset);
            int maxHeight = Math.max(0, state.maximumHeight-state.topOffset);

            measureChild(t.value, MeasureSpec.makeMeasureSpec(maxWidth, state.widthMode), MeasureSpec.makeMeasureSpec(maxHeight, state.heightMode));

            state.height = Math.max(state.topOffset+t.value.getMeasuredHeight(), state.height);
            state.width  = Math.max(leftOffset+t.value.getMeasuredWidth(), state.width);
            state.topOffset += t.value.getMeasuredHeight() + m_topOffsetPerChild;

            leftOffset += m_leftOffsetPerLevel;
            state.measureState = combineMeasuredStates(state.measureState, t.value.getMeasuredState());

            if(t.getChildren().size() > 0)
                state.topOffset += Math.max(m_extendHeight - t.value.getMeasuredHeight(), 0.0);
        }

        extend = extend && t.getExtend();
        for(Tree<View> l : t.getChildren())
            state = onMeasureLeaf(leftOffset, state, extend, l);

        return state;
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        //Define the state used for the leaves and retrieve the width/height of this Layout
        TreeViewMeasureState state = new TreeViewMeasureState();
        state.width  = getSuggestedMinimumWidth();
        state.height = getSuggestedMinimumHeight();
        state.measureState   = 0;
        state.widthMode      = MeasureSpec.getMode(widthMeasureSpec);
        state.maximumWidth   = Math.max(0, MeasureSpec.getSize(widthMeasureSpec) - getPaddingRight());
        state.heightMode     = MeasureSpec.getMode(heightMeasureSpec);
        state.maximumHeight  = Math.max(0, MeasureSpec.getSize(heightMeasureSpec) - getPaddingBottom());
        state.topOffset      = getPaddingTop();

        if(state.widthMode == MeasureSpec.EXACTLY)
            state.widthMode = MeasureSpec.AT_MOST;
        if(state.heightMode == MeasureSpec.EXACTLY)
            state.heightMode = MeasureSpec.AT_MOST;
        onMeasureLeaf(getPaddingLeft(), state, true, m_tree);
        setMeasuredDimension(resolveSizeAndState(state.width, widthMeasureSpec, state.measureState),
                             resolveSizeAndState(state.height, heightMeasureSpec, state.measureState << MEASURED_HEIGHT_STATE_SHIFT));
    }

    /** Layout a leaf
     * @param b See "onLayout" b parameter
     * @param leftMargin the left margin to apply
     * @param topMargin the top margin to apply
     * @param left the left part of this layout
     * @param top the top part of this layout
     * @param right the right part of this layout
     * @param bottom the bottom part of this layout
     * @param leaf the leaf to layout
     * @param extend Is the parent extended?
     * @return the new top margin to apply*/
    private int onLayoutLeaf(boolean b, int leftMargin, int topMargin,
                              int left, int top, int right, int bottom, Tree<View> leaf, boolean extend)
    {
        final View child = leaf.value;

        if(child != null && child.getVisibility() == GONE)
            return topMargin;

        if(child != null && extend )
        {
            final int width = child.getMeasuredWidth();
            final int height = child.getMeasuredHeight();

            if(child.getParent() != this)
                return topMargin;

            //Place for the expend logo
            if(leaf.getChildren().size() > 0)
            {
                leftMargin += m_extendWidth;
                topMargin  += Math.max(m_extendHeight-height, 0.0);
            }

            // These are the far left and right edges in which we are performing layout.
            int leftPos  = getPaddingLeft()+leftMargin;
            int rightPos = right - left - getPaddingRight();

            // These are the top and bottom edges in which we are performing layout.
            final int parentTop = getPaddingTop()+topMargin;
            final int parentBottom = bottom - top - getPaddingBottom();

            // Place the child.
            child.layout(leftPos, parentTop, Math.min(leftPos + width, rightPos), Math.min(parentTop + height, parentBottom));
            topMargin += height + m_topOffsetPerChild;
        }

        extend = extend && leaf.getExtend();
        for(Tree<View> l : leaf.getChildren())
            topMargin = onLayoutLeaf(b, leftMargin + (child != null ? m_leftOffsetPerLevel: 0),
                                     topMargin, left, top, right, bottom, l, extend);
        return topMargin;
    }

    @Override
    protected void onLayout(boolean b, int left, int top, int right, int bottom)
    {
        onLayoutLeaf(b, 0, 0, left,
                     top, right, bottom, m_tree, true);
    }

    @Override
    public void onDraw(Canvas canvas)
    {
        super.onDraw(canvas);
        m_paint.setColor(Color.GRAY);
        drawLeaf(canvas, m_tree);
    }

    /** Draw a leaf on screen
     * @param canvas the canvas to draw on
     * @param tree the tree to draw. Actually, this function draws the line and the extends images*/
    public void drawLeaf(Canvas canvas, Tree<View> tree)
    {
        if(tree.value != null && tree.value.getVisibility() == View.GONE)
            return;

        //Draw extend image
        if(tree.getChildren().size() > 0 && tree.value != null)
        {
            Bitmap b = (tree.getExtend() ? m_inExtendImg : m_notExtendImg);

            canvas.drawBitmap((tree.getExtend() ? m_inExtendImg : m_notExtendImg),
                    new Rect(0, 0, b.getWidth(), b.getHeight()),

                    new Rect((int)(tree.value.getX() - m_extendWidth),               (int)(tree.value.getY()+(tree.value.getHeight()-m_extendHeight)/2.0f),
                             (int)(tree.value.getX() - m_extendWidth+m_extendWidth), (int)(tree.value.getY()+(tree.value.getHeight()-m_extendHeight)/2.0f+m_extendHeight)), m_paint);
        }

        if(tree.getExtend())
        {
            for (Tree<View> t : tree.getChildren())
            {
                if (tree.value != null && t.value != null && t.value.getVisibility() != View.GONE)
                {
                    canvas.drawLine(tree.value.getX() - m_extendWidth / 2.0f, tree.value.getY() + Math.max(tree.value.getHeight(), m_extendHeight),
                                    tree.value.getX() - m_extendWidth / 2.0f, t.value.getY() + t.value.getHeight() / 2, m_paint);


                    canvas.drawLine(tree.value.getX() - m_extendWidth / 2.0f, t.value.getY() + t.value.getHeight() / 2,
                                    t.value.getX() - (t.getChildren().size() > 0 ? m_extendWidth / 2.0f : 0.0f), t.value.getY() + t.value.getHeight() / 2, m_paint);
                }
                drawLeaf(canvas, t);
            }
        }
    }

    /**  register this object to the 'child' children listeners
     * @param child the child to look at*/
    private void recursiveOnAddChildren(Tree<View> child)
    {
        if(child.value != null)
            addView(child.value);
        child.addListener(this);

        for(Tree<View> v : child.getChildren())
            recursiveOnAddChildren(v);
    }

    @Override
    public void onAddChild(Tree<View> parent, Tree<View> child)
    {
        recursiveOnAddChildren(child);
        invalidate();
    }

    /**  remove recursively a child and its children
     * @param child the child to look at*/
    private void recursiveOnRemoveChildren(Tree<View> child)
    {
        if(child.value != null)
            removeView(child.value);
        child.removeListener(this);

        for(Tree<View> v : child.getChildren())
            recursiveOnRemoveChildren(v);
    }

    @Override
    public void onRemoveChild(Tree<View> parent, Tree<View> child)
    {
        recursiveOnRemoveChildren(child);
        invalidate();

    }

    /** Set the extendability of a leaf
     * @param tree the leaf to look at
     * @param extend Is the parent extended?*/
    public void onSetExtendLeaf(Tree<View> tree, boolean extend)
    {
        if(tree.value != null)
        {
            if(!extend)
                tree.value.setVisibility(GONE);
            else
                tree.value.setVisibility(VISIBLE);
        }

        //Commit the extend state
        extend = extend && tree.getExtend();
        for(Tree<View> t : tree.getChildren())
        {
            onSetExtendLeaf(t, extend);
        }
    }

    @Override
    public void onSetExtend(Tree<View> tree, boolean extend)
    {
        //Search if we should really extend this leaf...
        if(extend)
        {
            for(Tree<View> parent = tree.getParent(); parent != null; parent = parent.getParent())
                if(!parent.getExtend())
                {
                    extend = false;
                    break;
                }
        }

        for(Tree<View> t : tree.getChildren())
        {
            onSetExtendLeaf(t, extend);
        }
        invalidate();
    }

    /**  Set the stroke width of this TreeView
     * @param strokeWidth the stroke width*/
    public void setStrokeWidth(int strokeWidth)
    {
        m_strokeWidth = strokeWidth;
        m_paint.setStrokeWidth(strokeWidth);
        invalidate();
    }

    /** Set the size of the extend bitmap
     * @param width the new extend bitmap width
     * @param height the new extend bitmap height*/
    public void setExtendSize(int width, int height)
    {
        m_extendWidth  = width;
        m_extendHeight = height;
    }

    /** Get the extend bitmap width
     * @return the extend bitmap width*/
    public int getExtendWidth()
    {
        return m_extendWidth;
    }

    /** Get the extend bitmap height
     * @return the extend bitmap height*/
    public int getExtendHeight()
    {
        return m_extendHeight;
    }

    private boolean onTouchEventLeaf(Tree<View> tree, float x, float y)
    {
        boolean changed = false;
        if(tree.value != null)
        {
            //Test collision
            if(tree.value.getX() - m_extendWidth < x && tree.value.getX() > x &&
               tree.value.getY() + (tree.value.getHeight()-m_extendHeight)/2.0 < y &&
               tree.value.getY() + (tree.value.getHeight()+m_extendHeight)/2.0 > y)
            {
                changed = true;
                tree.setExtend(!tree.getExtend());
            }
        }

        if(changed)
            return true;

        for(Tree<View> t : tree.getChildren())
            if(onTouchEventLeaf(t, x, y))
                return true;

        return false;
    }

    @Override
    public boolean onTouchEvent(MotionEvent event)
    {
        boolean changed = false; //We test ALL the fingers and do not stop at the first one hit (others may also hit the target)!

        if(event.getAction() == MotionEvent.ACTION_DOWN)
        {
            for(int i = 0; i < event.getPointerCount(); i++)
            {
                int pointID = event.getPointerId(i);
                if(onTouchEventLeaf(m_tree, event.getX(pointID), event.getY(pointID)))
                    changed = true;
            }
        }

        return changed;
    }

    public Tree<View> getModel()
    {
        return m_tree;
    }
}