using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Collision coll;
    [HideInInspector]
    public Rigidbody2D rb;
    private AnimationScript anim;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    // counter for jump charge time in the distinct style
    public float counter = 1;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool charging;

    [Space]

    private bool groundTouch;
    private bool hasDashed;

    public int side = 1;

    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;
    public AudioSource jumpSound1;
    public AudioSource dashSound1;

    [Space]
    [Header("Cameras")]
    [SerializeField] GameObject mainCam;
    [SerializeField] GameObject polishedCam;
    private Camera mc;
    private Camera pc;
    private ToggleMovement tm;

    // Start is called before the first frame update
    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();

        mc = mainCam.GetComponent<Camera>();
        pc = polishedCam.GetComponent<Camera>();
        tm = GetComponent<ToggleMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");

        // Makes controls feel less slippery - Em
        if (tm.isPolished() || tm.isDistinct()) {
            x = Input.GetAxis("HorizontalFixed");
        }

        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);


        Walk(dir);
        anim.SetHorizontalMovement(x, y, rb.velocity.y);

        // example of how to use toggle movement script - Gus
        if (GetComponent<ToggleMovement>().originalMovement == true){
            Debug.Log("Hi hi");
        }

        if (coll.onWall && Input.GetButton("Fire3") && canMove)
        {
            if(side != coll.wallSide)
                anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        
        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if(x > .2f || x < -.2f)
            rb.velocity = new Vector2(rb.velocity.x, 0);

            float speedModifier = y > 0 ? .5f : 1;

            rb.velocity = new Vector2(rb.velocity.x, y * (speed * speedModifier));
        }
        else
        {
            rb.gravityScale = 3;
        }

        if(coll.onWall && !coll.onGround)
        {
            // flip animation side - Gus
            if(coll.wallSide != side){
                anim.Flip(side * -1);
            }
            
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                // if polished movement is set to true - Gus
                if(tm.isPolished() || tm.isDistinct()) {
                    if ( rb.velocity.y < 0) {
                        WallSlide();
                    }
                } else { // else do original functionality - Gus
                    WallSlide();
                }
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        // Code for the chargeable jump in the distinct movement
        // To charge a jump, hold the space bar - Em
        if (tm.isDistinct()){
            if (Input.GetKey(KeyCode.Space)){
                counter += Time.deltaTime;
            }
            if (coll.onGround){
                if (Input.GetKeyUp(KeyCode.Space)){
                    anim.SetTrigger("jump");
                    jumpSound1.Play();
                    if (counter >= 2f){
                        counter = 2f;
                    }
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce * 1.25f * counter);
                    counter = 1;
                }
                    
            }
            if (coll.onWall && !coll.onGround){
                if (Input.GetButtonDown("Jump")){
                    WallJump();
                    counter = 1;
                }
            }            
        }
        // Original Jump code
        else{
            if (Input.GetButtonDown("Jump"))
            {
                anim.SetTrigger("jump");

                if (coll.onGround)
                    Jump(Vector2.up, false);
                if (coll.onWall && !coll.onGround)
                    WallJump();
            }
        }

        if (Input.GetButtonDown("Fire1") && !hasDashed)
        {
            // Allows the player to perform a horizontal dash in whichever direction they 
            // are facing if they press the dash button while not holding a direction - Em
            if(tm.isPolished() || tm.isDistinct()){
                if(xRaw != 0 || yRaw != 0)
                    Dash(xRaw, yRaw);
                else if (xRaw == 0 && yRaw == 0){
                    if (side == 1){
                        Dash(1, 0);
                    }
                    else if (side == -1){
                        Dash(-1, 0);
                    }
                }
            }
            // if the toggle is off, runs the original code
            else{
                if(xRaw != 0 || yRaw != 0)
                    Dash(xRaw, yRaw);
            }

        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if(!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if(x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }


    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        // SCREEN SHAKE - Justin
        if (tm.isPolished() || tm.isDistinct())
        {
            pc.transform.DOComplete();
            pc.transform.DOShakePosition(.2f, .15f, 14, 90, false, true);
            FindObjectOfType<RippleEffect>().Emit(pc.WorldToViewportPoint(transform.position));
        }
        else
        {
            mc.transform.DOComplete();
            mc.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
            FindObjectOfType<RippleEffect>().Emit(mc.WorldToViewportPoint(transform.position));
        }

        /*Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));*/

        hasDashed = true;

        anim.SetTrigger("dash");

        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);

        rb.velocity += dir.normalized * dashSpeed;
        StartCoroutine(DashWait());

        if (tm.isPolished() || tm.isDistinct()) {
            dashSound1.Play();
        }
    }

    IEnumerator DashWait()
    {
        FindObjectOfType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());

        // Dash goes slightly farther in polished movement - Em
        if(tm.isPolished()){
            DOVirtual.Float(11, 0, .8f, RigidbodyDrag);
        }
        // Dash doesn't go as far in distinct movement - Em
        else if (tm.isDistinct()){
            DOVirtual.Float(16, 0, .8f, RigidbodyDrag);
        }
        // Original code
        else{
            DOVirtual.Float(14, 0, .8f, RigidbodyDrag);
        }

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

        // Implementation of Vivian Zheng's Bigger Wall Jump
        if (tm.isPolished() || tm.isDistinct()){
            Jump((Vector2.up / 1.25f + wallDir / 1.25f), true);
            // Justin's implementation of tighter control and velocity after wall jump
            rb.velocity *= new Vector2(0.75f, 1.4f);
        } 
        // Original Code
        else{
            Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);
        }
        wallJumped = true;
    }

    private void WallSlide()
    {
        if(coll.wallSide != side)
         anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;

        if (wallGrab)
            return;

        if (!wallJumped)
        {
            // Walk Speed is slower for distinct movement - Em
            if (tm.isDistinct()){
                rb.velocity = new Vector2(dir.x * 5, rb.velocity.y);
            }
            else{
                rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
            }  
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, (new Vector2(dir.x * speed, rb.velocity.y)), wallJumpLerp * Time.deltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.velocity = new Vector2(rb.velocity.x, 0);
        // Distinct Movement has a heavier jump
        if (tm.isDistinct()){
            rb.velocity += dir * 9;
        }
        else{
            rb.velocity += dir * jumpForce;
        }
        // Polished and Distinct Movement plays a jump sound
        if (tm.isPolished() || tm.isDistinct()) {
            jumpSound1.Play();
        }

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.drag = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
