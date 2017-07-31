using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float Speed;
    public Position Position, PrevPosition;
    public Position Direction;
    public bool Walking;
    public int Rows, Cols;

    private float _totaldx, _totaldy;
    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    public void UpdateDirection()
    {
        _anim.SetFloat("x", Direction.c);
        _anim.SetFloat("y", -Direction.r);
    }

    public void UpdatePosition()
    {
        if (Walking)
        {
            var dx = Direction.c * Speed * Time.deltaTime;
            var dy = -Direction.r * Speed * Time.deltaTime;
            transform.position += new Vector3(dx, dy, 0.0f);
            
            _anim.SetFloat("x", Direction.c);
            _anim.SetFloat("y", -Direction.r);
            _anim.SetBool("walking", true);

            _totaldx -= Mathf.Abs(dx);
            _totaldy -= Mathf.Abs(dy);

            if (_totaldx <= 0 && _totaldy <= 0)
            {
                Stop();
            }
        }
        else
        {
            _anim.SetBool("walking", false);
        }
    }

    public void Stop()
    {
        _totaldx = _totaldy = 0;
        Walking = false;

        // correct the world position
        transform.position = new Vector3(Position.c + 0.5f, Rows - Position.r - 0.5f, 0.0f);

        _anim.SetBool("walking", false);
    }

    public void GoForward()
    {
        var newPosition = Position + Direction;

        _totaldx = Mathf.Abs(newPosition.c - Position.c);
        _totaldy = Mathf.Abs(newPosition.r - Position.r);

        Position = newPosition;
        Walking = true;
    }

    public void TurnRight()
    {
        Direction = Position.TurnRight(Direction);
    }

    public void TurnLeft()
    {
        Direction = Position.TurnLeft(Direction);
    }
}

