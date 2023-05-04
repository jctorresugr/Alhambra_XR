
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;



public class DataSync : SocketDataBasic
{
    public DataManager data;

    protected void Start()
    {
        data = main.data;

        FastReg<MessageAnnotationJointModify>(OnReceiveAddAnnotationToJoint);
        FastReg<MessageAnnotationJointModify>(OnReceiveRemoveAnnotationFromJoint);
        FastReg<int>(OnReceiveRemoveAnnotationJoint);
        FastReg<AnnotationJoint>(OnReceiveAddAnnotationJoint);
    }

    // C# cannot infer generic type like C++ :(
    protected void FastReg<T>(Action<T> action)
    {
        string name = action.Method.Name;
        name = name.Replace("OnReceive", "");
        RegReceiveInfo(name, (c, msg) => Parse(c, msg, action));
        Debug.Log("Fast reg socket message: " + name);
    }

    protected void Parse<T>(Client c, string msg, Action<T> func)
    {
        Debug.Log("Process " + msg);
        func(JsonUtility.FromJson<T>(msg));
    }

    protected AnnotationJoint GetAnnotationJoint(int jointID)
    {
        AnnotationJoint annotationJoint = data.FindJointID(jointID);
        if(annotationJoint==null)
        {
            Debug.LogWarning($"Cannot find joint {jointID}");
        }
        return annotationJoint;
    }

    protected Annotation GetAnnotation(AnnotationID id)
    {
        Annotation annotation = data.FindAnnotationID(id);
        if(annotation==null)
        {
            Debug.LogWarning($"Cannot find annotation {id}");
        }
        return annotation;
    }

    protected string ProcessMethodName(string name)
    {
        if(name.StartsWith("OnReceive"))
        {
            return name.Substring(9);
        }
        else if(name.StartsWith("Send"))
        {
            return name.Substring(3);
        }
        Debug.LogWarning("Problem with method name: " + name);
        return name;
    }
    public const string FAIL_NULL_ANNOTATION_OR_JOINT = "Cannot find annotation or annotation joint on the server";
    public const string FAIL_NULL_ANNOTATION_JOINT_LACK = "Annotation joint does not have this annotation";

    [Serializable]
    public struct MessageAnnotationJointModify
    {
        [SerializeField]
        public int jointID;
        [SerializeField]
        public AnnotationID annotationID;

        public MessageAnnotationJointModify(int jointID, AnnotationID annotationID)
        {
            this.jointID = jointID;
            this.annotationID = annotationID;
        }
    }

    // ===========================================================================
    // Sync functions
    // OnReceiveAddAnnotationToJoint() process "AddAnnotationToJoint" information
    // SendAddAnnotationtoJoint() send "AddAnnotationtoJoint" information

    public void OnReceiveAddAnnotationToJoint(MessageAnnotationJointModify msg)
    {
        AnnotationJoint annotationJoint = GetAnnotationJoint(msg.jointID);
        Annotation annotation = GetAnnotation(msg.annotationID);
        if(annotation==null || annotationJoint==null)
        {
            SendClientFailure(FAIL_NULL_ANNOTATION_OR_JOINT, msg);
            return;
        }
        annotationJoint.AddAnnotation(annotation);
    }

    public void SendAddAnnotationtoJoint(int jointID, AnnotationID annotationID)
    {
        SendClientAction(MethodBase.GetCurrentMethod().Name, new MessageAnnotationJointModify(jointID, annotationID));
    }

    public void OnReceiveRemoveAnnotationFromJoint(MessageAnnotationJointModify msg)
    {
        AnnotationJoint annotationJoint = GetAnnotationJoint(msg.jointID);
        Annotation annotation = GetAnnotation(msg.annotationID);
        if (annotation == null || annotationJoint == null)
        {
            SendClientFailure(FAIL_NULL_ANNOTATION_OR_JOINT, msg);
            return;
        }
        if(!annotationJoint.HasAnnotation(msg.annotationID))
        {
            SendClientFailure(FAIL_NULL_ANNOTATION_JOINT_LACK, msg);
            return;
        }
        annotationJoint.RemoveAnnotation(annotation);
    }

    public void SendRemoveAnnotationFromJoint(int jointID, AnnotationID annotationID)
    {
        SendClientAction(MethodBase.GetCurrentMethod().Name, new MessageAnnotationJointModify(jointID, annotationID));
    }

    public void OnReceiveRemoveAnnotationJoint(int jointID)
    {
        AnnotationJoint annotationJoint = GetAnnotationJoint(jointID);
        if(annotationJoint==null)
        {
            SendClientFailure(FAIL_NULL_ANNOTATION_OR_JOINT,jointID);
            return;
        }
        data.RemoveAnnotationJoint(jointID);
    }

    public void SendRemoveAnnotationJoint(int jointID)
    {
        SendClientAction(MethodBase.GetCurrentMethod().Name, jointID);
    }

    public void OnReceiveAddAnnotationJoint(AnnotationJoint aj)
    {
        AnnotationJoint annotationJoint = data.FindJointID(aj.ID);
        if(annotationJoint!=null)
        {
            SendClientFailure("Id duplication!", aj);
            return;
        }
        data.AddAnnotationJoint(aj);
    }

    public void SendAddAnnotationJoint(AnnotationJoint aj)
    {
        SendClientAction(MethodBase.GetCurrentMethod().Name, aj);
    }

}

