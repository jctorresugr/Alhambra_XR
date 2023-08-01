package com.alhambra;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;

import com.google.gson.Gson;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.lang.reflect.Array;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
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

    public static void drawCenterText(Canvas canvas,String text,float x,float y, Paint paint) {
        float textWidth = paint.measureText(text);
        float xOffset = textWidth*0.5f;
        canvas.drawText(text,x-xOffset,y,paint);
    }

    public static Path trianglePath(
            Path result,
            float x0,float y0,
            float x1,float y1,
            float x2,float y2
    ){
        result.setFillType(Path.FillType.EVEN_ODD);
        result.moveTo(x0,y0);
        result.lineTo(x1,y1);
        result.lineTo(x2,y2);
        result.lineTo(x0,y0);
        result.close();
        return result;
    }

    public static String readWholeString(File file) {
        try{
            FileInputStream fis = new FileInputStream(file);
            byte[] data = new byte[(int) file.length()];
            fis.read(data);
            fis.close();

            return new String(data, StandardCharsets.UTF_8);
        }catch (IOException e){
            e.printStackTrace();
        }

        return null;
    }
}
