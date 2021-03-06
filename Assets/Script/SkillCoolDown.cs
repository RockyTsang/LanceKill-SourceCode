﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCoolDown : MonoBehaviour
{
    public GameObject Me;
    private CharacterPreset PresetScript;
    private Image CDFrame;
    public enum SkillSet
    {
        LongAttack,
        Crush,
        Heal
    }
    public SkillSet SkillHandler;
    public Sprite[] IconSprites;

    // Start is called before the first frame update
    void OnEnable()
    {
        CDFrame = transform.Find("CDFrame").GetComponent<Image>();
        PresetScript = Me.GetComponent<CharacterPreset>();
        switch (gameObject.name)
        {
            case "LongAttackIcon":
                SkillHandler = SkillSet.LongAttack;
                break;
            case "CrushIcon":
                SkillHandler = SkillSet.Crush;
                break;
            case "HealIcon":
                SkillHandler = SkillSet.Heal;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Me)
        {
            switch (SkillHandler)
            {
                case SkillSet.LongAttack:
                    CDFrame.fillAmount = PresetScript.longAttackTimer / PresetScript.longAttackCD;
                    break;
                case SkillSet.Crush:
                    CDFrame.fillAmount = PresetScript.crushTimer / 5f;
                    break;
                case SkillSet.Heal:
                    CDFrame.fillAmount = PresetScript.healTimer / 20f;
                    break;
            }
        }
        else
        {
            CDFrame.fillAmount = 0;
        }
    }
}
