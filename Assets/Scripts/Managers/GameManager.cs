using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject player;

    #region Singleton
    private static GameManager instance;
    public static GameManager Instance { get { return instance; } }
    private void InitialiseSingleton()
    {
        if (instance != null && instance != this)
        {
            Debug.Log("Duplicate GameManager, destroying new one!");
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    #endregion

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        
    }
}
