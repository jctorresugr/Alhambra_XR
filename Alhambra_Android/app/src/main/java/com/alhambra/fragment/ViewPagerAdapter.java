package com.alhambra.fragment;

import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentPagerAdapter;

import java.util.ArrayList;
import java.util.List;

public class ViewPagerAdapter extends FragmentPagerAdapter
{
    private final List<AlhambraFragment> m_fragmentList      = new ArrayList<>();
    private final List<String>           m_fragmentTitleList = new ArrayList<>();

    public ViewPagerAdapter(FragmentManager manager)
    {
        super(manager);
    }

    @Override
    public Fragment getItem(int position)
    {
        return m_fragmentList.get(position);
    }

    @Override
    public int getCount()
    {
        return m_fragmentList.size();
    }

    public void addFragment(AlhambraFragment fragment, String title)
    {
        m_fragmentList.add(fragment);
        m_fragmentTitleList.add(title);
    }

    @Override
    public CharSequence getPageTitle(int position)
    {
        return m_fragmentTitleList.get(position);
    }

    @Override
    public long getItemId(int position) {
        return super.getItemId(position);
    }
}