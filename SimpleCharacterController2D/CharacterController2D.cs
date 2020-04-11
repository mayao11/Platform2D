using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    public float jumpForce = 400f;                          // 弹跳力
    public bool canAirControl = false;                      // 在空中时，是否能控制
    public LayerMask groundMask;                            // 定义哪一个Layer是地面
    public Transform m_GroundCheck;                         // 用于判定地面的空物体

    const float k_GroundedRadius = .1f; // 用于检测地面的小圆形的半径
    private bool m_Grounded;            // 当前是否在地面上
    private bool m_FacingRight = true;  // 玩家是否面朝右边
    private Vector3 m_Velocity = Vector3.zero;

    const float m_NextGroundCheckLag = 0.1f;    // 起跳后的一小段时间，不能再次起跳。防止连跳的一种解决方案
    float m_NextGroundCheckTime;            // 过了这个时间才可能落地、才能再次起跳

    // 这个角色控制器，是依靠刚体驱动的
    private Rigidbody2D m_Rigidbody2D;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;
    public UnityEvent OnAirEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();
        if (OnAirEvent == null)
            OnAirEvent = new UnityEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // 检测与地面的碰撞
        if (Time.time > m_NextGroundCheckTime)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, groundMask);
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].gameObject != gameObject)
                {
                    m_Grounded = true;
                    if (!wasGrounded)
                        OnLandEvent.Invoke();
                }
            }
        }

        if (wasGrounded && !m_Grounded)
        {
            OnAirEvent.Invoke();
        }
    }


    public void Move(float move, bool jump)
    {
        // 玩家在地面时，或者可以空中控制时，才能移动
        if (m_Grounded || canAirControl)
        {
            // 输入变量move决定横向速度
            m_Rigidbody2D.velocity = new Vector2(move, m_Rigidbody2D.velocity.y);

            // 面朝右时按左键，或面朝左时按右键，都会让角色水平翻转
            if (move > 0 && !m_FacingRight)
            {
                Flip();
            }
            else if (move < 0 && m_FacingRight)
            {
                Flip();
            }
        }

        // 在地面时按下跳跃键，就会跳跃
        if (m_Grounded && jump)
        {
            OnAirEvent.Invoke();
            m_Grounded = false;
            // 施加弹跳力
            m_Rigidbody2D.AddForce(new Vector2(0f, jumpForce));
            m_NextGroundCheckTime = Time.time + m_NextGroundCheckLag;
        }
    }


    private void Flip()
    {
        // true变false，false变true
        m_FacingRight = !m_FacingRight;

        // 缩放的x轴乘以-1，图片就水平翻转了
        transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
    }
}