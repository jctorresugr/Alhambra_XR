package com.alhambra;

import java.util.ArrayList;
import java.util.function.Consumer;

public class ListenerSubscriber<T> {

    @FunctionalInterface
    public interface ParameterRunnable<T> {

        void run(T listener);

    }
    ArrayList<T> listeners = new ArrayList<>();
    public void addListener(T listener) {
        listeners.add(listener);
    }

    public void removeListener(T listener) {
        listeners.remove(listener);
    }

    public void clearListener(){
        listeners.clear();
    }

    public void invoke(ParameterRunnable<T> invokes){
        for(T listener: listeners) {
            invokes.run(listener);
        }
    }
}
