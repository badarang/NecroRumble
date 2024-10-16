using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class All1ShaderManager
{
    public void SetCorpseSelectionEffect(Material material_, bool isSelected_)
    {
        if (material_ == null)
        {
            Debug.LogWarning("Given material is null");
            return;
        }

        //TODO: boolean 값도 바꿀 수 있는지 테스트 (마우스 갖다 대면 변수명 나옴)
        if (isSelected_)
        {
            material_.SetFloat("_OutlineAlpha", 1);
            material_.SetFloat("_ShakeUvSpeed", 10);
            material_.SetFloat("_ShakeUvX", 5);
            material_.SetFloat("_ShakeUvY", 5);
        }
        else
        {
            material_.SetFloat("_OutlineAlpha", 0);
            material_.SetFloat("_ShakeUvSpeed", 0);
        }
    }
    
}
