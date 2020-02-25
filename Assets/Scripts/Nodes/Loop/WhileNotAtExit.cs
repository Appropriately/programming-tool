public class WhileNotAtExit : Loop
{
    public override Node NodeFunction() {
        if (controller.player.Tile() == MapController.END_TILE) {
            return child;
        } else {
            return (loop is null) ? this : loop;
        }
    }
}
