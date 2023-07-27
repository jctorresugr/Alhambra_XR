using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class PositionDataSync : SocketDataBasic, PickPano.IPickPanoListener
{
    [Header("Components")]
    public Main main;
    public Camera arCamera;
    public Transform cameraContainer;
    public DataManager data;
    public ReferenceTransform referenceTransform;
    public TeleportAnimation teleportAnimation;
    public AnnotationRenderBuilder annotationRenderBuilder;
    public LayerMask modelLayer;

    [Header("Settings")]
    public float teleportOffset = 1.0f;
    public float upOffset = 1.0f;
    public float updateTime = 0.35f;

    public float tabletControllerSensitivity = 0.05f;
    public float tabletControllerYAxisSensitivity = 0.5f;
    public float tabletControllerDeltaTime = 0.7f;

    public float teleportIndicatorArrowTime = 1.5f;
    public float teleportIndicatorArrowLeastTime = 5.0f;

    private float passTime = 0.0f;

    private float controllerTime = 0.0f;
    private float arrowShowTime = 0.0f;
    private float arrowShowLeastTime = 0.0f;
    private Vector3 controllerLastPosition;
    private Vector3 controllerTotalDelta;


    protected void Start()
    {
        FastReg<TransformClass>(OnReceiveSyncPos);
        FastReg<AnnotationID>(OnReceiveTeleportAnnotation);
        FastReg<int>(OnReceiveTeleportAnnotationJoint);
        FastReg<Vector3>(OnReceiveTabletControllerMove);
        main.PickPanoModel.AddListener(this);
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

        //controller state
        controllerTime -= Time.deltaTime;
        if (controllerTime < 0.0f)
        {
            controllerTime = 0.0f;
            controllerLastPosition = arCamera.transform.position;
            controllerTotalDelta = Vector3.zero;
        }

        //teleport arrow state
        if(arrowShowTime<0&&arrowShowLeastTime<0)
        {
            if (main.IsOnlyArrowSet())
            {
                main.SetArrow(null);
            }
        }
        else
        {
            arrowShowLeastTime -= Time.deltaTime;
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
                    RaycastHit hit;
                    if (Physics.Raycast(worldPos, Vector3.down,out hit,upOffset*4.0f, modelLayer))
                    {
                        worldPos = hit.point + Vector3.up * upOffset;
                    }
                    else
                    {
                        worldPos += renderinfo.Normal * teleportOffset + Vector3.up * upOffset;
                    }
                    TeleportTo(worldPos);
                    main.SetArrow(annotation);
                    arrowShowTime = teleportIndicatorArrowTime;
                    arrowShowLeastTime = teleportIndicatorArrowLeastTime;
                    AnnotationRender annotationRender = annotationRenderBuilder.GetAnnotationUI(annotation.ID);
                    if(annotationRender!=null)
                    {
                        annotationRender.floatPanelText.MarkAsTemporaryHide();
                    }
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
        {
            Vector3 delta = 
                (tabletOffset.x * arCamera.transform.forward +
                tabletOffset.y * arCamera.transform.up * tabletControllerYAxisSensitivity +
                tabletOffset.z * arCamera.transform.right) * tabletControllerSensitivity*Time.deltaTime;
            controllerTotalDelta += delta;

            //TODO: move along the floor, this assume that the floor is flat, do a ray casting if use complex model
            if(tabletOffset.y==0.0f)
            {
                controllerTotalDelta.y = 0.0f;
            }
            

            TeleportTo(controllerTotalDelta + controllerLastPosition);
        }
        

        );

    }

    protected void TeleportToImmediate(Vector3 pos)
    {
        Transform t = cameraContainer.transform;
        Vector3 targetPos = pos - arCamera.transform.localPosition;
        t.position = targetPos;
        if (teleportAnimation != null)
        {
            teleportAnimation.targetPos = targetPos;
        }
    }


    protected void TeleportTo(Vector3 pos)
    {
        Transform t = cameraContainer.transform;
        if(teleportAnimation!=null)
        {
            teleportAnimation.targetPos = pos - arCamera.transform.localPosition;
        }else
        {
            t.position = pos - arCamera.transform.localPosition; //relative position
        }
        
    }

    void PickPano.IPickPanoListener.OnSelection(PickPano pano, Color c)
    {
    }

    void PickPano.IPickPanoListener.OnHover(PickPano pano, Color c)
    {
        
        arrowShowTime -= Time.deltaTime;
    }

    void PickPano.IPickPanoListener.OnSetTexture(PickPano pano, Texture2D newIndexTexture)
    {
    }
}
