package com.alhambra.network.sendingmsg;

public class OverviewMessage {

    public static String generateShowAllJSON()
    {
        return "{\n" +
                "    \"action\": \"showAllAnnotation\",\n" +
                "    \"data\": {}\n" +
                "}";
    }

    public static String generateStopShowAllJSON()
    {
        return "{\n" +
                "    \"action\": \"stopShowAllAnnotation\",\n" +
                "    \"data\": {}\n" +
                "}";
    }
}
