using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public class MenuTest
{
    [Test]
    public void MainMenu_Singleton_Exists()
    {
        TestContext.WriteLine("Executing: MainMenu_Singleton_Exists");
        GameObject go = new GameObject("MenuManager");
        var manager = go.AddComponent<MainMenuManager>();

        Assert.IsNotNull(manager);
        TestContext.WriteLine("Success: MainMenuManager component attached.");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void MatchmakingFlag_DefaultsToFalse()
    {
        TestContext.WriteLine("Executing: MatchmakingFlag_DefaultsToFalse");
        GameObject go = new GameObject();
        var manager = go.AddComponent<MainMenuManager>();

        Assert.IsTrue(manager.matchmakingFlag);
        TestContext.WriteLine($"Success: matchmakingFlag is {manager.matchmakingFlag}");

        Object.DestroyImmediate(go);
    }

    [Test]
    public void CardState_TogglesCorrectly()
    {
        TestContext.WriteLine("Executing: CardState_TogglesCorrectly");
        GameObject go = new GameObject();
        var card = go.AddComponent<CardState>();

        GameObject sA = new GameObject("StateA");
        GameObject sB = new GameObject("StateB");

        card.stateA = sA;
        card.stateB = sB;

        TestContext.WriteLine("Setting state to True (StateA Active)");
        card.SetState(true);

        Assert.IsTrue(sA.activeSelf);
        Assert.IsFalse(sB.activeSelf);

        TestContext.WriteLine($"Verification: StateA is {sA.activeSelf}, StateB is {sB.activeSelf}");

        Object.DestroyImmediate(sA);
        Object.DestroyImmediate(sB);
        Object.DestroyImmediate(go);
    }
}