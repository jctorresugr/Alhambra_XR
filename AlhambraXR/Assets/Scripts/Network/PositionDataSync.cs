using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PositionDataSync : SocketDataBasic
{
    public Main main;
    public Camera arCamera;
    public Transform cameraContainer;
    public DataManager data;
    public ReferenceTransform referenceTransform;

    public float teleportOffset = 1.0f;
    public float upOffset = 1.0f;
    public float updateTime = 0.35f;

    public float tabletControllerSensitivity = 0.05f;

    private float passTime = 0.0f;


    protected void Start()
    {
        FastReg<TransformClass>(OnReceiveSyncPos);
        FastReg<AnnotationID>(OnReceiveTeleportAnnotation);
        FastReg<int>(OnReceiveTeleportAnnotationJoint);
        FastReg<Vector3>(OnReceiveTabletControllerMove);
    }


    //update position info frequently
    void Update()
    {
        passTime += Time.deltaTime;
        if (passTime > updateTime)
        {
            passTime = 0.0f;
            SendSyncPos();
        }
    }



    public void SendSyncPos()
    {
        TransformClass transformClass = new TransformClass(arCamera.transform);
        main.AddTask(() =>
        {
            transformClass.position = referenceTransform.InvMapPosition(transformClass.position);
            SendClientAction("SyncPos", transformClass);
        }
        );
    }

    public void OnReceiveSyncPos(TransformClass msg)
    {
        main.AddTask(() =>
           TeleportTo(referenceTransform.MapPosition(msg.position)));
        //cameraContainer.rotation = Quaternion.Euler(msg.rotation);
    }

    public void OnReceiveTeleportAnnotation(AnnotationID id)
    {
        Annotation annotation = data.FindAnnotationID(id);
        if (annotation != null)
        {
            AnnotationRenderInfo renderinfo = annotation.renderInfo;
            if (renderinfo != null)
            {
                main.AddTask(() =>
                {
                    Vector3 worldPos = referenceTransform.MapPosition(renderinfo.averagePosition);
                    TeleportTo(worldPos + renderinfo.Normal * teleportOffset + Vector3.up * upOffset);
                }
                );
            }
            
        }
    }

    public void OnReceiveTeleportAnnotationJoint(int jointID)
    {
        AnnotationJoint joint = data.FindJointID(jointID);
        if(joint!=null)
        {
            main.AddTask(() =>
                TeleportTo(referenceTransform.MapPosition(joint.position) + Vector3.up * upOffset)
                );
        }
    }

    public void OnReceiveTabletControllerMove(Vector3 tabletOffset)
    {
        main.AddTask(() =>
        TeleportTo(
            (
            tabletOffset.x * arCamera.transform.forward +
            tabletOffset.y * arCamera.transform.up +
            tabletOffset.z * arCamera.transform.right
            )
            * tabletControllerSensitivity*Time.deltaTime + arCamera.transform.position)

        );

    }

    protected void TeleportTo(Vector3 pos)
    {
        Transform t = cameraContainer.transform;
        t.position = pos - arCamera.transform.localPosition; //relative position
    }
}
