using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System;

public class DechetCrush : Game, ITextureReceiver,
    IPointerDownHandler,
    IPointerUpHandler
{
    public enum Direction { UP, DOWN, LEFT, RIGHT };
    public enum Association { UP, DOWN, LEFT, RIGHT, MIDDLE_VERTICAL, MIDDLE_HORIZONTAL, NONE };

    //from config.json
    public float tempsJeu;
    public int tailleGrille;
    public float swipeDistance;
    public float swipeTime;
    public int maxAssociationParDechet;
    public float tempsVidageRessources;	
	public float pourcentGagneParAssociation;
    
    float ressourceCurrentValue;

    public GameObject candyPrefab;
    public GameObject associationPrefab;

    Transform candyContainer;

    int numItems;
    int numDechets;

    float timeAtLaunch;

    int associationsScore; //score

    public List<DechetData> dechets;

    public CandyDechet[] candies;
    public Dictionary<string,Image> associationBars;
    
    public Dictionary<string, int> associations;

    public Texture[] dechetTextures;

    RawImage messageImage;
    Text timeText;
    Text ressourcesText;
    Text associationsText;
    Image ressourceValueBar;
    Transform associationPanel;
    Text scoreText;
     
    CandyDechet currentCandy;
    Vector3 screenPosAtDown;

    public bool animating;


    public bool messageIsLocked;
    public string[] associationMessageImages;

    bool ressourceUnder20;
    bool ressourceAt0;

    public float minAssociationBarHeight;

    // Use this for initialization
    override public void Awake()
    {
        base.Awake();

        candyContainer = transform.Find("Canvas/Candies");
        messageImage = transform.Find("Canvas/MessagePanel/MessageImage").GetComponent<RawImage>();
        timeText = transform.Find("Canvas/TimePanel/Text").GetComponent<Text>();
        ressourcesText = transform.Find("Canvas/RessourcesPanel/Text").GetComponent<Text>();
        associationsText = transform.Find("Canvas/AssociationsPanel/Text").GetComponent<Text>();
        ressourceValueBar = transform.Find("Canvas/RessourcesBar/Value").GetComponent<Image>();
        associationPanel = transform.Find("Canvas/AssociationsBar");
        scoreText = transform.Find("Canvas/ScorePanel/Text").GetComponent<Text>();

        messageImage.transform.localScale = Vector3.zero;
    }

    public override void startGame()
    {
        base.startGame();

        associationMessageImages = AssetManager.getImagesInGameFolder(id, "messages/association");

        ressourceCurrentValue = 1;
        timeAtLaunch = Time.time;
        associationsScore = 0;

        numItems = tailleGrille * tailleGrille;
        numDechets = dechets.Count;

        candies = new CandyDechet[numItems];
        dechetTextures = new Texture[numDechets];
        associations = new Dictionary<string, int>();
        associationBars = new Dictionary<string,Image>();

        for (int i = 0; i < numDechets; i++)
        {
            
            StartCoroutine(AssetManager.loadGameTexture(id, "dechets/" + dechets[i].id + ".png", "tex_" + i, this));

            associations[dechets[i].id] = 0;

            Color c;
            ColorUtility.TryParseHtmlString(dechets[i].couleur, out c);

                
            Image bar = Instantiate(associationPrefab).GetComponent<Image>();
            associationBars[dechets[i].id] = bar;
            float parentWidth = associationPanel.transform.GetComponent<RectTransform>().rect.width;
            float parentHeight = associationPanel.transform.GetComponent<RectTransform>().rect.height;
            bar.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentWidth / numDechets);
            bar.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minAssociationBarHeight);
            bar.transform.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(i * parentWidth / numDechets, 0, 0),Quaternion.identity);
            bar.transform.GetComponent<RectTransform>().SetPositionAndRotation(new Vector3(i * parentWidth / numDechets, 0, 0), Quaternion.identity);
            bar.transform.Find("DechetImage").GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, bar.transform.Find("DechetImage").GetComponent<RectTransform>().rect.width);
            bar.transform.SetParent(associationPanel.transform, false);
            bar.color = c;
        }

        fillGrid();
    }

    void fillGrid()
    {
        for (int i = 0; i < numItems; i++)
        {
            if (candies[i] == null)
            {
                int tx = i % tailleGrille;
                int ty = Mathf.FloorToInt(i / tailleGrille);

                int tid = UnityEngine.Random.Range(0, numDechets);

                while (candyIsAligned(dechets[tid].id, tx, ty, true) != Association.NONE)
                {
                    tid = UnityEngine.Random.Range(0, numDechets);
                }

                Texture t = dechetTextures[tid];
                DechetData d = dechets[tid];
                CandyDechet c = Instantiate(candyPrefab).GetComponent<CandyDechet>();
                // c.candyTouchDown += candyTouchDown;

                c.transform.SetParent(candyContainer, false);

                c.setData(d, t);
                c.setBlock(tx, ty, tailleGrille, 2);
                candies[i] = c;
            }
        }
    }



    public override void Update()
    {
        base.Update();

        if (!isPlaying) return;

        if (Input.GetMouseButton(0))
        {
            if (currentCandy != null)
            {
                Vector2 d = Input.mousePosition - screenPosAtDown;
                if (Mathf.Abs(d.x) > swipeDistance)
                {
                    swipeCandy(currentCandy, d.x > 0 ? Direction.RIGHT : Direction.LEFT);
                    setCurrentCandy(null);
                }
                else if (Mathf.Abs(d.y) > swipeDistance)
                {
                    swipeCandy(currentCandy, d.y > 0 ? Direction.UP : Direction.DOWN);
                    setCurrentCandy(null);
                }
            }
        }


        float timeLeft = timeAtLaunch + tempsJeu - Time.time;

        if (timeLeft <= 0) endGame();
        
        timeText.text = StringUtil.timeToCountdownString(timeLeft);

        ressourceCurrentValue -= Time.deltaTime / tempsVidageRessources;
        ressourceCurrentValue = Mathf.Max(ressourceCurrentValue, 0);

        if (ressourceCurrentValue == 0)
        {
            if (!ressourceAt0)
            {
                showMessage("ressources/0", 2);
                ressourceAt0 = true;
                AudioPlayer.instance.play("wrong");
            }
        }
        else
        {
            ressourceAt0 = false;
            if (ressourceCurrentValue < .2f)
            {
                if (!ressourceUnder20)
                {
                    showMessage("ressources/20", 2);
                    ressourceUnder20 = true;
                    AudioPlayer.instance.play("wrong");

                }
            }
            else
            {
                ressourceUnder20 = false;
            }
        }

        int p100 = (int)(ressourceCurrentValue * 100);
        ressourcesText.text = p100 + "%";
        ressourceValueBar.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ressourceCurrentValue * ressourceValueBar.transform.parent.GetComponentInParent<RectTransform>().rect.height);
    }



    public CandyDechet getCandy(int tx, int ty)
    {
        return candies[ty * tailleGrille + tx];
    }


    Association candyIsAligned(CandyDechet c, bool searchForPreviousOnly = false)
    {
        return candyIsAligned(c.id, c.x, c.y, searchForPreviousOnly);
    }

    Association candyIsAligned(string tid, int tx, int ty, bool searchForPreviousOnly)
    {
        if (tx >= 2 && getCandy(tx - 1, ty).id == tid && getCandy(tx - 2, ty).id == tid) return Association.LEFT;
        if (ty >= 2 && getCandy(tx, ty - 1).id == tid && getCandy(tx, ty - 2).id == tid) return Association.UP;

        if (searchForPreviousOnly) return Association.NONE;

        if (tx >= 1 && tx <= tailleGrille - 2 && getCandy(tx - 1, ty).id == tid && getCandy(tx + 1, ty).id == tid) return Association.MIDDLE_HORIZONTAL;
        if (ty >= 1 && ty <= tailleGrille - 2 && getCandy(tx, ty - 1).id == tid && getCandy(tx, ty + 1).id == tid) return Association.MIDDLE_VERTICAL;

        if (tx <= tailleGrille - 3 && getCandy(tx + 1, ty).id == tid && getCandy(tx + 2, ty).id == tid) return Association.RIGHT;
        if (ty <= tailleGrille - 3 && getCandy(tx, ty + 1).id == tid && getCandy(tx, ty + 2).id == tid) return Association.DOWN;

        return Association.NONE;
    }

    void swipeCandy(CandyDechet c, Direction d)
    {
        int tx = c.x, ty = c.y;
        switch (d)
        {
            case Direction.UP:
                if (c.y == 0) return;
                ty--;
                break;

            case Direction.DOWN:
                if (c.y == tailleGrille - 1) return;
                ty++;
                break;

            case Direction.LEFT:
                if (c.x == 0) return;
                tx--;
                break;

            case Direction.RIGHT:
                if (c.x == tailleGrille - 1) return;
                tx++;
                break;
        }

        animating = true;

        CandyDechet tc = getCandy(tx, ty);
        tc.moveToBlock(c.x, c.y, swipeTime);
        c.moveToBlock(tx, ty, swipeTime);

        candies[c.y * tailleGrille + c.x] = c;
        candies[tc.y * tailleGrille + tc.x] = tc;


        StartCoroutine(checkAssociateCandies(swipeTime + .2f));
    }

    IEnumerator checkAssociateCandies(float delay)
    {
        
        yield return new WaitForSeconds(delay);

        bool found = false;
        
        for (int i=0;i<numItems;i++)
        {
            CandyDechet c = candies[i];
            Association d = candyIsAligned(c, true);
            if (d != Association.NONE)
            {
                associateCandies(c, d);
                StartCoroutine(checkAssociateCandies(swipeTime*2f));
                found = true;
                 break;
            }
        }

        if(!found)
        {
            animating = false;
        }
       
    }

    void associateCandies(CandyDechet c, Association d)
    {
        
        CandyDechet c1 = null, c2 = null;

        switch (d)
        {
            case Association.UP:
                c1 = getCandy(c.x, c.y - 1);
                c2 = getCandy(c.x, c.y - 2);
                break;

            case Association.DOWN:
                c1 = getCandy(c.x, c.y + 1);
                c2 = getCandy(c.x, c.y + 2);
                break;
            case Association.LEFT:
                c1 = getCandy(c.x - 1, c.y);
                c2 = getCandy(c.x - 2, c.y);
                break;
            case Association.RIGHT:
                c1 = getCandy(c.x + 1, c.y);
                c2 = getCandy(c.x + 2, c.y);
                break;

            case Association.MIDDLE_HORIZONTAL:
                c1 = getCandy(c.x - 1, c.y);
                c2 = getCandy(c.x + 1, c.y);
                break;

            case Association.MIDDLE_VERTICAL:
                c1 = getCandy(c.x, c.y - 1);
                c2 = getCandy(c.x, c.y + 1);
                break;
        }


        incrementAssociation(c.id);
        removeCandiesTriplet(new CandyDechet[] { c, c1, c2 }, d);
    }

    public void removeCandiesTriplet(CandyDechet[] cc, Association d)
    {
        if (d == Association.LEFT || d == Association.RIGHT || d == Association.MIDDLE_HORIZONTAL)
        {
            for (int i = 0; i < cc.Length; i++)
            {
                for (int j = cc[i].y - 1; j >= 0; j--)
                {
                    CandyDechet c = getCandy(cc[i].x, j);
                    if (c != null)
                    {
                        c.moveToBlock(c.x, c.y + 1, swipeTime);
                        candies[c.y * tailleGrille + c.x] = c;
                    }
                }

                candies[cc[i].x] = null; //Remove reference in first row
            }

        }
        else
        {
            int tx = cc[0].x;
            int minY = cc[0].y;
            for (int i = 0; i < cc.Length; i++) minY = Mathf.Min(minY, cc[i].y);

            for (int i = minY - 1; i >= 0; i--)
            {
                CandyDechet c = getCandy(tx, i);
                c.moveToBlock(c.x, i + cc.Length, swipeTime);
                candies[c.y * tailleGrille + c.x] = c;
            }

            for (int i = 0; i < cc.Length; i++) candies[i * tailleGrille + cc[0].x] = null; //remove first candies in that column
        }

        for (int i = 0; i < cc.Length; i++) cc[i].remove();

        Invoke("fillGrid", swipeTime/2f);
    }

    public void incrementAssociation(string tid)
    {
        float targetVal = Mathf.Min(ressourceCurrentValue + pourcentGagneParAssociation / 100, 1);
        DOTween.To(() => ressourceCurrentValue, x => ressourceCurrentValue = x, targetVal, .3f);

        associationsScore++;
        associationsText.text = associationsScore.ToString();

        if (associations[tid]+1 == maxAssociationParDechet)
        {
            showMessage("recyclage/" + tid);
            score++;
            scoreText.text = score.ToString();
            setAssociation(tid, 0);
            AudioPlayer.instance.play("yes");

        }
        else
        {
            //If we want random congrats messages
            //showMessage("association/" + associationMessageImages[UnityEngine.Random.Range(0, associationMessageImages.Length)]);
            showMessage("association/" + tid);

            setAssociation(tid, associations[tid] + 1);

            AudioPlayer.instance.play("bip");

        }


    }

    public void setAssociation(string id, int value)
    {
        associations[id] = value;
        //animate


        float p = value * 1f / (maxAssociationParDechet-1); //-1 pour aller jusqu'en haut à l'avant derniere association

        RectTransform parentRect = associationBars[id].transform.parent.GetComponent<RectTransform>();
        RectTransform barRect = associationBars[id].GetComponent<RectTransform>();

        DOTween.To(()=> barRect.rect.height,x=> barRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, x), minAssociationBarHeight + (parentRect.rect.height - minAssociationBarHeight) * p,.5f);

        Transform dechetImg = barRect.Find("DechetImage").GetComponent<Transform>();
        dechetImg.localScale = Vector3.one;
        dechetImg.DOScale(2, 1).SetEase(Ease.OutElastic);
        dechetImg.DOScale(1, .5f).SetDelay(1);
    }
    




    public void candyTouchDown(CandyDechet c, Vector2 pos)
    {
        setCurrentCandy(c);
    }

    public void setCurrentCandy(CandyDechet c)
    {
        if (currentCandy != null)
        {
            currentCandy.setSelected(false);
        }
        currentCandy = c;

        if (currentCandy != null)
        {
            currentCandy.setSelected(true);
        }

    }

    public void textureReady(string textureID, Texture2D tex)
    {
        if (textureID.StartsWith("tex"))
        {
            string[] texSplit = textureID.Split(new char[] { '_' });
            int tid = -1;
            int.TryParse(texSplit[1], out tid);
            if (tid >= 0) dechetTextures[tid] = tex;

            associationBars[dechets[tid].id].GetComponentInChildren<RawImage>().texture = tex;

            //fill already assign candies
            for (int i = 0; i < numItems; i++)
            {
                if (candies[i] != null)
                {
                    if (candies[i].id == dechets[tid].id)
                    {
                        candies[i].setTexture(tex);
                    }
                }
            }
        }else if(textureID == "message")
        {
            messageImage.texture = tex;
            messageImage.transform.DOKill();
            messageImage.transform.localScale = Vector3.zero;
            messageImage.transform.DOScale(1, .6f).SetEase(Ease.OutElastic);
            messageImage.transform.DOScale(0, .3f).SetDelay(2f);
        }
              

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        setCurrentCandy(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (animating) return;

        if (eventData.pointerEnter == null) return;
        CandyDechet c = eventData.pointerEnter.GetComponent<CandyDechet>();
        if (c == null) return;
        setCurrentCandy(c);
        screenPosAtDown = eventData.position;

    }

    public void showMessage(string path, float lockTime = 0)
    {
        if (messageIsLocked) return;

        StartCoroutine(AssetManager.loadGameTexture(id, "messages/"+path + ".png", "message", this));
        if(lockTime > 0)
        {
            messageIsLocked = true;
            Invoke("unlockMessage", lockTime);
        }

    }

    public void unlockMessage()
    {
        messageIsLocked = false;
    }
   
}