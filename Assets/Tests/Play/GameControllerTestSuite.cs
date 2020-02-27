using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class GameControllerTestSuite
    {
        [SetUp]
        public void Setup()
        {
            LevelManager.Clear();
            int id = LevelManager.Add("level_test", "XEX\nXOX\nXSX", new Block[]{Block.Move});
            LevelManager.Load(id);
        }

        [UnityTest, Description("A newly created scene should not have more than one node, the starting Node")]
        public IEnumerator CountNumberOfNodesAtLevelStart_Returns1()
        {
            yield return null;
            GameController ctrl = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            Assert.IsTrue(ctrl.NodeCount == 1, "There should only be one node in an empty solution.");
        }

        [UnityTest, Description("A newly created scene should be stopped and not running")]
        public IEnumerator CheckStateOfFreshLevel_ReturnsStateStopped()
        {
            yield return null;
            GameController ctrl = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            Assert.IsTrue(ctrl.IsStopped, "A newly created level should be stopped.");
        }

        [UnityTest, Description("Player should start on a START_TILE")]
        public IEnumerator CheckTheTileAtThePlayersFeet_ReturnsStartTile()
        {
            yield return null;
            GameController ctrl = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            Assert.IsTrue(ctrl.player.Tile() == MapController.START_TILE);
        }
    }
}