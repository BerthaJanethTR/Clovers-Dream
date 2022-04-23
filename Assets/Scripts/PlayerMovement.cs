using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Componentes de Clover
    [Header("Components")]
    private Rigidbody2D _rb;
    private Animator _anim;

    //Capa que se usará para reconocer el piso
    [Header("Layer Masks")]
    [SerializeField] private LayerMask _groundLayer;

    //Variables de movimiento de Clover
    [Header("Movement Variables")]
    [SerializeField] private float _movementAcceleration = 70f;
    [SerializeField] private float _maxMoveSpeed = 12f;
    [SerializeField] private float _groundLinearDrag = 7f;
    private float _horizontalDirection;
    private float _verticalDirection;
    private bool _changingDirection => (_rb.velocity.x > 0f && _horizontalDirection < 0f) || (_rb.velocity.x < 0f && _horizontalDirection > 0f);
    private bool _facingRight = true;
    bool isAlive = true;

    //Variables de salto de Clover
    [Header("Jump Variables")]
    [SerializeField] private float _jumpForce = 12f;
    [SerializeField] private float _airLinearDrag = 2.5f;
    [SerializeField] private float _fallMultiplier = 8f;
    [SerializeField] private float _lowJumpFallMultiplier = 5f;
    [SerializeField] private int _extraJumps = 1;
    [SerializeField] private float _hangTime = .1f;
    [SerializeField] private float _jumpBufferLength = .1f;
    private int _extraJumpsValue;
    private float _hangTimeCounter;
    private float _jumpBufferCounter;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpsValue > 0);

    //Variables para detectar la colisión con el piso
    [Header("Ground Collision Variables")]
    [SerializeField] private float _groundRaycastLength;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _onGround;

    //Inicializacion de los componentes de Clover
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
    }

    //Bucle que estará detectando cuando Clover se moverá y cuando activará el método de muerte
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
        Death();

        //Animaciones
        _anim.SetBool("isGrounded", _onGround);
        _anim.SetFloat("horizontalDirection", Mathf.Abs(_horizontalDirection));
        if (_horizontalDirection < 0f && _facingRight)
        {
            Flip();
        }
        else if (_horizontalDirection > 0f && !_facingRight)
        {
            Flip();
        }
        if (_rb.velocity.y < 0f)
        {
            _anim.SetBool("isJumping", false);
            _anim.SetBool("isFalling", true);
        }
    }

    //Bucle que hará los calculos para el correcto movimiento de Clover
    private void FixedUpdate()
    {
        CheckCollisions();
        MoveCharacter();
        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _extraJumpsValue = _extraJumps;
            _hangTimeCounter = _hangTime;

            //Animacion
            _anim.SetBool("isJumping", false);
            _anim.SetBool("isFalling", false);
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _hangTimeCounter -= Time.fixedDeltaTime;
        }
        if (_canJump) Jump();
    }

    //Método para control básico
    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    //Método que nos impedirá movernos si morimos
    private void MoveCharacter()
    {
        if (!isAlive) { return; }
        _rb.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rb.velocity.x) > _maxMoveSpeed)
            _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxMoveSpeed, _rb.velocity.y);
    }

    //Método que nos desacelerará al dejar de movernos o cambiar de dirección
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

    //Método para desacelerar al saltar
    private void ApplyAirLinearDrag()
    {
        _rb.drag = _airLinearDrag;
    }

    //Método para el salto
    private void Jump()
    {
        if (!isAlive) { return; }
        if (!_onGround)
            _extraJumpsValue--;

        ApplyAirLinearDrag();
        _rb.velocity = new Vector2(_rb.velocity.x, 0f);
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;

        //Animacion
        _anim.SetBool("isJumping", true);
        _anim.SetBool("isFalling", false);
    }

    //Método para calcular fuerza de gravedad entre saltos
    private void FallMultiplier()
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

    //Método para voltear el sprite de Clover
    void Flip()
    {
        _facingRight = !_facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    //Método para activar la acción de muerte
    private void Death()
    {
        if (_rb.IsTouchingLayers(LayerMask.GetMask("Enemy", "Hazard")))
        {
            isAlive = false;
            _anim.SetBool("isAlive", false);
        }
    }

    //Método para detectar las colisiones con el piso
    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
                    Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);
    }

    //Lineas que detectarán el piso
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);
    }
}