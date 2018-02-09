using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;

public class AudioManager : MonoBehaviour {

    public AudioSource bgmMain;
    public AudioSource bgmBattle;
    public AudioClip lose;
    public AudioClip win;
    public AudioSource attack;
    public AudioClip click;
    public AudioClip add;
    public AudioClip sub;
    public AudioClip battleDemon;
    public AudioClip battleHuman;
    public AudioClip battleTogether;

    enum State
    {
        Main,
        Battle,
        Win,
        Lose,
        Attack
    }

    private State state;

    private AudioSource audioSource;

	// Use this for initialization
	void Start ()
	{
	    audioSource = GetComponent<AudioSource>();
	    state = State.Main;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayAudioMain()
    {
        if (state == State.Attack)
        {
            attack.Stop();
        }
        if (state != State.Main)
        {
            bgmBattle.Pause();
            bgmMain.Play();
            state = State.Main;
        }
    }

    public void PlayAudioBattle()
    {
        if (state == State.Attack)
        {
            attack.Stop();
        }
        if (state != State.Battle)
        {
            bgmMain.Pause();
            bgmBattle.Play();
            state = State.Battle;
        }
    }

    public void PlayAudioWin()
    {
        if (state == State.Attack)
        {
            attack.Stop();
        }
        state = State.Win;
        audioSource.clip = win;
        audioSource.Play();
        bgmBattle.Pause();
    }

    public void PlayAudioLose()
    {
        if (state == State.Attack)
        {
            attack.Stop();
        }
        state = State.Lose;
        audioSource.clip = lose;
        audioSource.Play();
        bgmBattle.Pause();
    }

    public void StopAudioAttack()
    {
        if (state == State.Attack)
        {
            attack.Stop();
        }
    }

    public void PlayAudioAttack()
    {
        if (state == State.Attack) return;
        if (BattleManager.PlayerTroops.race == BattleManager.EnemyTroops.race)
        {
            attack.clip = BattleManager.PlayerTroops.race == BattleManager.Race.Demon ? battleDemon : battleHuman;
        }
        else
        {
            attack.clip = battleTogether;
        }
        attack.Play();
        state = State.Attack;
    }

    public void PlayAudioClick()
    {
        audioSource.clip = click;
        audioSource.Play();
    }

    public void PlayAudioAdd()
    {
        audioSource.clip = add;
        audioSource.Play();
    }

    public void PlayAudioSub()
    {
        audioSource.clip = sub;
        audioSource.Play();
    }
}
