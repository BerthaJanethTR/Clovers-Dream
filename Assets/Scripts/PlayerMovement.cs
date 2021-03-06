using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Componentes de Clover
    [Header("Components")]
    private Rigidbody2D _rb;
    private Animator _anim;

    //Campo donde poner el sonido de salto
    [Header("SFX")]
    [SerializeField] AudioClip jumpSFX;

    //Capa que se usar? para reconocer el piso y pared
    [Header("Layer Masks")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _wallLayer;

    //Variables de movimiento de Clover
    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration = 70f;
    [SerializeField] private float _maxMoveSpeed = 12f;
    [SerializeField] private float _groundLinearDrag = 7f;
    private float _horizontalDirection;
    private float _verticalDirection;
    private bool _changingDirection => (_rb.velocity.x > 0f && _horizontalDirection < 0f) || (_rb.velocity.x < 0f && _horizontalDirection > 0f);
    private bool _facingRight = true;
    private bool _canMove => !_wallGrab;
    bool isAlive = true;

    //Variables de salto de Clover
    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 14f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private float _downMultiplier = 12f;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private float _hangTime = .1f;
    [SerializeField] private float _jumpBufferLength = .1f;
    private int _extraJumpsValue;
    private float _hangTimeCounter;
    private float _jumpBufferCounter;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpsValue > 0 || _onWall);
    private bool _isJumping = false;

    //Variables para el comportamiento de Clover en una pared
    [Header("Wall Movement Variables")]
    [SerializeField] private float _wallSlideModifier = 0.5f;
    [SerializeField] private float _wallJumpXVelocityHaltDelay = 0.2f;
    private bool _wallGrab => _onWall && !_onGround && Input.GetButton("WallGrab");
    private bool _wallSlide => _onWall && !_onGround && !Input.GetButton("WallGrab") && _rb.velocity.y < 0f;

    //Variables para detectar la colisi?n con el piso
    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;

    //Variables para detectar la colisi?n con las paredes
    [Header("Wall Collision Variables")]
    [SerializeField] private float _wallRaycastLength;
    private bool _onWall;
    private bool _onRightWall;

    //Inicializacion de los componentes de Clover
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    //Bucle que estar? detectando cuando Clover se mover?, saltar? y activar? animaciones y el estado de muerte
    private void Update()
    {
        if (!isAlive) { return; }

        _horizontalDirection = GetInput().x;
        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferLength;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
        Animation();
        Death();
    }

    //Bucle que har? los calculos para el correcto movimiento de Clover
    private void FixedUpdate()
    {
        CheckCollisions();
        if (_canMove) MoveCharacter();
        else _rb.velocity = Vector2.Lerp(_rb.velocity, (new Vector2(_horizontalDirection * _maxMoveSpeed, _rb.velocity.y)), .5f * Time.deltaTime);
        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _extraJumpsValue = _extraJumps;
            _hangTimeCounter = _hangTime;
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _hangTimeCounter -= Time.fixedDeltaTime;
            if (!_onWall || _rb.velocity.y < 0f) _isJumping = false;
        }
        if (_canJump)
        {
            if (_onWall && !_onGround)
            {
                if (_onRightWall && _horizontalDirection > 0f || !_onRightWall && _horizontalDirection < 0f)
                {
                    AudioSource.PlayClipAtPoint(jumpSFX, Camera.main.transform.position);
                    StartCoroutine(NeutralWallJump());
                }
                else
                {
                    AudioSource.PlayClipAtPoint(jumpSFX, Camera.main.transform.position);
                    WallJump();
                }
                Flip();
            }
            else
            {
                AudioSource.PlayClipAtPoint(jumpSFX, Camera.main.transform.position);
                Jump(Vector2.up);
            }
        }
        if (!_isJumping)
        {
            if (_wallSlide) WallSlide();
            if (_wallGrab) WallGrab();
            if (_onWall) StickToWall();
        }
    }

    //M?todo para control b?sico
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    //M?todo que nos impedir? movernos si morimos
    private void MoveCharacter()
    {
        if (!isAlive) { return; }
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
    }

    //M?todo que nos desacelerar? al dejar de movernos o cambiar de direcci?n
    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rb.drag = _groundLinearDrag;
        }
        else
        {
            _rb.drag = 0f;
        }
    }

    //M?todo para desacelerar al saltar
    private void ApplyAirLinearDrag()
    {
        _rb.drag = _airLinearDrag;
    }

    //M?todo para el salto
    private void Jump(Vector2 direction)
    {
        if (!isAlive) { return; }
        if (!_onGround)
            _extraJumpsValue--;

        ApplyAirLinearDrag();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(direction * _jumpForce, ForceMode2D.Impulse);
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;
        _isJumping = true;
    }

    //M?todo para saltar desde la pared
    private void WallJump()
    {
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
    }

    //M?todo para saltar desde una pared con menos fuerza
    IEnumerator NeutralWallJump()
    {
        Vector2 jumpDirection = _onRightWall ? Vector2.left : Vector2.right;
        Jump(Vector2.up + jumpDirection);
        yield return new WaitForSeconds(_wallJumpXVelocityHaltDelay);
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
    }

    //M?todo para calcular fuerza de gravedad entre saltos
    private void FallMultiplier()
    {
        if (_verticalDirection < 0f)
        {
            _rb.gravityScale = _downMultiplier;
        }
        else
        {
            if (_rb.velocity.y < 0)
            {
                _rb.gravityScale = _fallMultiplier;
            }
            else if (_rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                _rb.gravityScale = _lowJumpFallMultiplier;
            }
            else
            {
                _rb.gravityScale = 1f;
            }
        }
    }

    //M?todo para agarrarse de la pared
    void WallGrab()
    {
        _rb.gravityScale = 0f;
        _rb.velocity = Vector2.zero;
    }

    //M?todo para deslizarse por las paredes
    void WallSlide()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, -_maxMoveSpeed * _wallSlideModifier);
    }

    //M?todo para dejar de deslizarse por la pared
    void StickToWall()
    {
        //Empuja a Clover hacia la pared
        if (_onRightWall && _horizontalDirection >= 0f)
        {
            _rb.velocity = new Vector2(1f, _rb.velocity.y);
        }
        else if (!_onRightWall && _horizontalDirection <= 0f)
        {
            _rb.velocity = new Vector2(-1f, _rb.velocity.y);
        }

        if (_onRightWall && !_facingRight)
        {
            Flip();
        }
        else if (!_onRightWall && _facingRight)
        {
            Flip();
        }
    }

    //M?todo para voltear el sprite de Clover
    void Flip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    //M?todo que controla los estados de las animaciones
    void Animation()
    {
        if ((_horizontalDirection < 0f && _facingRight || _horizontalDirection > 0f && !_facingRight) && !_wallGrab && !_wallSlide)
        {
            Flip();
        }
        if (_onGround)
        {
            _anim.SetBool("isGrounded", true);
            _anim.SetBool("isFalling", false);
            _anim.SetBool("WallGrab", false);
            _anim.SetFloat("horizontalDirection", Mathf.Abs(_horizontalDirection));
        }
        else
        {
            _anim.SetBool("isGrounded", false);
        }
        if (_isJumping)
        {
            _anim.SetBool("isJumping", true);
            _anim.SetBool("isFalling", false);
            _anim.SetBool("WallGrab", false);
            _anim.SetFloat("verticalDirection", 0f);
        }
        else
        {
            _anim.SetBool("isJumping", false);

            if (_wallGrab || _wallSlide)
            {
                _anim.SetBool("WallGrab", true);
                _anim.SetBool("isFalling", false);
                _anim.SetFloat("verticalDirection", 0f);
            }
            else if (_rb.velocity.y < 0f)
            {
                _anim.SetBool("isFalling", true);
                _anim.SetBool("WallGrab", false);
                _anim.SetFloat("verticalDirection", 0f);
            }
        }
    }

    //M?todo para activar la acci?n de muerte
    private void Death()
    {
        if (_rb.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazard")))
        {
            isAlive = false;
            _anim.SetBool("isAlive", false);
            _anim.SetBool("isFalling", false);
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    //M?todo para detectar las colisiones con el piso y las paredes
    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);
        
        _onWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer) ||
                    Physics2D.Raycast(transform.position, Vector2.left, _wallRaycastLength, _wallLayer);
        _onRightWall = Physics2D.Raycast(transform.position, Vector2.right, _wallRaycastLength, _wallLayer);
    }

    //Lineas que detectar?n el piso y las paredes
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);

        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * _wallRaycastLength);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _wallRaycastLength);
    }
}