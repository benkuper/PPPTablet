using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnswer : QuizzAnswer,
    ITextureReceiver
{
    RectTransform answerImagePanel;
    RawImage answerImage;

    public override void Awake()
    {
        base.Awake();
        answerImagePanel = transform.FindChild("AnswerImagePanel").GetComponent<RectTransform>();
        answerImage = transform.FindChild("AnswerImagePanel/Texture").GetComponent<RawImage>();
    }

    public override void setData(string gameID, int questionID, string answer, bool isGood)
    {
        base.setData(gameID, questionID, answer, isGood);
        StartCoroutine(AssetManager.loadGameTexture(gameID, "reponses/question"+questionID+"/reponse"+answerID+".jpg", "answerImage", this));

    }

    public void textureReady(string texID, Texture2D tex)
    {
        if (texID == "answerImage")
        {
            float texRatio = (float)tex.width / (float)tex.height;
            Rect qRect = answerImagePanel.rect;
            float tWidth = qRect.width - 16;
            float tHeight = qRect.height - 16;
            float imgRatio = tWidth / tHeight;

            if (texRatio > imgRatio)
            {
                answerImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tWidth);
                answerImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tWidth / texRatio);
            }
            else
            {
                answerImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tHeight * texRatio);
                answerImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tHeight);
            }
        }
        answerImage.color = Color.white;
        answerImage.texture = tex;
    }
}
