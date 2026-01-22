using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipController : MonoBehaviour
{
    public GameObject spaceshipDebris;
    public WeaponList weaponList;
    public Vector2 direction = Vector2.zero;
    PlayerInput playerInput;
    InputAction moveAction;
    InputAction shotAction;
    bool isColliding = false;

    public enum Weapon
    {
        Basic,
        Laser
    }

    public Weapon currentWeapon = Weapon.Basic;
    private GameObject weaponInstance;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        shotAction = playerInput.actions["Jump"];
    }

    private void Update()
    {
        if (!GameManager.IsPaused)
            Move();
    }

    public void Move()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float horizontalAxis = moveInput.x;
        float verticalAxis = moveInput.y;

        if (verticalAxis > 0.0f)
            Thrust(verticalAxis);
        if (Mathf.Abs(horizontalAxis) > 0.0f)
            Turn(horizontalAxis);
        if (shotAction.triggered)
            Shoot();

        transform.position += (Vector3)direction * Time.deltaTime * 4.0f;
        CalculatePositionOnCamera();
    }

    public void Thrust(float power)
    {
        direction = Vector2.Lerp(direction, transform.up * power, Time.deltaTime * 4.0f);
    }

    public void Turn(float delta)
    {
        transform.eulerAngles = new Vector3(0.0f, 0.0f, transform.eulerAngles.z - delta * 150.0f * Time.deltaTime);
        transform.GetChild(0).localEulerAngles = new Vector3(0.0f, -delta * 30.0f, 0.0f);
    }

    public void Shoot(bool onMobile = false)
    {
        // If Weapon is Basic, the Prefabs/Weapons/Projectile prefab is instantiated
        // If Weapon is Laser, the Prefabs/Weapons/Laser prefab is instantiated

        switch (currentWeapon)
        {
            case Weapon.Basic:
                ProjectileController projectile = Instantiate(weaponList.weapons[(int)currentWeapon].weaponPrefab, transform.position, Quaternion.identity).GetComponent<ProjectileController>();
                projectile.SetDirection(transform.up);
                break;
            case Weapon.Laser:
                if (weaponInstance != null)
                    break;
                weaponInstance = Instantiate(weaponList.weapons[(int)currentWeapon].weaponPrefab, transform.position, Quaternion.identity);
                weaponInstance.transform.up = transform.up;
                weaponInstance.transform.parent = transform;
                break;
            default:
                Debug.LogError("Invalid weapon state.");
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Preventing multiple collision triggers on the same frame
        if (isColliding)
            return;

        AsteroidController asteroidController = collision.gameObject.GetComponent<AsteroidController>();

        if (asteroidController)
        {
            asteroidController.Split();
            if (GameManager.instance != null)
                GameManager.instance.RespawnShip();
            Instantiate(spaceshipDebris, transform.position, transform.GetChild(0).rotation);
            Destroy(gameObject);
            isColliding = true;
        }
    }

    private void CalculatePositionOnCamera()
    {
        if (Camera.main == null)
            return;

        Vector2 positionOnCamera = Camera.main.WorldToViewportPoint(transform.position);
        Vector2 wrappedPosition = positionOnCamera;
        bool warped = false;

        if (positionOnCamera.x > 1.05f)
        {
            wrappedPosition.x = -0.05f;
            warped = true;
        }
        else if (positionOnCamera.x < -0.05f)
        {
            wrappedPosition.x = 1.05f;
            warped = true;
        }

        if (positionOnCamera.y > 1.05f)
        {
            wrappedPosition.y = -0.05f;
            warped = true;
        }
        else if (positionOnCamera.y < -0.05f)
        {
            wrappedPosition.y = 1.05f;
            warped = true;
        }

        if (warped)
        {
            transform.position = (Vector2)Camera.main.ViewportToWorldPoint(wrappedPosition);
            transform.GetChild(0).GetChild(0).GetComponent<EngineTrail>().ClearParticles();
        }
    }

    public void UpdateWeapon(int score)
    {
        Weapon weapon = Weapon.Basic;

        if (score >= 8000)
            weapon = Weapon.Laser;

        if (weapon == currentWeapon)
            return;

        currentWeapon = weapon;
    }
}
