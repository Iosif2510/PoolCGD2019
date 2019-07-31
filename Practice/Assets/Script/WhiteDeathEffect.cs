using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteDeathEffect : MonoBehaviour
{

    public float fadeOutTime = 1.0f;
    Material material;
    bool isFadingOut = false;
    float time = 0;

    void Awake()
    {
        material = transform.GetComponent<Renderer>().material;
        FadeOutPlay();
    }

   
    void Update()
    {
        
    }

    void FadeOutPlay()
    {
        if (isFadingOut == true) return;
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeOut()
    {
        isFadingOut = true;

        Color fadeColor = material.color;
  
        while(fadeColor.a > 0)
        {
            time += Time.deltaTime / fadeOutTime;
            fadeColor.a = Mathf.Lerp(1, 0, time);
            material.color = fadeColor;
            yield return null;
        }

        isFadingOut = false;

        Destroy(gameObject);
    }
}
