using EmpyrionScripting.Interface;

namespace EmpyrionScripting
{
    public class HarvestInfo : IHarvestInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DropOnHarvestId { get; set; }
        public string DropOnHarvestItem { get; set; }
        public int DropOnHarvestCount { get; set; }
        public int ChildOnHarvestId { get; set; }
        public string ChildOnHarvestItem { get; set; }
        public int PickupTargetId { get; set; }
    }
}