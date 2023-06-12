package com.alhambra;

import com.google.gson.Gson;

import java.lang.reflect.Array;
import java.util.ArrayList;
import java.util.HashSet;

public class Utils {

    //make compiler happy, type conversion
    public static int[] unbox(ArrayList<Integer> list) {
        int[] result = new int[list.size()];
        for (int i = 0; i < list.size(); i++) {
            result[i]=list.get(i);
        }
        return result;
    }

    public static int[] unbox(HashSet<Integer> set) {
        int[] result = new int[set.size()];
        int i=0;
        for (int x:set) {
            result[i]=x;
            i++;
        }
        return result;
    }
}
