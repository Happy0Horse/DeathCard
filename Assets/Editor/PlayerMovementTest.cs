using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementTests
{
    GameObject player;
    PlayerMovement movement;

    [SetUp]
    public void Setup()
    {
        TestContext.WriteLine("Setup: Creating player object and adding components");

        player = new GameObject();
        player.AddComponent<CharacterController>();
        movement = player.AddComponent<PlayerMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        TestContext.WriteLine("TearDown: Destroying player object");

        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerMovement_ComponentExists()
    {
        TestContext.WriteLine("Executing: PlayerMovement_ComponentExists");

        Assert.IsNotNull(movement);
    }

    [Test]
    public void SprintSpeed_IsGreaterThanNormalSpeed()
    {
        TestContext.WriteLine("Executing: SprintSpeed_IsGreaterThanNormalSpeed");

        Assert.Greater(movement.sprintSpeed, movement.speed);
    }

    [Test]
    public void SprintSpeed_ShouldBeLessThanNormalSpeed()
    {
        TestContext.WriteLine("Executing: SprintSpeed_ShouldBeLessThanNormalSpeed (Expected to FAIL)");

        Assert.Less(movement.sprintSpeed, movement.speed);
    }
}