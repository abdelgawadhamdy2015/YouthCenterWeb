namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class Activity
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}