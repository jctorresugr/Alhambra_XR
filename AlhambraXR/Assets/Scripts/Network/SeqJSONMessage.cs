using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class SeqJSONMessage
{
    public const string ACTION_NAME = "SeqMessage";

    //action sequence, it will receive message by sequence
    [SerializeField]
    public int actionSeq;

    //package nums
    [SerializeField]
    public int packNum;

    //notify begin or end
    [SerializeField]
    public int flag;
    enum ActionTag
    {
        BEGIN=0,
        PROCESSING=1,
        END=2
    }
}
