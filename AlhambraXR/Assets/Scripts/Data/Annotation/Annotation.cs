using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Annotation
{
    public AnnotationInfo info = null;
    public AnnotationRenderInfo renderInfo = new AnnotationRenderInfo();
    public AnnotationID ID
    {
        get;
    }

    public Annotation(AnnotationID _id)
    {
        ID = _id;
    }

}
