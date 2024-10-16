using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrackEffectController : MonoBehaviour
{
    // Start is called before the first frame update
    public void Init(bool isfacingRight, float originalScale = 1f, bool fadeBurn = false, bool isUndead = false)
    {
        transform.localScale = new Vector3(isfacingRight ? 1f : -1f, 1f, 1f) * originalScale;
        ParticleSystem particleSystem = GetComponent<ParticleSystem>();
        
        //Texture2D spriteSheet = Resources.Load<Texture2D>("Sprites/Effects/CrackSprite");
        var list = new List<Sprite>();
        list = Resources.LoadAll<Sprite>("Sprites/Effects/CrackSprite").ToList();
        var sprite = list[Random.Range(0, list.Count)];
        
        var textureSheetAnimation = particleSystem.textureSheetAnimation;
        textureSheetAnimation.SetSprite(0, sprite);
        
        if (fadeBurn)
        {
            var material = particleSystem.GetComponent<ParticleSystemRenderer>().material;
            if (material.HasProperty("_FadeBurnGlow"))
            {
                material.SetFloat("_FadeBurnGlow", 100f);
            }
            if (material.HasProperty("_FadeBurnColor"))
            {
                if (isUndead)
                {
                    material.SetColor("_FadeBurnColor", new Color(143f / 255f, 28f / 255f, 154f / 255f));
                }
                else
                {
                    material.SetColor("_FadeBurnColor", new Color(1f, 201f / 255f, 0f));
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
