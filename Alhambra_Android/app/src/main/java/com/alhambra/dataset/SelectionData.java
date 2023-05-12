package com.alhambra.dataset;

import com.alhambra.ListenerSubscriber;

import java.util.HashSet;

public class SelectionData {
    public interface ISelectionDataChange{
        void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups);
        void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups);
        void onGroupClear();
    }
    public ListenerSubscriber<ISelectionDataChange> subscriber = new ListenerSubscriber<>();

    private HashSet<Integer> selectedGroups = new HashSet<>();
    public void addSelectedGroup(int index) {
        selectedGroups.add(index);
        subscriber.invoke(l->l.onGroupSelectionAdd(index,selectedGroups));
    }

    public void removeSelectedGroup(int index) {
        selectedGroups.remove(index);
        subscriber.invoke(l->l.onGroupSelectionRemove(index,selectedGroups));
    }

    public void clearSelectedGroup(){
        selectedGroups.clear();
        subscriber.invoke(ISelectionDataChange::onGroupClear);
    }

    public boolean containSelectedGroup(int index){
        return selectedGroups.contains(index);
    }
}
