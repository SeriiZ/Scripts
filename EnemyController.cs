using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour, IHealth, IMana {

	Transform _transform;

	int _health = 100;
	int _maxHealth = 100;
	int _recoveryHealth = 2;
	bool _isRecoveryHealth = false;
	int _mana = 100;
	int _maxMana = 100;
	int _recoveryMana = 5;
	bool _isRecoveryMana = false;
	
	EnemyType _enemyType;
	float _speed;
	float _strength;
	float _defense;
	float _hitDistance;
	float _findDistance;
	int _damage;
	float _freezeResist;
	float _fireResist; 
	float _waterResist;
	float _blizardResist;
	float _windResist;
	string _name;
    NavMeshAgent _agent;
    public UnitAnimController _anim;
	
	[SerializeField]
	private GameController _gameController;	
	DamageType _damageType = DamageType.None;
	
	Vector3[] path;
    int targetIndex;
    bool isMove;
    bool toNextPos; 
	Vector3 velosity;
	Vector3 position;
    Vector3 _lastPosition;
    float gravity = 9.8f;
	float speed = 5.5f;
	public EnemyType _characterType;
	
	public CapsuleCollider _characterCollider;
    public CharacterController _characterController;
    public bool _attackMode;
	bool _enableHit = true;
	float _hitDelay = 0.5f;
	public GameObject _enemy;
    int _check;

    [SerializeField]
	GameObject _atackCollider;

	public int Health {
		get{
			return _health;
		}
		set{
			_health = value;
		}
	}

	public int Mana {
		get{
			return _mana;
		}
		set{
			_mana = value;
		}
	}

	public int Damage {
		get{
			return _damage;
		}
		set{
			_damage = value;
		}
	}

	public float Speed {
		get{
			return _speed;
		}
		set{
			_speed = value;
		}
	}

	public float Strength {
		get{
			return _strength;
		}
		set{
			_strength = value;
		}
	}

	public float Defense {
		get{
			return _defense;
		}
		set{
			_defense = value;
		}
	}

	public float HitDistance {
		get{
			return _hitDistance;
		}
		set{
			_hitDistance = value;
		}
	}

	public float FindDistance {
		get{
			return _findDistance;
		}
		set{
			_findDistance = value;
		}
	}

	public float FreezeResist {
		get{
			return _freezeResist;
		}
		set{
			_freezeResist = value;
		}
	}

	public float FireResist {
		get{
			return _fireResist;
		}
		set{
			_fireResist = value;
		}
	}

	public float WaterResist {
		get{
			return _waterResist;
		}
		set{
			_waterResist = value;
		}
	}

	public float BlizardResist {
		get{
			return _blizardResist;
		}
		set{
			_blizardResist = value;
		}
	}

	public float WindResist {
		get{
			return _windResist;
		}
		set{
			_windResist = value;
		}
	}

	void Start () {		
		_transform = transform;
		_attackMode = _atackCollider.GetComponent<AttackColliction>()._attackMode;
        _agent = GetComponent<NavMeshAgent>();

        int _index = _gameController._playerParams.Length;
		switch (_characterType)
		{
			case EnemyType.Warrior:
			{
				_index = 4;
			}
			break;
			case EnemyType.Wither:
			{
				_index = 5;
			}
			break;
			case EnemyType.Girl:
			{
				_index = 6;
			}
			break;
			case EnemyType.Enemy1:
			{
				_index = 7;
			}
			break;
			case EnemyType.Enemy2:
			{
				_index = 8;
			}
			break;
			case EnemyType.Enemy3:
			{
				_index = 9;
			}
			break;
			default:
			_index = _gameController._playerParams.Length;
			break;
		}
		
		if(_index < _gameController._playerParams.Length)
		{
			SetPlayerParams(_index);
		}
	}
	
	void SetPlayerParams(int index) {
		_health 	   	= _gameController._playerParams[index].health;
	  	_mana 		   	= _gameController._playerParams[index].mana;
		_name 			= _gameController._playerParams[index].characterName;
		_speed 		  	= _gameController._playerParams[index].speed;
		_strength 	  	= _gameController._playerParams[index].strength;
		_defense 	  	= _gameController._playerParams[index].defense;
		_damage		  	= _gameController._playerParams[index].damage;
		_hitDistance  	= _gameController._playerParams[index].hitDistance;
		_findDistance 	= _gameController._playerParams[index].findDistance;
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			Spells(15);
		}
		
		if (_attackMode) {
			RaycastHit hit;
			
			Vector3 forward = _transform.TransformDirection(Vector3.forward) * _findDistance;
			Debug.DrawRay(_transform.position, forward, Color.red);
			
			transform.LookAt(_enemy.transform);
			
			if(Physics.Raycast(_transform.position, forward, out hit, _findDistance)){	
				if(hit.distance >= (_findDistance - 1)) {
                     DisableAttackMode();

                     return;
				}
				
				if(hit.transform.tag == "Player" && (_hitDistance >= hit.distance)){
					if(hit.transform.gameObject != null) {
						if(_enableHit){
							_enableHit = false;
                            //_agent.Stop();
							hit.transform.gameObject.GetComponent<PlayerController>().DamageToHealth(_damage, gameObject, _damageType);
							_anim.Hit();
							StartCoroutine(WaitToNextHit(2f));
						}
					}
				} 

				if(hit.transform.tag == "Player" && (_hitDistance < hit.distance)){
					position = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					if (!isMove) {
                        _check = 0;
                        _anim.Run();
                        _agent.SetDestination(hit.point);
                        isMove = true;
                        StartCoroutine(MoveToPosition(hit.point));

                        //PathRequestManager.RequestPath(_transform.position, position, OnPathFound);
                    }
				}
				
				if(hit.transform.gameObject.GetComponent<PlayerController>().Health <= 0) {
					_anim.Idle();
					hit.transform.gameObject.GetComponent<PlayerController>().enabled = false;
				}
			}
		}
	}

	#region IHealth implementation

	public void DamageToHealth (int health, GameObject enemy, DamageType _damagedType){
		if(_enemy == null && _attackMode == false){
			_enemy = enemy;
			StartCoroutine(RotateToAngle(_enemy.transform.position));
			_attackMode = true;
		}
		float healthF = 0.0f;
		switch (_damagedType)
		{
			case DamageType.Freeze:
				healthF = (health * _freezeResist)/ 100;
				break;
			case DamageType.Fire:
				healthF = (health * _fireResist)/ 100;
				break;
			case DamageType.Water:
				healthF = (health * _waterResist)/ 100;
				break;
			case DamageType.Blizard:
				healthF = (health * _blizardResist)/ 100;
				break;
			case DamageType.Wind:
				healthF = (health * _windResist)/ 100;
				break;
			default:
				healthF = 0.0f;
				break;
		}

		health = health - Convert.ToInt32(healthF);

		if((_health -= health) > 0)
		{
			_anim.TakeDamage();
			_health -= health;
			Debug.Log("Enemy Health = " + _health);
		} else {
            _enemy.GetComponent<PlayerController>().DisableAttackMode();
			_anim.Death();
			CharacterDie();
		}
		
		if(_health < 0)
		{
			_health = 0;
		}
		
		if(!_isRecoveryHealth)
		{
			if (_health < _maxHealth)
			{
				_isRecoveryHealth = true;				
				InvokeRepeating("RecoveryHealth", 8f, 2f);
			}
		}
	}

	public void RecoveryHealth ()
	{
		if(_health < _maxHealth)
		{
			_health += _recoveryHealth;
		} 

		if(_health >= _maxHealth)
		{
			_health = _maxHealth;
			_isRecoveryHealth = false;
			CancelInvoke("RecoveryHealth");
		}
	}

    public void DisableAttackMode()
    {
        _transform.GetChild(0).gameObject.SetActive(true);
        _enemy.transform.GetChild(0).gameObject.SetActive(true);
        _enemy = null;
        _attackMode = false;
        _anim.Idle();
    }

    public void CharacterDie()
	{
   		_characterCollider.enabled = false;
        _characterController.enabled = false;
        this.enabled = false;
    }

	#endregion

	#region IMana implementation

	public void Spells (int mana)
	{
		if (_mana > 0)
		{
			_mana -= mana;
		}
		
		if(_mana < 0)
		{
			_mana = 0;
		}
		
		if (!_isRecoveryMana)
		{
			if(_mana < _maxMana)
			{
				_isRecoveryMana = true;
				InvokeRepeating("RecoveryMana", 5f, 2f);
			}
		}
	}

	public void RecoveryMana ()
	{
		if(_mana < _maxMana)
		{
			_mana += _recoveryMana;
		}
		
		if(_mana >= _maxMana)
		{
			_mana = _maxMana;
			_isRecoveryMana = false;
			CancelInvoke("RecoveryMana");
		}
	}
	
    #endregion
	
	
	#region Pathing movement
	/*public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
		Debug.Log("OnPathFound: " + pathSuccessful);
        if (pathSuccessful)
        {
            path = null;
            path = newPath;
            // StopCoroutine("MoveToPosition");
            // StopCoroutine("RotateToAngle");
			
			if(path.Length != 0){
				targetIndex = 0;
                _anim.Run();
            	StartCoroutine(MoveToPosition(path[targetIndex]));
            	StartCoroutine(RotateToAngle(path[targetIndex]));
			} else {
				Debug.Log("path.Length == 0");
				if (!isMove) {
                    isMove = true;
                    PathRequestManager.RequestPath(_transform.position, position, OnPathFound);
                } else {
                    isMove = false;
                    toNextPos = true;
                }
			}
        }
    }*/

    void ToPosition(Vector3 touchPos)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touchPos);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (hit.transform.tag == "NonTouchable")
            {
                if (!isMove)
                    _anim.Idle();
                return;
            }


            if (hit.transform.tag == "Surface" || hit.transform.tag == "Enemy")
            {
                _anim.Run();
                _agent.SetDestination(hit.point);
                isMove = true;
                StartCoroutine(MoveToPosition(hit.point));
            }
        }
    }

    IEnumerator MoveToPosition(Vector3 pos)
    {
        Vector3 currentWaypoint = new Vector3(pos.x, _transform.position.y, pos.z);
        while (isMove)
        {
            _anim.Run();

            

            if (Vector3.Distance(currentWaypoint, _transform.position) < 0.1f)
            {
                _check = 0;
                isMove = false;
                _anim.Idle();
            }

            if (Vector3.Distance(_lastPosition, _transform.position) == 0f)
            {
                _check++;

                if (_check > 20 && _agent.velocity.sqrMagnitude == 0)
                {
                    _check = 0;
                    isMove = false;
                    _anim.Idle();
                }
            }
            else
            {
                _check = 0;
            }

            Vector3 currentPos = new Vector3(_transform.position.x, _transform.position.y, _transform.position.z);
            _lastPosition = currentPos;
            yield return null;
        }
    }

    /*IEnumerator MoveToPosition(Vector3 pos) {
        Vector3 currentWaypoint = new Vector3(pos.x, _transform.position.y, pos.z);
		
        bool isLoop = true;
        isMove = true;
		
        while (isLoop)
        {
            if (!isMove)
            {
                targetIndex = 0;
                path = null;
                isLoop = false;
            }

            if (isLoop)
            {
                _transform.position = Vector3.MoveTowards(_transform.position, currentWaypoint, Time.deltaTime * speed);
                velosity.y -= gravity * Time.deltaTime;
                yield return null;

                if (Vector3.Distance(currentWaypoint, _transform.position) < 1f)
                {
                    targetIndex++;
                    if (targetIndex >= path.Length)
                    {
                        targetIndex = 0;
                        path = null;
                        isLoop = false;
                    }

                    if (isLoop)
                    {
                        currentWaypoint = new Vector3(path[targetIndex].x, _transform.position.y, path[targetIndex].z);
                        StopCoroutine("RotateToAngle");
                        StartCoroutine(RotateToAngle(currentWaypoint));
                    }
                }
            }
        }

        if (toNextPos)
        {
            toNextPos = false;
            PathRequestManager.RequestPath(_transform.position, position, OnPathFound);
        }
        else
        {
            isMove = false;
        }
        
        _anim.Idle();
    }*/

    public IEnumerator RotateToAngle(Vector3 pos)
    {
        Vector3 currentWaypoint = pos;
        Quaternion targetRot = Quaternion.LookRotation(currentWaypoint - transform.position);
        targetRot.x = 0f;
        targetRot.z = 0f;

        float elapsedTime = 0.0f;
        float time = 0.4f;
        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, (elapsedTime / time));
            
            yield return null;
        }
    }
	#endregion
	
	IEnumerator WaitToNextHit(float waitTime){
		yield return new WaitForSeconds(waitTime);
		_enableHit = true;
	}
}
