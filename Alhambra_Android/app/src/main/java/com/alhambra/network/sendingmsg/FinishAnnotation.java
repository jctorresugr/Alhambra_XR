package com.alhambra.network.sendingmsg;

import android.graphics.Point;

import com.sereno.math.Quaternion;
import com.sereno.math.Vector3;
import com.sereno.view.AnnotationGeometry;
import com.sereno.view.AnnotationPolygon;
import com.sereno.view.AnnotationStroke;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.ArrayList;
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
        res.append(SendingMessage.generateIncr(incr+4)).append("\"width\": ").append(stroke.getWidth()).append(",\n");
        res.append(SendingMessage.generateIncr(incr+4)).append("\"points\": [");
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

    /** Generate the object associated with one annotation polygons
     * @param polygon the polygon to convert to JSON. It must be valid
     * @param incr  the current increment in the overall JSON message
     * @return the JSON string associated with the polygon, starting and ending curly brackets included.*/
    private static String generatePolygonObjectJSON(AnnotationPolygon polygon, int incr)
    {
        StringBuilder res = new StringBuilder();
        List<Point> points = polygon.getPoints();

        res.append(SendingMessage.generateIncr(incr)).append("{\n");
        res.append(SendingMessage.generateIncr(incr+4)).append("\"points\": [");
        for(int i = 0; i < points.size()-1; i++)
            res.append(points.get(i).x).append(", ").append(points.get(i).y).append(", ");
        res.append(points.get(points.size()-1).x).append(", ").append(points.get(points.size()-1).y).append("]\n");
        res.append(SendingMessage.generateIncr(incr)).append("}");

        return res.toString();
    }

    /** Generate the JSON array of annotation polygons
     * @param polygons the list of polygons to convert to JSON
     * @param incr the current incrementation in the overall JSON message
     * @return the JSON string. Starting and ending brackets are included.*/
    private static String generatePolygonsJSON(List<AnnotationPolygon> polygons, int incr)
    {
        StringBuilder res = new StringBuilder("[");

        if(polygons.size() > 0)
        {
            res.append("\n");
            for(int i = 0; i < polygons.size()-1; i++)
                res.append(generatePolygonObjectJSON(polygons.get(i), incr+4)).append(",\n");
            res.append(generatePolygonObjectJSON(polygons.get(polygons.size()-1), incr+4));
            res.append(SendingMessage.generateIncr(incr)).append("\n");
        }
        res.append("]");
        return res.toString();
    }

    /** Generate the JSON message of "finishAnnotation"
     * @param confirm is the annotation validated (true) or cancelled (false)?
     * @param geometries the geometries of the annotation
     * @param width the annotated image width
     * @param height the annotated image height
     * @param desc the annotation description
     * @param cameraPos the camera position at the time of where the annotated image was taken
     * @param cameraRot the camera orientation at the time of where the annotated image was taken
     * @return the JSON message, curly brackets included.*/
    public static String generateJSON(boolean confirm, List<AnnotationGeometry> geometries, int width, int height, String desc, float[] cameraPos, Quaternion cameraRot)
    {
        List<AnnotationStroke>  strokes = new ArrayList<>();
        List<AnnotationPolygon> polygons = new ArrayList<>();
        for(AnnotationGeometry g : geometries)
        {
            if(!g.isValid())
                continue;
            if(g instanceof AnnotationStroke)
                strokes.add((AnnotationStroke)g);
            else if(g instanceof AnnotationPolygon)
                polygons.add((AnnotationPolygon)g);
        }

        return "{\n" +
                "   \"action\": \"finishAnnotation\",\n" +
                "   \"data\": {\n" +
                "       \"cameraPos\": " + Vector3.toString(cameraPos) + ",\n" +
                "       \"cameraRot\": " + cameraRot.toString() + ",\n" +
                "       \"confirm\": " + (confirm ? "true" : "false") + ",\n" +
                "       \"description\": " + JSONObject.quote(desc) + ",\n" +
                "       \"width\": " + width + ",\n" +
                "       \"height\": " + height + ",\n" +
                "       \"strokes\": " + generateStrokesJSON(strokes, 8) + ",\n" +
                "       \"polygons\": " + generatePolygonsJSON(polygons, 8) + "\n" +
                "   }\n" +
                "}";
    }
}
