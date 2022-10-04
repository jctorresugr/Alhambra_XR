package com.alhambra.fragment;

import androidx.fragment.app.Fragment;

import java.util.ArrayList;

/** The common abstract class for all the fragment of the application*/
public abstract class AlhambraFragment extends Fragment
{
    /** The listener associated with this fragment*/
    public interface IFragmentListener
    {
        /** Method called when the swiping of the tabs should be enabled
         * @param fragment the fragment calling this method*/
        void onEnableSwiping(Fragment fragment);

        /** Method called when the swiping of the tabs should be disabled
         * @param fragment the fragment calling this method*/
        void onDisableSwiping(Fragment fragment);
    }

    public AlhambraFragment()
    {
        super();
    }

    /** The list of registered listeners*/
    private ArrayList<IFragmentListener> m_listeners = new ArrayList<>();

    /** Add a new listener
     * @param l the new listener*/
    public void addListener(IFragmentListener l)
    {
        m_listeners.add(l);
    }

    /** Remove an old listener
     * @param l the listener to remove*/
    public void removeListener(IFragmentListener l)
    {
        m_listeners.remove(l);
    }

    /** Method used by children classes to call "onEnableSwiping" on all the listeners of this fragment*/
    protected void callOnEnableSwiping()
    {
        for(IFragmentListener l : m_listeners)
            l.onEnableSwiping(this);
    }

    /** Method used by children classes to call "onDisableSwiping" on all the listeners of this fragment*/
    protected void callOnDisableSwiping()
    {
        for(IFragmentListener l : m_listeners)
            l.onDisableSwiping(this);
    }
}