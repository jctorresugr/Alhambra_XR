package com.alhambra.fragment;

import androidx.fragment.app.Fragment;

import java.util.ArrayList;

public class AlhambraFragment extends Fragment
{
    public interface IFragmentListener
    {
        void onEnableSwipping(Fragment fragment);
        void onDisableSwipping(Fragment fragment);
    }

    public AlhambraFragment()
    {
        super();
    }

    /** The list of registered listeners*/
    protected ArrayList<IFragmentListener> m_listeners = new ArrayList<>();

    /** @brief Add a new listener
     * @param l the new listener*/
    public void addListener(IFragmentListener l)
    {
        m_listeners.add(l);
    }

    /** @brief Remove an old listener
     * @param l the listener to remove*/
    public void removeListener(IFragmentListener l)
    {
        m_listeners.remove(l);
    }

    public void callOnEnableSwipping()
    {
        for(IFragmentListener l : m_listeners)
            l.onEnableSwipping(this);
    }

    public void callOnDisableSwipping()
    {
        for(IFragmentListener l : m_listeners)
            l.onDisableSwipping(this);
    }
}