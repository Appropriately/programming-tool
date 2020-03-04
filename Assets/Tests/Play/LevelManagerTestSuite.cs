using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class LevelManagerTestSuite
    {
        [SetUp]
        public void Setup()
        {
            LevelManager.Clear();
            LevelManager.Seed();
            SceneManager.LoadScene("Menu");
        }

        // -------------------------------------------------------------------------------------------------------- //
        // Test the setup procedure and ensure scene management is working as expected
        // -------------------------------------------------------------------------------------------------------- //

        [Test]
        public void LevelsSuccessfullySeeded_ReturnsTrue()
        {
            Assert.IsTrue(LevelManager.GetLevels().Count > 0);
        }

        [Test]
        public void CompareOpenedLevelNameWithExpectedValue_ReturnsTrue()
        {
            LevelManager.Load(1);
            Assert.IsTrue(LevelManager.GetName() == LevelManager.GetNameForID(1));
        }

        [Test]
        public void TestAddingACustomLevelAndLoadingIt_ReturnsTrue()
        {
            string name = "test";
            int id = LevelManager.Add(name, "XEX\nXOX\nXSX", new Block[]{Block.Move});
            LevelManager.Load(id);
            Assert.IsTrue(LevelManager.GetName() == LevelManager.GetNameForID(id));
        }
    }
}
