package com.alhambra.dataset;

import com.alhambra.ListenerSubscriber;
import com.alhambra.Utils;
import com.alhambra.dataset.data.Annotation;

import java.util.ArrayList;
import java.util.HashSet;

public class SelectionData {
    public interface ISelectionDataChange{
        void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups);
        void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups);
        void onGroupClear();
        void onKeywordChange(String text);
    }
    public ListenerSubscriber<ISelectionDataChange> subscriber = new ListenerSubscriber<>();

    private HashSet<Integer> selectedGroups = new HashSet<>();
    private String keywords;
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
    public void setKeyword(String word) {
        keywords=word;
        subscriber.invoke(l->l.onKeywordChange(keywords));
    }

    public boolean containSelectedGroup(int index){
        return selectedGroups.contains(index);
    }

    public ArrayList<Annotation> filter(AnnotationDataset annotationDataset){
        if(selectedGroups.size()==0 && (keywords==null || keywords.equals(""))) {
            return new ArrayList<>();
        }
        ArrayList<Annotation> filtered = new ArrayList<>();
        ArrayList<Annotation> basic;
        if(selectedGroups.size()!=0) {
            basic = annotationDataset.getAnnotationsByGroups(Utils.unbox(selectedGroups));
        }else{
            basic = new ArrayList<>(annotationDataset.getAnnotationList());
        }
        if(keywords==null || keywords.equals("")){
            return basic;
        }else
        {
            for(Annotation a: basic) {
                if(a.info.getText().contains(keywords)) {
                    filtered.add(a);
                }
            }
        }


        return filtered;
    }
}
