using UnityEngine;
using UnityEngine.UI;

public class ChangeExpressions : MonoBehaviour
{

    public Sprite[] expressionsClose;
    public Sprite[] expressionsOpen;

    public void ChangeExpression(int expressionID, bool isSpeaking)
    {
        if (isSpeaking)
        {
            GetComponent<Image>().sprite = expressionsOpen[expressionID];
        }
        else
        {
            GetComponent<Image>().sprite = expressionsClose[expressionID];
        }
    }
}
