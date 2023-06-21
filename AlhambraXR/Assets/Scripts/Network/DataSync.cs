
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
        FastReg<List<AnnotationID>>(OnReceiveselection);
        FastReg<HighlightMessage>(OnReceivehighlight);
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

    [Serializable]
    public struct MessageUpdateAnnotationRenderInfo
    {
        [SerializeField]
        public AnnotationID id;
        [SerializeField]
        public AnnotationRenderInfo annotationRenderInfo;

        public MessageUpdateAnnotationRenderInfo(AnnotationID id, AnnotationRenderInfo annotationRenderInfo)
        {
            this.id = id;
            this.annotationRenderInfo = annotationRenderInfo;
        }
    }
    public void SendUpdateAnnotationRenderInfo(MessageUpdateAnnotationRenderInfo annotationRenderInfo)
    {
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name),
            annotationRenderInfo);
    }

    public static string GetUpdateAnnotationRenderInfo(Annotation annotation)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name), 
            new MessageUpdateAnnotationRenderInfo(annotation.ID,annotation.renderInfo));
    }

    public static string GetUpdateModelBounds(Bounds bounds)
    {
        // JsonUtility only seralize class but not struct...
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name),
            new BoundsClass(bounds));
    }

    public static string GetUpdateAnnotationRenderInfo(MessageUpdateAnnotationRenderInfo annotationRenderInfo)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name), annotationRenderInfo);
    }

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

    public void SendAddAnnotationToJoint(int jointID, AnnotationID annotationID)
    {
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name), new MessageAnnotationJointModify(jointID, annotationID));
    }

    public string GetAddAnnotationToJoint(int jointID, AnnotationID annotationID)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name),
            new MessageAnnotationJointModify(jointID, annotationID));
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
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name), new MessageAnnotationJointModify(jointID, annotationID));
    }

    public string GetRemoveAnnotationFromJoint(int jointID, AnnotationID annotationID)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name),
            new MessageAnnotationJointModify(jointID, annotationID));
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
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name), jointID);
    }

    public string GetRemoveAnnotationJoint(int jointID)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name), jointID);
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
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name), aj);
    }

    public string GetAddAnnotationJoint(AnnotationJoint aj)
    {
        return JSONMessage.ActionJSON(ProcessMethodName(MethodBase.GetCurrentMethod().Name), aj);
    }

    //Selection data

    public void OnReceiveselection(List<AnnotationID> aids)
    {
        main.SelectionData.SelectedAnnotations = aids;
    }

    public void OnReceivehighlight(HighlightMessage detailedMsg)
    {
        main.SelectionData.CurrentAction = CurrentAction.IN_HIGHLIGHT;
        main.SelectionData.CurrentHighlightMain = new AnnotationID(detailedMsg.layer, detailedMsg.id);
        main.SelectionData.CurrentHighlightSecond = AnnotationID.INVALID_ID;
    }

}

