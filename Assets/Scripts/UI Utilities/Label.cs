using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Label : MonoBehaviour
{
    private RectTransform rectTransform;

    Transform target;

    float initialScale;

    public bool isAlwaysShow = false;

    //private LineRenderer lineRenderer;
    bool isBlocked = false;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform =GetComponent<RectTransform>();
        initialScale = transform.localScale.x;

        //lineRenderer = gameObject.AddComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //rectTransform.anchoredPosition = Camera.main.WorldToScreenPoint(target.position) / canvasRectTransform.localScale.x;
        if (target)
        {
            rectTransform.position = Camera.main.WorldToScreenPoint(target.position);

            CheckVisibility();
        }

        if (isBlocked)
        {
            if (isAlwaysShow)
            {
                if (rectTransform.localPosition.z >= 0)
                {
                    transform.DOScale(new Vector3(initialScale, initialScale, initialScale), 0.2f);
                }
                else
                {
                    transform.DOScale(new Vector3(0, 0, 0), 0.2f);
                }
            }
            else
            {
                transform.DOScale(new Vector3(0, 0, 0), 0.2f);
            }
        }
        else
        {
            if (rectTransform.localPosition.z >= 0)
            {
                transform.DOScale(new Vector3(initialScale, initialScale, initialScale), 0.2f);
            }
            else
            {
                transform.DOScale(new Vector3(0, 0, 0), 0.2f);
            }
        }
        
    }

    public void SetTarget(Transform pos)
    {
        target = pos;
    }

    
    public void CheckVisibility()
    {
        RaycastHit hit;
        Vector3 direction = Camera.main.transform.position - target.position;
        float distance = Vector3.Distance(Camera.main.transform.position, target.position);

        if (Physics.Raycast(target.position, direction, out hit, distance, 1<<6))
        {
            // If the raycast hits something, set the positions of the LineRenderer
            //lineRenderer.positionCount = 2;
            //lineRenderer.SetPosition(0, target.position);
            //lineRenderer.SetPosition(1, hit.point);

            isBlocked = true;
        }
        else
        {
            // If the raycast doesn't hit anything, set the positions to a default end point
            //lineRenderer.positionCount = 2;
            //lineRenderer.SetPosition(0, target.position);
            //lineRenderer.SetPosition(1, target.position + direction * distance);

            isBlocked = false;
        }
    }

}
