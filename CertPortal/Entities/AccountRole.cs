namespace CertPortal.Entities
{
    public class AccountRole
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int RoleId { get; set; }
        public virtual Account Account { get; set; }
        public virtual Role Role { get; set; }
    }
}