using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PositionDataSync : SocketDataBasic
{
    public DataManager data;
    public ReferenceTransform referenceTransform;

    public float teleportOffset = 1.0f;
    public float upOffset = 1.0f;
    public float updateTime = 0.35f;

    private float passTime = 0.0f;


    protected void Start()
    {
        FastReg<TransformClass>(OnReceiveSyncPos);
        FastReg<AnnotationID>(OnReceiveTeleportAnnotation);
        FastReg<int>(OnReceiveTeleportAnnotationJoint);
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
        TransformClass transformClass = new TransformClass(main.mainCamera.transform);
        transformClass.position = referenceTransform.InvMapPosition(transformClass.position);
        SendClientAction(ProcessMethodName(MethodBase.GetCurrentMethod().Name), transformClass);
    }

    public void OnReceiveSyncPos(TransformClass msg)
    {
        main.mainCamera.transform.position = msg.position;
        main.mainCamera.transform.rotation = Quaternion.Euler(msg.rotation);
    }

    public void OnReceiveTeleportAnnotation(AnnotationID id)
    {
        Annotation annotation = data.FindAnnotationID(id);
        if (annotation != null)
        {
            AnnotationRenderInfo renderinfo = annotation.renderInfo;
            if(renderinfo!=null)
            {
                main.AddTask(() =>
                {
                    Vector3 worldPos = referenceTransform.MapPosition(renderinfo.averagePosition);
                    main.mainCamera.transform.position =
                        worldPos + renderinfo.Normal * teleportOffset + Vector3.up * upOffset;
                    main.mainCamera.transform.LookAt(worldPos);
                });
                
            }
            
        }
    }

    public void OnReceiveTeleportAnnotationJoint(int jointID)
    {
        AnnotationJoint joint = data.FindJointID(jointID);
        if(joint!=null)
        {
            main.AddTask(() =>
            {
                main.mainCamera.transform.position = referenceTransform.MapPosition(joint.position) + Vector3.up * upOffset;
            });
                
        }
    }
}
