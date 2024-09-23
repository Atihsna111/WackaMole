using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mole : MonoBehaviour
{
     [Header ("Graphics")]
    [SerializeField] private Sprite mole;
    [SerializeField] private Sprite moleHardHat;
    [SerializeField] private Sprite moleHatBroken;
    [SerializeField] private Sprite moleHit;
    [SerializeField] private Sprite moleHatHit;

    [Header ("GameManager")]
    [SerializeField] private GameManager gameManager;

    //The offset of the sprite to hide it.
    private Vector2 _startPosition = new Vector2(0f, -2.56f);
    private Vector2 _endPosition = Vector2.zero;
    //time taken to show a mole
    private float _showDuration = 0.25f;
    private float _duration = 1f;
    private SpriteRenderer spriteRenderer;
    private Animator _animator;
    private BoxCollider2D _boxCollider2D;
    private Vector2 _boxOffset;
    private Vector2 _boxSize;
    private Vector2 _boxOffsetHidden;
    private Vector2 _boxSizeHidden;
    //Mole Parameters
    private bool hittable = true;
    public enum MoleType{Standard, HardHit, Bomb};
    private MoleType moleType;
    private float _hardRate = 0.25f;
    private float _bombRate = 0f;
    private int _lives;
    private int _moleIndex = 0;

    private IEnumerator ShowHide(Vector2 start, Vector2 end)
    {
        //make sure we start at the start
        transform.localPosition = start;

        //Show the mole.
        float elapsed = 0f;
        while (elapsed < _showDuration)
        {
            transform.localPosition = Vector2.Lerp(start, end,elapsed/_showDuration);
            _boxCollider2D.offset = Vector2.Lerp(_boxOffset,_boxOffsetHidden, elapsed/_showDuration);
            _boxCollider2D.size = Vector2.Lerp(_boxSize, _boxSizeHidden, elapsed /_showDuration);

            //update at max framrate.
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Make sure to be exactly at the end
        transform.localPosition = end;
        _boxCollider2D.offset = _boxOffset;
        _boxCollider2D.size = _boxSize;

        //wait for duration to pass.
        yield return new WaitForSeconds(_duration);

        //Hide the mole.
        elapsed = 0f;
        while(elapsed < _showDuration)
        {
            transform.localPosition = Vector2.Lerp(end, start, elapsed / _showDuration);
            _boxCollider2D.offset = Vector2.Lerp(_boxOffset,_boxOffsetHidden, elapsed/_showDuration);
            _boxCollider2D.size = Vector2.Lerp(_boxSize, _boxSizeHidden, elapsed /_showDuration);

            //Update at max framerate
            elapsed +=Time.deltaTime;
            yield return null;
        }
        //Make sure we're back at the start position.
        transform.localPosition = start;
        _boxCollider2D.offset = _boxOffsetHidden;
        _boxCollider2D.size = _boxSizeHidden;

        //If we got to the end and it's still hittable then we missed it.
        if(hittable)
        {
            hittable = false;
            //we only give time penalty if it isn't a bomb.
           gameManager.Missed(_moleIndex, moleType != MoleType.Bomb);
        }
    }

    public void Hide()
    {
        //Set the appropriate mole parameters to hide it.
        transform.localPosition = _startPosition;
        _boxCollider2D.offset = _boxOffsetHidden;
        _boxCollider2D.size = _boxSizeHidden;
    }

    private IEnumerator QuickHide()
    {
        yield return new WaitForSeconds(0.2f);
        //Whilst we were waiting we may have spawned again here, so just 
        //check that hasn't happened before hiding it. this will stop it
        //flickering in that case.
        if(!hittable)
        {
            Hide();
        }
    }

    private void OnMouseDown()
    {
        if(hittable)
        {
            switch(moleType)
            {
                case MoleType.Standard:
                    spriteRenderer.sprite = moleHit;
                    gameManager. SoundFX(gameManager.moleClick);
                    gameManager.AddScore(_moleIndex);
                    //Stop the animation
                    StopAllCoroutines();
                    StartCoroutine(QuickHide());
                    //Turn off hittable so that we can't keep tapping for score.
                    hittable = false;
                    break;
                case MoleType.HardHit:
                    //If lives ==2 reduce, and change sprite.
                    if(_lives ==2)
                    {
                        spriteRenderer.sprite = moleHatBroken;
                        _lives--;
                    }
                    else{
                        spriteRenderer.sprite = moleHatHit;
                        gameManager. SoundFX(gameManager.moleClick);
                        gameManager.AddScore(_moleIndex);
                        //Stop the animation
                        StopAllCoroutines();
                        StartCoroutine(QuickHide());
                        //turn off hittable so the we can't keeo tapping for score.
                        hittable = false;
                    }
                    break;
                    case MoleType.Bomb:
                    //Game over, 1 for bomb.
                    gameManager.GameOver(1);
                    break;
                    default:
                    break;
            }

        }
    }
    private void CreateNext()
    {
        float random = Random.Range(0f,1f);
        if(random < _bombRate)
        {
            //Make a bomb.
            moleType = MoleType.Bomb;
            //The animator handles setting the sprite.
            _animator.enabled = true;
        }
        else
        {
            _animator.enabled = false;
            random = Random.Range(0f, 1f);
            if(random < _hardRate)
            {
                //Create a hard one.
                moleType = MoleType.HardHit;
                _lives = 2;
                spriteRenderer.sprite = moleHardHat;
                
            } else{
                //Create a standard one.
                moleType = MoleType.Standard;
                _lives = 1;
                spriteRenderer.sprite = mole;
                
            }
        }
        //Mark as hittable so we can register an onclick event.
        hittable = true;
    }
    private void SetLevel (int level)
    {
        //As level increases increase the bomb rate to 0.25 at level 10.
        _bombRate = Mathf.Min(level* 0.025f, 0.25f);
        //Increase the amounts of HardHits until 100% at level 40.
        _hardRate = Mathf.Min(level*0.025f, 1f);

        //Duration bounds get quikcer as we progress. No cap on insanity.
        float durationMin = Mathf.Clamp(1 - level * 0.1f, 0.01f, 1f);
        float durationMax = Mathf.Clamp(2 - level * 0.1f, 0.01f, 2f);
        _duration = Random.Range(durationMin, durationMax);
    }
    private void Awake()
    {
        //Get references to the componnets we'll need.
        spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _boxCollider2D = GetComponent<BoxCollider2D>();
        //Work out collider values.
        _boxOffset = _boxCollider2D.offset;
        _boxSize = _boxCollider2D.size;
        _boxOffsetHidden = new Vector2(_boxOffset.x, -_startPosition.y/ 2f);
        _boxSizeHidden = new Vector2 (_boxSize.x, 0f); 
    }
    public void Activate (int level)
    {
        SetLevel(level);
        CreateNext();
        StartCoroutine(ShowHide(_startPosition,_endPosition));
    }
    //used by the game Manager to uniquely identify moles.
    public void SetIndex(int index)
    {
        _moleIndex = index;
    }
    //used to freeze the game on finish.
    public void StopGame()
    {
        hittable = false;
        StopAllCoroutines();
    }
    
}
