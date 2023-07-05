using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // hmm yes bewegen my beloved
    [Header("Movement")]
    private float mvSpeed;
    private float maxSpeed;
    public float walkSpeed;
    public float maxWalkSpeed;
    public float sprintSpeed;
    public float maxSprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float maxCrouchSpeed;
    public float crouchYScale;
    public bool toggleCrouch;
    private float startYScale;

    [Header("Gun Scale Fix")]
    public Transform playerGunTransform;
    public float gunCrouchZScale;
    private float gunStartZScale;

    // key bindings
    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    private bool crouchKeyPressed;

    // controle of de speler op de grond staat of niet
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;


    // het handelen van slopes
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    public float slopeSpeedMultiplier;
    public float maxSpeedOnSlopeMultiplier;
    private RaycastHit slopeHit;
    private bool exitingSlope;


    public Transform orientation;

    float currentSpeed;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


    // player state machine
    public movementState state;
    public enum movementState {
        walking,
        sprinting,
        crouching,
        air
    }


    private void Start() {
        // zorg dat de player niet omvalt (rotation freezen)
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;


        // sla de normale Yscale van de player op
        startYScale = transform.localScale.y;
        gunStartZScale = playerGunTransform.localScale.z;
    }


    private void Update() {
        // ground check d.m.v. een raycast naar beneden van de helft van de spelerhoogte plus 0.2
        // de layermask whatIsGround wordt gebruikt in unity om te kijken welke objects allemaal als
        // grond gezien worden en welke niet
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);


        MyInput();
        SpeedControl();
        StateHandler();

        // zorg dat de drag nul is als de player niet op de grond staat
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }


    private void FixedUpdate() {
        MovePlayer();

        // get current movement speed
        currentSpeed = rb.velocity.magnitude;
        //Debug.Log(currentSpeed);
    }



    private void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        // controleer of de speler op de grond staat en dat hij klaar is om te springen
        if (Input.GetKeyDown(jumpKey) && readyToJump && grounded) {
            
            readyToJump = false;

            Jump();

            // voer de funcite ResetJump na een bepaalde tijd uit (jumpCooldown om exact te zijn)
            Invoke(nameof(ResetJump), jumpCooldown);
        }


        // voor als toggleCrouch wel aan staat, zie de code in StateHandler()
        if (!toggleCrouch) {
            // start met crouchen
            if (Input.GetKeyDown(crouchKey)) {
                // maak de speler kleiner qua hoogte
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);

                // als je de speler kleiner maakt op de Y-as zorgt het er ook voor dat ie in de lucht gaat vliegen
                // daarom moeten we een kracht naar beneden maken die één keer wordt geactiveerd
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }


            // stop met crouchen
            if (Input.GetKeyUp(crouchKey)) {
                // maak de speler weer normaal
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
    }



    private void StateHandler() {
        // switch naar de crouching state als de crouch key ingedrukt is
        // als togglecrouch true is ga je crouchen als je de crouch key indrukt en weer stoppen met crouchen
        // als de crouch key weer opnieuw ingedrukt is (dus voor een tweede keer)


        // als togglecrouch false is ga je alleen crouchen als je de crouch key ingedrukt hebt
        if (toggleCrouch && grounded && Input.GetKeyDown(crouchKey)) {



            if (state == movementState.crouching) {
                // stop met crouchen
                state = movementState.walking;
                
                // maak de speler weer normaal
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
                playerGunTransform.localScale = new Vector3(playerGunTransform.localScale.x, playerGunTransform.localScale.y, gunStartZScale);

            }
            else {
                // begin met crouchen
                state = movementState.crouching;

                // maak de speler kleiner qua hoogte
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                playerGunTransform.localScale = new Vector3(playerGunTransform.localScale.x, playerGunTransform.localScale.y, gunCrouchZScale);
                
                

                // als je de speler kleiner maakt op de Y-as zorgt het er ook voor dat ie in de lucht gaat vliegen
                // daarom moeten we een kracht naar beneden maken die één keer wordt geactiveerd
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

                
            }




        } else if (grounded && !toggleCrouch) {
            // zet de state op walking, maar als de crouch key ingedrukt is, zet hem weer op crouching
            state = movementState.crouching;
            if (Input.GetKey(crouchKey)) state = movementState.crouching;
        }

        

        // switch naar sprinting state als je op de grond staat en sprint ingedrukt is
        if (grounded && Input.GetKey(sprintKey) && state != movementState.crouching) {
            state = movementState.sprinting;
            mvSpeed = sprintSpeed;
            maxSpeed = maxSprintSpeed;
        }


        // switch naar walking state als je op de grond staat en sprint niet ingedrukt is
        else if (grounded && state != movementState.crouching) {
            state = movementState.walking;
            mvSpeed = walkSpeed;
            maxSpeed = maxWalkSpeed;
        }


        // als je uiteindelijk niet op de grond staat, switch dan naar de air state
        else if (!grounded) {
            state = movementState.air;
        }



        if (state == movementState.crouching) {
            state = movementState.crouching;
            mvSpeed = crouchSpeed;
            maxSpeed = maxCrouchSpeed;
        }
    }



    private void MovePlayer() {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;


        // moving on a slope
        if (OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection() * mvSpeed * slopeSpeedMultiplier, ForceMode.Force);


            // voeg een kracht omlaag toe, zodat de speler op de slope blijft
            // dit voorkomt ook gelijk dat de speler in de lucht vliegt als het de slope af loopt.
            if (rb.velocity.y != 0) 
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            
            
        }


        // disable gravity when on a slope (zodat je er niet vanaf valt)
        // dit is geen probleem, want als je van de slope af gaat wordt de zwaartekracht
        // weer aangezet
        rb.useGravity = !OnSlope();


        if (grounded) {
            // forcemode.force zorgt ervoor dat de kracht de hele tijd toegepast kan worden
            rb.AddForce(moveDirection.normalized * mvSpeed * 10f, ForceMode.Force);
        } else {
            // in de lucht worden krachten met airMultiplier vermenigvuldigd, wat ervoor kan
            // zorgen dat je langzamer je direction kan aanpassen (als airMultiplier < 1)
            rb.AddForce(moveDirection.normalized * mvSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }



    private void SpeedControl() {
        // limit speed on slope
        if (OnSlope() && !exitingSlope) {
            if (rb.velocity.magnitude > maxSpeed * maxSpeedOnSlopeMultiplier)
                rb.velocity = rb.velocity.normalized * maxSpeed * maxSpeedOnSlopeMultiplier;
            

        // limit speed on ground/air
        } else {
            // get current horizontal velocity
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // if current velocity is higher than the maximum allowed movement speed
            if (flatVel.magnitude > maxSpeed) {
                // calculate maximum allowed velocity (by max speed)
                Vector3 limitedVelocity = flatVel.normalized * maxSpeed;

                // apply new limited velocity to player object
                rb.velocity = new Vector3(limitedVelocity.x, rb.velocity.y, limitedVelocity.z);
            }
        }
    }



    private void Jump() {
        exitingSlope = true;
        // reset y velocity
        // hierdoor spring je altijd even hoog
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // voeg een nieuwe kracht toe met forcemode op impulse, wat ervoor zorgt dat deze kracht
        // maar een keer wordt toegepast
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;

        exitingSlope = false;
    }






    private bool OnSlope() {
        // hier weer zoals eerst, een raycast van half spelerlengte plus 0.3, maar dan voor slopes
        // er wordt in plaats van een layermask een raycasthit variable gebruikt
        // de object die de raycast raakt wordt opgeslagen in slopeHit
        // dit omdat slopes verschillende angles kunnen hebben, dus we willen degene waarop we staan pinpointen
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {

            // meet de hoek tussen de normaal van de slope en een vector omhoog
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);

            // return true (staat er niet bij maar dat doet het wel)
            //als de angle kleiner is als de maxSlopeAngle en angle niet nul is
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }


    private Vector3 GetSlopeMoveDirection() {
        // hiermee wordt de move direction parallel gezet aan de slope angle
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
