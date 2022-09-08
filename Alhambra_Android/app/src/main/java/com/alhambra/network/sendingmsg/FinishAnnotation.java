package com.alhambra.network.sendingmsg;

import android.graphics.Point;

import com.sereno.view.AnnotationStroke;

import java.util.List;

/** Generate the JSON message for the 'end annotation' action*/
public class FinishAnnotation
{
    /** Generate the object associated with one annotation stroke
     * @param stroke the stroke to convert to JSON
     * @param incr  the current increment in the overall JSON message
     * @return the JSON string associated with the stroke, starting and ending curly brackets included.*/
    private static String generateStrokeObjectJSON(AnnotationStroke stroke, int incr)
    {
        StringBuilder res = new StringBuilder();
        List<Point> points = stroke.getPoints();

        res.append(SendingMessage.generateIncr(incr)).append("{\n");
        res.append(SendingMessage.generateIncr(incr+4)).append("\"point\": [");
        for(int i = 0; i < points.size()-1; i++)
            res.append(points.get(i).x).append(", ").append(points.get(i).y).append(", ");
        res.append(points.get(points.size()-1).x).append(", ").append(points.get(points.size()-1).y).append("]\n");
        res.append(SendingMessage.generateIncr(incr)).append("}");

        return res.toString();
    }

    /** Generate the JSON array of annotation strokes
     * @param strokes the list of strokes to convert to JSON
     * @param incr the current incrementation in the overall JSON message
     * @return the JSON string. Starting and ending brackets are included.*/
    private static String generateStrokesJSON(List<AnnotationStroke> strokes, int incr)
    {
        StringBuilder res = new StringBuilder("[");

        if(strokes.size() > 0)
        {
            res.append("\n");
            for(int i = 0; i < strokes.size()-1; i++)
                res.append(generateStrokeObjectJSON(strokes.get(i), incr+4)).append(",\n");
            res.append(generateStrokeObjectJSON(strokes.get(strokes.size()-1), incr+4));
            res.append(SendingMessage.generateIncr(incr)).append("\n");
        }
        res.append("]");
        return res.toString();
    }

    /** Generate the JSON message of "finishAnnotation"
     * @param confirm is the annotation validated (true) or cancelled (false)?
     * @param strokes the strokes of the annotation
     * @return the JSON message, curly brackets included.*/
    public static String generateJSON(boolean confirm, List<AnnotationStroke> strokes)
    {
        return "{\n" +
                "   \"action\": \"finishAnnotation\",\n" +
                "   \"data\": {\n" +
                "       \"confirm\": " + (confirm ? "true" : "false") + ",\n" +
                "       \"strokes\": " + generateStrokesJSON(strokes, 8) + "\n" +
                "   }\n" +
                "}";
    }
}