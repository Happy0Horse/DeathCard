using NUnit.Framework;
using UnityEngine;

public class MazeGeneratorTests
{
    GameObject mazeObject;
    MazeGenerator generator;

    [SetUp]
    public void Setup()
    {
        mazeObject = new GameObject();
        generator = mazeObject.AddComponent<MazeGenerator>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(mazeObject);
    }

    [Test]
    public void Maze_Size_ShouldBePositive()
    {
        TestContext.WriteLine("Executing: Maze_Size_ShouldBePositive");

        Assert.Greater(generator.width, 0, "Width should be positive");
        Assert.Greater(generator.height, 0, "Height should be positive");
    }
    [Test]
    public void CellSize_ShouldBeGreaterThanZero()
    {
        TestContext.WriteLine("Executing: CellSize_ShouldBeGreaterThanZero");

        Assert.Greater(generator.cellSize, 0, "Cell size should be greater than zero");
    }

    [Test]
    public void MazeWidth_ShouldBeFive()
    {
        TestContext.WriteLine("Executing: MazeWidth_ShouldBeFive");

        Assert.AreEqual(5, generator.width, "This test is intentionally failing because width is not 5");
    }
}