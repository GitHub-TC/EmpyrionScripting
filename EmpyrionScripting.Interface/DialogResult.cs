namespace EmpyrionScripting.Interface
{
    public class DialogResult
    {
        public int ButtonIdx { get; set; }
        public string Link { get; set; }
        public string PlayerInput { get; set; }
        public IPlayerData Player { get; set; }
        public int DialogData { get; set; }
    }
}
