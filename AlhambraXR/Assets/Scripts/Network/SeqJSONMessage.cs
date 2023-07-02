using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SeqJSONMessage
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
    public enum ActionTag
    {
        BEGIN=0,
        PROCESSING=1,
        END=2
    }

    public string data;

    public SeqJSONMessage(int actionSeq, int packNum, ActionTag flag, string data)
    {
        this.actionSeq = actionSeq;
        this.packNum = packNum;
        this.flag = (int)flag;
        this.data = data;
    }

    public static List<string> GenerateSeqJSON(
        PackedJSONMessages msgs, int packNum)
    {
        List<string> results = new List<string>();
        for (int i = 0; i < msgs.actions.Count; i++)
        {
            ActionTag flag = ActionTag.PROCESSING;
            if (i == msgs.actions.Count - 1)
            {
                flag = ActionTag.END;
            }else if (i==0)
            {
                flag = ActionTag.BEGIN;
            }
            SeqJSONMessage s = new SeqJSONMessage(i, packNum, flag, msgs.actions[i]);
            results.Add(JSONMessage.ActionJSON(ACTION_NAME, s));
        }
        return results;

    }
}
