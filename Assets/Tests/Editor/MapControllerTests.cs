using NUnit.Framework;

namespace Tests
{
    public class MapControllerTests
    {
        private static MapController controller = new MapController();

        [SetUp]
        public void Setup() => controller.Create("SO\nXE");

        // -------------------------------------------------------------------------------------------------------- //
        // Testing various techniques for generating the map
        // -------------------------------------------------------------------------------------------------------- //

        [Test]
        public void MapWithCorectString_ReturnsTrue() => Assert.IsTrue(controller.Create("SO\nXE"));

        [Test, Description("A correct map should be of size nxn where n is an arbitrary positive integer")]
        public void MapWithIncorrectWidthAndHeight_ReturnsFalse() => Assert.IsFalse(controller.Create("XXXXXX\nXX"));

        [Test, Description("A map should at least have one start block")]
        public void MapWithNoStart_ReturnsFalse() => Assert.IsFalse(controller.Create("OO\nOE"));

        [Test, Description("A map should have only one start block")]
        public void MapWitTwoStartBlocks_ReturnsFalse() => Assert.IsFalse(controller.Create("SS\nOO"));

        [Test, Description("A map should at least have one end block")]
        public void MapWithNoEnd_ReturnsFalse() => Assert.IsFalse(controller.Create("OO\nOS"));

        // -------------------------------------------------------------------------------------------------------- //
        // Testing map traversable
        // -------------------------------------------------------------------------------------------------------- //

        [Test, Description("Test generating a valid map then traversing a valid location")]
        public void CorrectMapTraversal_ReturnsTrue() => Assert.IsTrue(controller.IsTraversable(1, 0));

        [Test, Description("Test generating a valid map then traversing an invalid location")]
        public void IncorrectMapTraversal_ReturnsFalse() => Assert.IsFalse(controller.IsTraversable(0, 0));

        [Test, Description("Test generating a valid map then attempting to access a region outside the array")]
        public void OutOfBoundsMapTraversal_ReturnsFalse() => Assert.IsFalse(controller.IsTraversable(-1, -1));

        // -------------------------------------------------------------------------------------------------------- //
        // Miscellaneous map testing
        // -------------------------------------------------------------------------------------------------------- //

        [Test, Description("The returned item from an array should be of type Char")]
        public void GetCharacterFromMap_ReturnsTypeChar() => Assert.IsInstanceOf<char>(controller.map[0,0]);

        [Test, Description("The returned character from a particular map position should be E")]
        public void GetCharacterFromMap_ReturnsE() => Assert.AreEqual('E', controller.map[1,0]);
    }
}
