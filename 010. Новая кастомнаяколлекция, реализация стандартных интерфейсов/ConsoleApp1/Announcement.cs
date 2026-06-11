namespace ConsoleApp1
{
    public class Announcement : EventArgs
    {
        private ActionInSalvatory actionType;
        public ActionInSalvatory Action { get { return actionType; } }
        public Announcement(ActionInSalvatory arg)
        {
            actionType = arg;
        }
    }
}
