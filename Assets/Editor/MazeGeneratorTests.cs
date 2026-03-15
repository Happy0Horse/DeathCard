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

    // 1. Правильний тест – перевірка значень розміру лабіринту
    [Test]
    public void Maze_Size_ShouldBePositive()
    {
        Assert.Greater(generator.width, 0);
        Assert.Greater(generator.height, 0);
    }

    // 2. Правильний тест – перевірка розміру клітинки
    [Test]
    public void CellSize_ShouldBeGreaterThanZero()
    {
        Assert.Greater(generator.cellSize, 0);
    }

    // 3. Неправильний тест (навмисно)
    // Очікує що ширина лабіринту дорівнює 5,
    // але в коді width = 10
    [Test]
    public void MazeWidth_ShouldBeFive()
    {
        Assert.AreEqual(5, generator.width);
    }
}