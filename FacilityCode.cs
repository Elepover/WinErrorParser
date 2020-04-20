namespace WinErrorParser
{
    public class FacilityCode
    {
        private const string _prefix = "FACILITY_";
        public string ErrorCode { get; set; }
        public int Value { get; set; }
        public string FullErrorCode { get => _prefix + ErrorCode; }
        public FacilityCode() { }
        public FacilityCode(string name, int value)
        {
            ErrorCode = name;
            Value = value;
        }
    }
}
