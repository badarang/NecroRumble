using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows;

public class Mover : MonoBehaviour
{
	public bool _canMove = true;
    public bool CanMove
    {
        get { return _canMove; }
        set { _canMove = value; }
    }


    [Header("�ӵ� ����")]
    public float speed;
    public float accel;
    public Vector2 direction;
    public Vector2 angularAccel;

    protected Rigidbody2D rigid;

    public Vector2 lastInput = new Vector2(0,-1);


    private void FixedUpdate()
    {
        if (direction != Vector2.zero)
        {
            lastInput = direction;
        }
        BeforeMove();
        if (CanMove)
        {
            Move();
            OnMove();
            CalculateDelta();
        }
        AfterMove();
    }

    public void Move()
    {
        if (rigid == null)
        {
            rigid = GetComponent<Rigidbody2D>();
        }
        // rigid.MovePosition(rigid.position + speed * Time.fixedDeltaTime * direction.normalized);
        //rigid.velocity = direction.normalized * speed;
    }

    public void CalculateDelta()
    {
        //�ӵ�, ���ӵ�, ���ӵ� ���
        speed += accel * Time.fixedDeltaTime;
        direction += angularAccel * Time.fixedDeltaTime;
    }

    public virtual void BeforeMove() { }
    public virtual void OnMove() { }
    public virtual void AfterMove() { }
}
