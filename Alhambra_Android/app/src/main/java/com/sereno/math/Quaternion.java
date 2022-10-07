package com.sereno.math;

import androidx.annotation.NonNull;

/** Quaternion class*/
public class Quaternion
{
    /** The x (i) component*/
    public float x;

    /** The y (j) component*/
    public float y;

    /** The z (k) component*/
    public float z;

    /** The w (real) component*/
    public float w;

    /** Constructor, create a unit quaternion*/
    public Quaternion()
    {
        this(0.0f, 0.0f, 0.0f, 1.0f);
    }

    /** Constructor, create a custom quaternion
     * @param _x the i component
     * @param _y the j component
     * @param _z the k component
     * @param _w the real component*/
    public Quaternion(float _x, float _y, float _z, float _w)
    {
        x = _x;
        y = _y;
        z = _z;
        w = _w;
    }

    /** Constructor, create a custom quaternion based on a float array (w, i, j, k)
     * @param quat the float array*/
    public Quaternion(float[] quat)
    {
        this(quat[1], quat[2], quat[3], quat[0]);
    }

    /** Constructor, create a quaternion from a axis angle
     * @param axis the 3D axis to rotate around
     * @param angle the angle to rotate to*/
    public Quaternion(float[] axis, float angle)
    {
        w = (float)Math.cos(angle/2.0f);
        float s = (float)Math.sin(angle/2.0f);
        x = axis[0]*s;
        y = axis[1]*s;
        z = axis[2]*s;
    }

    /** Get the inverse of this quaternion
     * @return the inverse of this quaternion*/
    public Quaternion getInverse()
    {
        return new Quaternion(-x, -y, -z, w);
    }

    /** Get the multiplication between "this" and x
     * @return this*x */
    public Quaternion multiplyBy(Quaternion q)
    {
        return new Quaternion(w*q.x + x*q.w + y*q.z - z*q.y,  //i part
                              w*q.y - x*q.z + y*q.w + z*q.x,  //j part
                              w*q.z + x*q.y - y*q.x + z*q.w,  //k part
                              w*q.w - x*q.x - y*q.y - z*q.z); //The real part
    }

    /** Rotate a vector v via this quaternion
     * @param v the vector to rotate. The float array should be of size 3 (or more)
     * @return a new vector in a float[3] {x, y, z} array.*/
    public float[] rotateVector(float[] v)
    {
        Quaternion invQ  = getInverse();
        Quaternion qPure = new Quaternion(v[0], v[1], v[2], 0);
        Quaternion res   = this.multiplyBy(qPure).multiplyBy(invQ);
        return new float[]{res.x, res.y, res.z};
    }

    /** Create a quaternion that point from sourcePoint to destPoint
     * @param sourcePoint the source 3D point
     * @param destPoint the source 3D point
     * @return the quaternion*/
    public static Quaternion lookAt(float[] sourcePoint, float[] destPoint)
    {
        float[] forwardVector = Vector3.normalise(Vector3.minus(destPoint, sourcePoint));
        float dot = forwardVector[2];

        if (Math.abs(dot - (-1.0f)) < 0.000001f)
        {
            return new Quaternion(new float[]{0.0f, 1.0f, 0.0f}, (float)Math.PI);
        }
        if (Math.abs(dot - (1.0f)) < 0.000001f)
        {
            return new Quaternion();
        }

        float rotAngle = (float) Math.acos(dot);
        float[] rotAxis = Vector3.crossProduct(new float[]{0.0f, 0.0f, 1.0f}, forwardVector);
        rotAxis = Vector3.normalise(rotAxis);
        return new Quaternion(rotAxis, rotAngle);
    }

    /** Convert this quaternion to a float array
     * @return a float array containing {w, x, y, z}*/
    public float[] toFloatArray()
    {
        return new float[]{w, x, y, z};
    }

    @NonNull
    @Override
    public String toString()
    {
        return "[" + w + ", " + x + ", " + y + ", " + z + "]";
    }

    @NonNull
    @Override
    public Object clone()
    {
        return new Quaternion(x, y, z, w);
    }
}
