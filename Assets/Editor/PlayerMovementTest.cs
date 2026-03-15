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
        player = new GameObject();
        player.AddComponent<CharacterController>();
        movement = player.AddComponent<PlayerMovement>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerMovement_ComponentExists()
    {
        Assert.IsNotNull(movement);
    }

    [Test]
    public void SprintSpeed_IsGreaterThanNormalSpeed()
    {
        Assert.Greater(movement.sprintSpeed, movement.speed);
    }

    [Test]
    public void SprintSpeed_ShouldBeLessThanNormalSpeed()
    {
        Assert.Less(movement.sprintSpeed, movement.speed);
    }
}