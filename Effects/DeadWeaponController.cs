using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using LOONACIA.Unity.Managers;
using UnityEngine;

public class DeadWeaponController : ParabolaEffectController
{
    // just to prevent warning log by cyrano
    // private SpriteRenderer _spriteRenderer;
    // private float originalRotationSpeed = 2f;
    private float rotationSpeed = 2700f;
    private float endRotation;
    public int parentUnitType = -1;

    // Start is called before the first frame update
    void Start()
    {
        _useLocal = true;
        endRotation = Random.Range(150f, 210f);
        FireProjectile(Vector3.zero, GetRandomOffset());
    }

    public void SetLocalScale(float scale)
    {
        float resScale = scale / 3;
        speed = resScale;
        projectileHeight = resScale / 1.5f;
        transform.localScale = new Vector3(resScale, resScale, resScale);
    }

    public void SetParentUnitType(int unitType)
    {
        parentUnitType = unitType;
        string soundName = "";

        switch (parentUnitType)
        {
            case (int)UnitType.ArcherMan:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/ArcherWeapon");
                soundName = "Heavy Sword Swing 10";
                break;
            case (int)UnitType.Assassin:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/AssassinWeapon");
                soundName = "Light Sword Swing 3";
                break;
            case (int)UnitType.SwordMan:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/SwordWeapon");
                soundName = "Heavy Sword Swing 3";
                break;
            case (int)UnitType.HorseMan:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/HorseWeapon");
                ManagerRoot.Sound.PlaySfx("Medium Monster Death 01", 1f);
                soundName = "Heavy Sword Swing 3";
                break;
            case (int)UnitType.FlameMagician:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/MagicWeapon");
                soundName = "Heavy Sword Swing 3";
                break;
            case (int)UnitType.FlightSword:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/AngelWeapon");
                ManagerRoot.Sound.PlaySfx("Small Monster Death 02 Larger", 1f);
                soundName = "Heavy Sword Swing 3";
                break;
            case (int)UnitType.PriestSuccubus:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/PriestWeapon");
                soundName = "Heavy Sword Swing 3";
                break;
            case (int)UnitType.TwoSwordAssassin:
                GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Units/DeadWeapon/TwoSwordWeapon");
                soundName = "Heavy Sword Swing 3";
                break;
            default:
                break;
        }
        if (soundName != "")
        {
            ManagerRoot.Sound.PlaySfx(soundName, 1f);
        }
    }

    private void SetRandomInit()
    {
        speed = Random.Range(.5f, 1f);
        projectileHeight = Random.Range(2f, 3f);
    }

    private Vector3 GetRandomOffset()
    {
        float xx = Random.Range(-.2f, .2f);
        float yy = 0.2f;
        return new Vector3(xx, yy, 0);
    }

    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }

    protected override void Update()
    {
        base.Update();

        if (fireLerp < 1)
        {
            //rotate
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, endRotation);
            if (isGrounded == false)
            {
                isGrounded = true;
                string soundName = "";

                switch (parentUnitType)
                {
                    case (int)UnitType.ArcherMan:
                        soundName = "Staff Hitting (Flesh) 1";
                        break;
                    case (int)UnitType.Assassin:
                        soundName = "Staff Hitting (Flesh) 4";
                        break;
                    case (int)UnitType.SwordMan:
                        soundName = "Staff Hitting (Flesh) 2";
                        break;
                    default:
                        break;
                }
                if (soundName != "")
                {
                    ManagerRoot.Sound.PlaySfx(soundName, 0.4f);
                }
            }
        }
        /*
        if (fireLerp > .7f && rotationSpeed == originalRotationSpeed)
        {
            rotationSpeed /= 2;
        }
        */
    }
}
