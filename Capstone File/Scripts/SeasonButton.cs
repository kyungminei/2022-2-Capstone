using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonButton : MonoBehaviour
{
    public GameObject seasonsPanel;

    public GameObject springPanel;
    public GameObject summerPanel;
    public GameObject fallPanel;
    public GameObject winterPanel;

    public void Spring_Button()
    {
        springPanel.SetActive(true);
        seasonsPanel.SetActive(false);
    }

    public void Summer_Button()
    {
        seasonsPanel.SetActive(false);
        summerPanel.SetActive(true);
    }

    public void fall_Button()
    {
        seasonsPanel.SetActive(false);
        fallPanel.SetActive(true);
    }

    public void winter_Button()
    {
        seasonsPanel.SetActive(false);
        winterPanel.SetActive(true);
    }
}
