using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.TestTools;

public class InputSystemTests : InputTestFixture
{
    GameObject spaceshipPrefab;
    GameObject projectilePrefab;
    GameObject cameraPrefab;
    GameObject spaceshipObject;
    GameObject cameraObject;
    SpaceshipController spaceshipController;
    Keyboard _keyboard;
    PlayerInput playerInput;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;

        GameManager.InitializeTestingEnvironment(true, false, true, false, false);

        spaceshipPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().spaceshipPrefab;
        projectilePrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().projectilePrefab;
        cameraPrefab = ((GameObject)Resources.Load("TestsReferences", typeof(GameObject))).GetComponent<TestsReferences>().cameraPrefab;

        cameraObject = Object.Instantiate(cameraPrefab);
        spaceshipObject = Object.Instantiate(spaceshipPrefab);
        spaceshipController = spaceshipObject.GetComponent<SpaceshipController>();
        playerInput = spaceshipObject.GetComponent<PlayerInput>();

        _keyboard = InputSystem.AddDevice<Keyboard>();
    }

    [UnityTest]
    public IEnumerator TestSpacebarShooting()
    {
        // Press spacebar to shoot
        Press(_keyboard.spaceKey);
        yield return new WaitForSeconds(0.1f);

        ProjectileController projectile = Object.FindAnyObjectByType<ProjectileController>();
        Assert.IsTrue(projectile != null, "Projectile should be instantiated when spacebar is pressed.");

        Release(_keyboard.spaceKey);
    }

    [UnityTest]
    public IEnumerator TestUpArrowMovement()
    {
        // Press up arrow to move forward
        Press(_keyboard.upArrowKey);
        yield return new WaitForSeconds(0.1f);
        Vector2 moveValue = playerInput.actions["Move"].ReadValue<Vector2>();
        Assert.IsTrue(moveValue.y > 0, "Move action Y value should be positive when pressing Up Arrow");
        Release(_keyboard.upArrowKey);
    }

    [UnityTest]
    public IEnumerator TestLeftArrowMovement()
    {
        // Press left arrow to turn left
        Press(_keyboard.leftArrowKey);
        yield return new WaitForSeconds(0.1f);
        Vector2 moveValue = playerInput.actions["Move"].ReadValue<Vector2>();
        Assert.IsTrue(moveValue.x < 0, "Move action X value should be negative when pressing Left Arrow");
        Release(_keyboard.leftArrowKey);
    }

    [UnityTest]
    public IEnumerator TestRightArrowMovement()
    {
        // Press right arrow to turn right
        Press(_keyboard.rightArrowKey);
        yield return new WaitForSeconds(0.1f);
        Vector2 moveValue = playerInput.actions["Move"].ReadValue<Vector2>();
        Assert.IsTrue(moveValue.x > 0, "Move action X value should be positive when pressing Right Arrow");
        Release(_keyboard.rightArrowKey);
    }
}
