using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-2)]
public class GlobalVar : MonoBehaviour
{
	public static GlobalVar Instance;
	
	private void Awake() { 
		Instance = this;
	}
	
	public int Currency = 0;
	public int Gems = 0;

	private bool NeedsUpdate = false;
	private int nextCurrency = 0;
	private int nextGems = 0;
	private float lastTime;
	
	public float currentXP, nextXP;
	
	private void FixedUpdate() 
	{
		if(NeedsUpdate && Time.time >= lastTime)
		{
			//currency
			bool up = Currency < nextCurrency;
			if(Currency != nextCurrency) //auto move up or down but with 'rolling' numbers!
			{
				if(up)
				{
					Currency += 1;
					if(Currency > nextCurrency) Currency = nextCurrency;
				}
				else
				{
					Currency -= 1;
					if(Currency < nextCurrency) Currency = nextCurrency;
				}

				UIManager.Instance.UpdateUI();

				if (Currency == nextCurrency)
				{
					nextCurrency = 0;
					NeedsUpdate = false;
				}
			}

			//gems
            bool upG = Gems < nextGems;
            if (Gems != nextGems) //auto move up or down but with 'rolling' numbers!
            {
                if (upG)
                {
                    Gems += 1;
                    if (Gems > nextGems) Gems = nextGems;
                }
                else
                {
                    Gems -= 1;
                    if (Gems < nextGems) Gems = nextGems;
                }

                UIManager.Instance.UpdateUI();

                if (Gems == nextGems)
                {
                    nextGems = 0;
                    NeedsUpdate = false;
                }
            }
            lastTime = Time.time + 0.01f;
		}
	}
	
	public void AddCurrency(int val)
	{
		if (nextCurrency != 0) { Currency = nextCurrency; nextCurrency = 0; } //skip animation if not already complete!

		nextCurrency += Currency + val;
		NeedsUpdate = true;

		//achievements?
		UIManager.Instance.achievementManager.ProgressAchievement("Collect 50000 Money");
		UIManager.Instance.achievementManager.ProgressAchievement("Collect 10000 Money");
	}
	
	public bool RemoveCurrency(int val)
	{
		if(Currency - val < 0) 
			return false;

		if (nextCurrency != 0) //skip animations.
			Currency = nextCurrency;
		
		nextCurrency = Currency - val;
		NeedsUpdate = true;
		return true;
	}

	public void RemoveCurrencyOnZero(int val) 
	{
        if (nextCurrency != 0)
            Currency = nextCurrency;

        nextCurrency = Currency - val;
		if (nextCurrency < 0) nextCurrency = 0;
		NeedsUpdate = true;
	}

    public void AddGems(int val)
    {
        if (nextGems != 0) { Gems = nextGems; nextGems = 0; } //skip animation if not already complete!

        nextGems += Gems + val;
        NeedsUpdate = true;
    }

    public bool RemoveGems(int val)
    {
        if (Gems - val < 0)
            return false;

        if (nextGems != 0)
            Gems = nextGems;

        nextGems = Gems - val;
        NeedsUpdate = true;
        return true;
    }

    public void RemoveGemsOnZero(int val)
    {
        if (nextGems != 0)
            Gems = nextGems;

        nextGems = Gems - val;
        if (nextGems < 0) nextGems = 0;
        NeedsUpdate = true;
    }

    private void OnApplicationQuit()
	{
		//save the currency:
		PlayerPrefs.SetInt("currency", Currency);
	}
}
