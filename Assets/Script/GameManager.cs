using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<Mole> _moles;
    [Header ("UI Objects")]
    [SerializeField] private GameObject _Button;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private GameObject _OutOfTimeText;
    [SerializeField] private GameObject _bombText;
    [SerializeField] private TMPro.TextMeshProUGUI _timeText;
    [SerializeField] private TMPro.TextMeshProUGUI _scoreText;

    [Header("Audio")]
     [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource sFXSource;
    [SerializeField] AudioClip gameOver;
    [SerializeField]  AudioClip BGM;
    [SerializeField]  AudioClip Bomb;
    [SerializeField]  public AudioClip moleClick;

    private float _startingTime = 30f;
    private float _remainingTime;
    private HashSet<Mole> currentMoles = new HashSet<Mole>();
    private int _score;
    private bool _playing = false;

    public void Start()
    {
        _OutOfTimeText.SetActive (false);
        _bombText.SetActive (false);
        _Button.SetActive(true);
    }

    public void StartGame()
    {
        //Hide/show the UI elements we don't / do want to see.
        audioSource.clip = BGM;
        audioSource.Play();
        _Button.SetActive(false);
        _OutOfTimeText.SetActive(false);
        _bombText.SetActive(false);
        _gameUI.SetActive(true);
        //Hide all the visible moles.
        for(int i = 0; i <_moles.Count; i++)
        {
            _moles[i].Hide();
            _moles[i].SetIndex(i);
        }
        //Remove any old game state.
        currentMoles.Clear();
        //Start with 30 seconds.
        _remainingTime = _startingTime;
        _score= 0;
        _scoreText.text = "0";
        _playing = true;
    }
     public void SoundFX(AudioClip clip)
    {
        sFXSource.PlayOneShot(clip);

    }

    public void GameOver(int type)
    { //show the message
        if(type == 0)
        {
            _OutOfTimeText.SetActive(true);
            SoundFX(gameOver);
            audioSource.Pause();

        }
        else{
            _bombText.SetActive(true);
            SoundFX(Bomb);
            audioSource.Pause();
        }
        //hide all moles.
        foreach (Mole mole in _moles)
        {
            mole.StopGame();
        }
        //stop the game and show the satrt UI.
        _playing = false;
        _Button.SetActive(true);

    }
   // Update is called once per frame
    void Update()
    {
        if (_playing)
        {
            //update time
            _remainingTime -= Time.deltaTime;
            if(_remainingTime <= 0)
            {
                _remainingTime = 0;
                GameOver(0);
            }
            int _minutes = (int)(_remainingTime / 60);
            int _seconds = (int)(_remainingTime % 60);
            _timeText.text = $"{_minutes}:{_seconds:D2}";
            Debug.Log($"Remaining time: {_remainingTime}, Minutes: {_minutes}, Seconds: {_seconds}");
            //check if we need to start anymore moles.
            if(currentMoles.Count <= (_score/10))
            {
                //Choose a random mole
                int index = Random.Range(0,_moles.Count);
                //check for next frame
                if(!currentMoles.Contains(_moles[index]))
                {
                    currentMoles.Add(_moles[index]);
                    _moles[index].Activate(_score/10);
                }
            }
        }
      
    }
    public void AddScore(int _moleIndex)
    {
        //Add and  update score.
        _score += 1;
        _scoreText.text = $"{_score}";
        //increase time by a 1.
        //_remainingTime += 1;
        //Remove from active moles.
        currentMoles.Remove(_moles[_moleIndex]);
    }
    public void Missed(int _moleIndex, bool isMole)
    {
        if(isMole)
        {
            //Decrease time by 2.
            _remainingTime -= 2;
        }
        //remove from active moles.
        currentMoles.Remove(_moles[_moleIndex]);
    }
}
